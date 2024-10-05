namespace ResetPassword.Models
{
    public class PasswordResetToken
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string ResetToken { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool Used { get; set; }
    }
}