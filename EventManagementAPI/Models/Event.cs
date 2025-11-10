using System;
using System.ComponentModel.DataAnnotations;

namespace EventManagementAPI.Models
{
    public class Event
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; }

        public string Description { get; set; }

        [Required(ErrorMessage = "Event date is required")]
        public DateTime EventDate { get; set; }

        [Required(ErrorMessage = "Event type is required")]
        [RegularExpression("^(sports|educational|cultural|school)$",
            ErrorMessage = "Event type must be one of: sports, educational, cultural, school")]
        public string EventType { get; set; }

        public string EventLogo { get; set; }

        public bool IsDeleted { get; set; } = false;

        public string Location { get; set; }
    }

}

