﻿using JuanApp.ViewModels.BasketVMs;

namespace JuanApp.Services.Interfaces;
public interface ILayoutService
{
    Task<List<BasketVM>> GetBasketAsync();
    Task<Dictionary<string, string>> GetSettingsAsync();
}