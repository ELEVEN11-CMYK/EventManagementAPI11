namespace EventManagementAPI.Models
{
    public enum AchievementCategory
    {
        Academic,
        Sports,
        Cultural,
        Educational,
        School,
        Other
    }
    public class Achievement
    {
            public int Id { get; set; }

            public string Title { get; set; }

            public string Description { get; set; }

            public AchievementCategory Category { get; set; }

            public string ImageUrl { get; set; }

            public DateTime Date { get; set; }

            public bool IsActive { get; set; } = true;

            public bool IsDeleted { get; set; } = false;

            public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

            public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        }
    }

