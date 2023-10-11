\namespace JuanApp.ViewModels.AccountVMs;
public class ForgotPasswordVM
{
    [EmailAddress]
    public string Email { get; set; }
}