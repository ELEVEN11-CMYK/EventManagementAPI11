using System.ComponentModel.DataAnnotations;

namespace EventManagementAPI.Models

{
    public class Gallery

    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }
        public string? Description { get; set; }
        public string? Category { get; set; }

        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;

        public DateTime Created { get; set; } = DateTime.Now;
        public DateTime Updated { get; set; } = DateTime.Now;

        public List<GalleryImage> Images { get; set; } = new List<GalleryImage>();
    }
}

