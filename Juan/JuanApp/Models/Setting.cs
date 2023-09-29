using System.ComponentModel.DataAnnotations;

namespace JuanApp.Models;
public class Setting
{
    public int Id { get; set; }
    [StringLength(50)]
    public string Key { get; set; }
    [StringLength(1000)]
    public string Value { get; set; }
}