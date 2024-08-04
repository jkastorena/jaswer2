using System.ComponentModel.DataAnnotations;

namespace jaswer2;

public class LoginModel
{
    [StringLength(100), Required]
    public string? Username { get; set; }
    [StringLength(100), Required]
    public string? Password { get; set; }
}
