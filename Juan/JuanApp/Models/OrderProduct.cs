namespace JuanApp.Models;
public class OrderProduct : BaseEntity
{
    [StringLength(100)]
    public string Title { get; set; }
    [Column(TypeName = "smallmoney"), Display(Name = "Price ($)")]
    public double Price { get; set; }
    public int Count { get; set; }

    public int? ProductId { get; set; }
    public Product? Product { get; set; }
    public int? OrderId { get; set; }
    public Order? Order { get; set; }
}