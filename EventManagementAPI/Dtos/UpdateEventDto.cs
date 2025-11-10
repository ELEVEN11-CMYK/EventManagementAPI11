namespace EventManagementAPI.Dtos
{
    public class UpdateEventDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime? EventDate { get; set; }
        public string? EventType { get; set; }
        public string? Location { get; set; }
    }
}
