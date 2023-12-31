﻿namespace JuanApp.Models;
public class Category : BaseEntity
{
    [StringLength(50)]
    public string Name {get ; set; }

    public IEnumerable<Product>? Products { get; set; }
}