using System;

namespace EventManagementAPI.Models
{
    public class SchoolInfo
    {
        public int Id { get; set; }
        public string? SchoolName { get; set; }
        public string? Address { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? LogoUrl { get; set; }
        public string? About { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
