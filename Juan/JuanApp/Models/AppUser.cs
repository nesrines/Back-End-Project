namespace JuanApp.Models;
public class AppUser : IdentityUser
{
    [StringLength(100)]
    public string? Name { get; set; }
    [StringLength(100)]
    public string? Surname { get; set; }
    public bool IsActive { get; set; }
    public IEnumerable<Address>? Addresses { get; set; }

    [NotMapped]
    public IEnumerable<string> Roles { get; set; }

    public IEnumerable<BasketProduct>? BasketProducts { get; set; }
    public IEnumerable<Order>? Orders { get; set; }
}