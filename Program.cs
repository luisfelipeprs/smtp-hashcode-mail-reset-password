using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResetPassword.Data;
using ResetPassword.Models;
using System.Net;
using System.Net.Mail;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Adicionar o contexto do banco de dados
builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    var serverVersion = new MySqlServerVersion(new Version(8, 0, 39));
    options.UseMySql(connectionString, serverVersion);
});

// Construir o aplicativo
var app = builder.Build();

// Endpoint para redefini��o de password
app.MapPost("/api/reset-password", async ([FromForm] string email, AppDbContext dbContext) =>
{
    if (string.IsNullOrEmpty(email))
    {
        return Results.BadRequest(new { message = "Email � necess�rio" });
    }

    var user = dbContext.User.FirstOrDefault(u => u.Email == email);

    if (user != null)
    {
        try
        {
            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587, //  this is the port for encrypted email transmissions using SMTP Secure (SMTPS)
                Credentials = new NetworkCredential("your-mail-here", "your-token-here"),
                EnableSsl = true,
            };

            // Criar o c�digo de redefini��o
            Random random = new Random();
            string codeResetPassword = random.Next(0, 1000000).ToString("D6");

            // Armazenar no banco de dados com data de expira��o
            var resetToken = new PasswordResetToken
            {
                UserId = user.Id,
                ResetToken = codeResetPassword,
                ExpiresAt = DateTime.Now.AddMinutes(15),
                Used = false
            };

            dbContext.PasswordResetToken.Add(resetToken);
            await dbContext.SaveChangesAsync();

            // Enviar o e-mail
            var mailMessage = new MailMessage
            {
                From = new MailAddress("lipezeraxz@gmail.com"),
                Subject = "Redefini��o de Password",
                Body = $"<p>Ol� {user.Name}, o c�digo para redefinir sua password �: <br><br><strong>{codeResetPassword}</strong></p>",
                IsBodyHtml = true,
            };

            mailMessage.To.Add(user.Email);
            smtpClient.Send(mailMessage);

            return Results.Ok(new { message = "E-mail de redefini��o enviado com sucesso." });
        }
        catch (Exception ex)
        {
            return Results.Problem(detail: ex.Message, statusCode: 500, title: "Erro ao enviar o e-mail.");
        }
    }
    else
    {
        return Results.NotFound(new { message = "Usu�rio n�o encontrado" });
    }
}).DisableAntiforgery().WithTags("Autentica��o");

// Endpoint para confirmar redefini��o de password
app.MapPost("/api/confirm-reset", async ([FromForm] string email, [FromForm] string token, [FromForm] string newPassword, AppDbContext dbContext) =>
{
    var user = dbContext.User.FirstOrDefault(u => u.Email == email);
    if (user == null)
    {
        return Results.NotFound(new { message = "Usu�rio n�o encontrado" });
    }

    var resetToken = dbContext.PasswordResetToken
        .FirstOrDefault(rt => rt.UserId == user.Id && rt.ResetToken == token && !rt.Used);

    if (resetToken == null || resetToken.ExpiresAt < DateTime.Now)
    {
        return Results.BadRequest(new { message = "Token inv�lido ou expirado." });
    }

    user.Password = newPassword;
    resetToken.Used = true;

    await dbContext.SaveChangesAsync();

    return Results.Ok(new { message = "Password alterada com sucesso." });
}).DisableAntiforgery().WithTags("Autentica��o");

// Endpoints CRUD para Usu�rios
app.MapGet("/api/users", async (AppDbContext dbContext) =>
{
    var users = await dbContext.User.ToListAsync();
    return Results.Ok(users);
}).WithTags("Usu�rios");

app.MapGet("/api/users/{id}", async (int id, AppDbContext dbContext) =>
{
    var user = await dbContext.User.FindAsync(id);
    if (user == null) return Results.NotFound();
    return Results.Ok(user);
}).WithTags("Usu�rios");

app.MapPost("/api/users", async ([FromBody] UserModel newUser, AppDbContext dbContext) =>
{
    if (newUser == null || string.IsNullOrEmpty(newUser.Name) || string.IsNullOrEmpty(newUser.Email) || string.IsNullOrEmpty(newUser.Password))
    {
        return Results.BadRequest(new { message = "Name, Email e Password s�o necess�rios." });
    }

    var existingUser = await dbContext.User.FirstOrDefaultAsync(u => u.Email == newUser.Email);
    if (existingUser != null)
    {
        return Results.BadRequest(new { message = "Este email j� est� em uso." });
    }

    dbContext.User.Add(newUser);
    await dbContext.SaveChangesAsync();

    return Results.Created($"/api/users/{newUser.Id}", new { message = "Usu�rio criado com sucesso.", userId = newUser.Id });
}).WithTags("Usu�rios");

app.MapPut("/api/users/{id}", async (int id, [FromBody] UserModel updatedUser, AppDbContext dbContext) =>
{
    var user = await dbContext.User.FindAsync(id);
    if (user == null) return Results.NotFound();

    user.Name = updatedUser.Name;
    user.Email = updatedUser.Email;
    user.Password = updatedUser.Password;

    await dbContext.SaveChangesAsync();
    return Results.Ok(user);
}).WithTags("Usu�rios");

app.MapDelete("/api/users/{id}", async (int id, AppDbContext dbContext) =>
{
    var user = await dbContext.User.FindAsync(id);
    if (user == null) return Results.NotFound();

    dbContext.User.Remove(user);
    await dbContext.SaveChangesAsync();
    return Results.Ok(new { message = "Usu�rio deletado com sucesso" });
}).WithTags("Usu�rios");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
