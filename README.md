# Projeto de Redefinição de Senha com SMTP em .NET

Este projeto em .NET implementa uma funcionalidade de redefinição de senha usando SMTP para enviar um código de verificação por e-mail e um hashcode para confirmar a alteração da senha.

## Tecnologias Utilizadas

- .NET
- Entity Framework
- SMTP

## Funcionalidades
1. **CRUD de Usuários**: Permite criar, atualizar, deletar e pegar informações de usuários.
2. **Enviar Código de Redefinição**: O sistema envia um código de verificação para o e-mail do usuário utilizando SMTP.
3. **Confirmar Alteração de Senha**: Após receber o código de verificação, o sistema usa um hashcode para confirmar a alteração da senha.

## Como Rodar o Projeto

1. Clone o repositório:

    ```bash
    git clone https://github.com/luisfelipeprs/smtp-hashcode-mail-reset-password.git
    cd smtp-hashcode-mail-reset-password
    ```

2. Instale as dependências:

    ```bash
    dotnet restore
    ```

3. Configure suas credenciais

    ```bash
    Port = sua-porta, // use a porta 587 para ssmtp 
    Credentials = new NetworkCredential("seu-email-aqui", "seu-token-de-acesso-aqui"),
    EnableSsl = true // defina o SSL
    ```

4. Rode o projeto:

    ```bash
    dotnet run
    ```

5. Aplique as migrações para o banco de dados:

    ```bash
    dotnet ef database update
    ```
