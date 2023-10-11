namespace JuanApp.Models;
public class Address : BaseEntity
{
    [StringLength(255)]
    public string? Country { get; set; }
    [StringLength(255)]
    public string? TownCity { get; set; }
    [StringLength(255)]
    public string? Line1 { get; set; }
    [StringLength(255)]
    public string? Line2 { get; set; }
    [StringLength(255)]
    public string? ZipCode { get; set; }
    public bool IsDefault { get; set; }

    public string? UserId { get; set; }
    public AppUser? User { get; set; }
}