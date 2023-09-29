﻿using System.ComponentModel.DataAnnotations;

namespace JuanApp.Models;
public class Category : BaseEntity
{
    [StringLength(50)]
    public string Name {get ; set; }
}