namespace EventManagementAPI.Models
{
    public class Facility
    {
        public int Id { get; set; }
        public string FacilityName { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
        public string ColorTheme { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;

        public DateTime Created { get; set; } = DateTime.Now;
        public DateTime Updated { get; set; } = DateTime.Now;
    }
}
