using System.ComponentModel.DataAnnotations;

namespace JuanApp.Models;
public class BaseEntity
{
    public int Id { get; set; }
    public bool IsDeleted { get; set; }
    [StringLength(100)]
    public string CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; }
    [StringLength(100)]
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedDate { get; set; }
    [StringLength(100)]
    public string? DeletedBy { get; set; }
    public DateTime? DeletedDate { get; set; }
}