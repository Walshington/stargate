using System.ComponentModel.DataAnnotations;

namespace StargateAPI.Domain.Dtos;

public class CreatePersonRequest
{
    [Required]
    [MinLength(2)]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
}
