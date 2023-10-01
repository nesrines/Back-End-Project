using JuanApp.DataAccessLayer;
using JuanApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JuanApp.ViewComponents;
public class SliderViewComponent : ViewComponent
{
    private readonly AppDbContext _context;
    public SliderViewComponent(AppDbContext context) { _context = context; }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        IEnumerable<Slider> sliders = await _context.Sliders.Where(s => !s.IsDeleted).ToListAsync();
        return View(sliders);
    }
}