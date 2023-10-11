namespace JuanApp.Models;
public class Order : BaseEntity
{
    public int No { get; set; }
    public int Status { get; set; }
    [StringLength(255)]
    public string? Comment { get; set; }
    [StringLength(100)]
    public string Name { get; set; }
    [StringLength(100)]
    public string Surname { get; set; }
    [StringLength(255), EmailAddress]
    public string Email { get; set; }
    [StringLength(255)]
    public string Country { get; set; }
    [StringLength(255)]
    public string TownCity { get; set; }
    [StringLength(255)]
    public string Line1 { get; set; }
    [StringLength(255)]
    public string Line2 { get; set; }
    [StringLength(255)]
    public string ZipCode { get; set; }

    public string? UserId { get; set; }
    public AppUser? User { get; set; }

    public IEnumerable<OrderProduct>? OrderProducts { get; set; }
}