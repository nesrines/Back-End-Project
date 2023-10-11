namespace JuanApp.ViewModels.AccountVMs;
public class RegisterVM
{
    [StringLength(100)]
    public string Name { get; set; }
    [StringLength(100)]
    public string Surname { get; set; }
    public string UserName { get; set; }
    [EmailAddress]
    public string Email { get; set; }
    [DataType(DataType.Password)]
    public string Password { get; set; }
    [DataType(DataType.Password), Compare(nameof(Password))]
    public string ConfirmPassword { get; set; }
}