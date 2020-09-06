using System.ComponentModel.DataAnnotations;

namespace DeadlockedDatabase.Models
{
    public class AuthenticationRequest
    {
        [Required]
        public string AccountName { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
