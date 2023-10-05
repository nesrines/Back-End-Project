﻿using System.ComponentModel.DataAnnotations;

namespace JuanApp.Areas.Manage.ViewModels.AccountVMs;
public class RegisterVM
{
    public string UserName { get; set; }
    [EmailAddress]
    public string Email { get; set; }
    [DataType(DataType.Password)]
    public string Password { get; set; }
    [DataType(DataType.Password)]
    [Compare("Password")]
    public string ConfirmPassword { get; set; }
}