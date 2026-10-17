using System.ComponentModel.DataAnnotations;

namespace AemenersolAssessment.Models;

public class Platform
{
    [Key]
    public int Id { get; set; }

    [MaxLength(200)]
    public string UniqueName { get; set; } = string.Empty;

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public List<Well> Wells { get; set; } = new();
}
