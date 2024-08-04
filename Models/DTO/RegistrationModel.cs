using System.ComponentModel.DataAnnotations;

namespace jaswer2;

public class RegistrationModel
{
    [Required]
    public string? Name { get; set; }
    [StringLength(100), Required]
    public string? Username { get; set; }
    [StringLength(100), Required]
    public string? Email { get; set; }
    [Required]
    public string? Password { get; set; }
}

