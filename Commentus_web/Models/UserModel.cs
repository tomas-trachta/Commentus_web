using System.ComponentModel.DataAnnotations;

namespace Commentus_web.Models
{
    public class UserModel
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Name cannot be empty!")]
        [MinLength(3, ErrorMessage = "Name must have at least 3 characters!")]
        public string Name { get; set; } = null!;

        [Required(AllowEmptyStrings = false, ErrorMessage = "Password cannot be empty!")]
        [MinLength(4, ErrorMessage = "Password must have at least 4 characters!")]
        public string Password { get; set; } = null!;
    }
}
