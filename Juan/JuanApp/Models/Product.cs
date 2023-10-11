﻿namespace JuanApp.Models;
public class Product : BaseEntity
{
    [StringLength(100)]
    public string Title { get; set; }
    [Column(TypeName = "smallmoney"), Display(Name = "Price ($)")]
    public double Price { get; set; }
    [Display(Name = "Discount (%)")]
    public byte Discount { get ; set; }
    public int Count { get; set; }
    public bool IsFeatured { get; set; }
    [StringLength(255)]
    public string? MainImage { get; set; }
    [StringLength(255)]
    public string? SmallDesc { get; set; }
    public string? FullDesc { get; set; }

    [Display(Name = "Category")]
    public int? CategoryId { get; set; }
    public Category? Category { get; set; }

    public List<ProductImage>? ProductImages { get; set; }

    [NotMapped, Display(Name = "Main Image"), MaxFileSize(100), FileTypes("image/jpeg", "image/png")]
    public IFormFile? MainFile { get; set; }

    [NotMapped, MaxFileSize(100), FileTypes("image/jpeg", "image/png")]
    public IEnumerable<IFormFile>? Images { get; set; }

    public IEnumerable<BasketProduct>? BasketProducts { get; set; }
    public IEnumerable<OrderProduct>? OrderProducts { get; set; }
}