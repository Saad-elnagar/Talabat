using System.ComponentModel.DataAnnotations;

namespace Talabat.Dto
{
    public class RegisterDto
    {

        [Required] 
        public string DisplayName { get; set; }
        [Required] 
        public string Email { get; set; }
        [Required] 
        public string UserName { get; set; }
        [Required] 
        public string Password { get; set; }
        [Required]
        public string PhoneNumber { get; set; }

    }
}
