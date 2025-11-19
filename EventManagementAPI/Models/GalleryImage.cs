using System.Text.Json.Serialization;

namespace EventManagementAPI.Models
{
    public class GalleryImage
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; }
        public int GalleryId { get; set; }

        [JsonIgnore]
        public Gallery Gallery { get; set; }
    }
}
