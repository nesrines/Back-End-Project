using System.ComponentModel.DataAnnotations;

namespace JuanApp.Models;
public class Slider : BaseEntity
{
    [StringLength(255)]
    public string? BackgroundImage { get; set; }
    [StringLength(50)]
    public string? Subtitle { get; set; }
    [StringLength(50)]
    public string? Title { get; set; }
    [StringLength(255)]
    public string? Description { get; set; }
    [StringLength(255)]
    public string? ButtonLink { get; set; }
    [StringLength(20)]
    public string? ButtonText { get; set; }
}