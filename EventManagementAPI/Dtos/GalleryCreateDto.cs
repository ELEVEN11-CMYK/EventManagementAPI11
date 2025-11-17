using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace EventManagementAPI.Dtos
{
    public class GalleryCreateDto
    {
        [Required]
        public string Title { get; set; }
        public string? Description { get; set; }
        public string? Category { get; set; }

        [Required]
        public IFormFile[] Images { get; set; }
    }
}
