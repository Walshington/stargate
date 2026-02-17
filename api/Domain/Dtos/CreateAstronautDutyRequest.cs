using System.ComponentModel.DataAnnotations;

namespace StargateAPI.Domain.Dtos;

public class CreateAstronautDutyRequest
{
    [Required]
    [MinLength(2)]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MinLength(2)]
    [MaxLength(100)]
    public string Rank { get; set; } = string.Empty;

    [Required]
    [MinLength(2)]
    [MaxLength(100)]
    public string DutyTitle { get; set; } = string.Empty;

    [Required]
    public DateTime DutyStartDate { get; set; } = DateTime.UtcNow;
}
