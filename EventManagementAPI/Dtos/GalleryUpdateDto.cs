namespace EventManagementAPI.Dtos
{
    public class GalleryUpdateDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Category { get; set; }
        public bool? IsActive { get; set; }

        public IFormFile[]? NewImages { get; set; }
        public int[]? DeleteImageIds { get; set; }
    }
}
