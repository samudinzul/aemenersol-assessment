using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AemenersolAssessment.Models;

public class Well
{
    [Key]
    public int Id { get; set; }

    public int PlatformId { get; set; }

    [ForeignKey(nameof(PlatformId))]
    public Platform? Platform { get; set; }

    [MaxLength(200)]
    public string UniqueName { get; set; } = string.Empty;

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
