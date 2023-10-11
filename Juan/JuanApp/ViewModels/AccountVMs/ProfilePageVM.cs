namespace JuanApp.ViewModels.AccountVMs;
public class ProfilePageVM
{
    public ProfileVM? ProfileVM { get; set; }
    public IEnumerable<Address>? Addresses { get; set; }
    public Address? Address { get; set; }
}