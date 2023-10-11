namespace JuanApp.ViewModels.AccountVMs;
public class LoginVM
{
    public string Login { get; set; }
    [DataType(DataType.Password)]
    public string Password { get; set; }
    public bool RememberMe { get; set; }
}