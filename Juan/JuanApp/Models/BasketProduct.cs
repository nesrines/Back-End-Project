﻿namespace JuanApp.Models;
public class BasketProduct : BaseEntity
{
    public int Count { get; set; }

    public int? ProductId { get; set; }
    public Product? Product { get; set; }
    public string? UserId { get; set; }
    public AppUser? User { get; set; }
}