using EventManagementAPI.Data;
using EventManagementAPI.Dtos;
using EventManagementAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventManagementAPI.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class FacilitiesController : ControllerBase
    {
        private readonly AppDbContext _context;

        private readonly string[] AllowedColors =
            { "Blue", "Green", "Red", "Orange", "Purple", "Yellow" };

        public FacilitiesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateFacility([FromBody] FacilityCreateDto dto)
        {
            // Validate required fields
            if (string.IsNullOrEmpty(dto.FacilityName) ||
                string.IsNullOrEmpty(dto.Description) ||
                string.IsNullOrEmpty(dto.Icon) ||
                string.IsNullOrEmpty(dto.ColorTheme))
            {
                return BadRequest(new { message = "All fields are required" });
            }

            // Facility name unique check
            if (await _context.Facilities.AnyAsync(f => f.FacilityName == dto.FacilityName))
            {
                return BadRequest(new { message = "Facility name already exists" });
            }

            // Color theme validation
            if (!AllowedColors.Contains(dto.ColorTheme))
            {
                return BadRequest(new
                {
                    message = "Invalid colorTheme. Allowed: Blue, Green, Red, Orange, Purple, Yellow"
                });
            }

            // Create object
            var facility = new Facility
            {
                FacilityName = dto.FacilityName,
                Description = dto.Description,
                Icon = dto.Icon,
                ColorTheme = dto.ColorTheme
            };

            // Save
            await _context.Facilities.AddAsync(facility);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Facility created successfully" });
        }
    }
}
