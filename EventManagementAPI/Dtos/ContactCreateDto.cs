using System.ComponentModel.DataAnnotations;

namespace EventManagementAPI.Dtos
{
    public class ContactCreateDto
    {
        [Required, MinLength(2)]
        public string? Name { get; set; }

        [Required, EmailAddress]
        public string? Email { get; set; }

        [Required, MinLength(10), MaxLength(15)]
        public string? Phone { get; set; }

        [Required]
        public string? Message { get; set; }
    }
}
