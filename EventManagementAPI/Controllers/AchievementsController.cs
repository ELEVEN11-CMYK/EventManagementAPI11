using Microsoft.AspNetCore.Mvc;
using EventManagementAPI.Data;
using EventManagementAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace EventManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AchievementsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AchievementsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Achievements
        [HttpGet]
        public async Task<IActionResult> GetAchievements()
        {
            var achievements = await _context.Achievements
                .Where(a => !a.IsDeleted)
                .ToListAsync();
            return Ok(achievements);
        }

        // GET: api/Achievements/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAchievementById(int id)
        {
            var achievement = await _context.Achievements.FindAsync(id);
            if (achievement == null || achievement.IsDeleted)
                return NotFound(new { message = "Achievement not found" });

            return Ok(achievement);
        }

        // POST: api/Achievements
        [HttpPost]
        public async Task<IActionResult> CreateAchievement([FromBody] Achievement achievement)
        {
            achievement.CreatedAt = DateTime.UtcNow;
            achievement.UpdatedAt = DateTime.UtcNow;

            _context.Achievements.Add(achievement);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Achievement created successfully" });
        }

        // PATCH: api/Achievements/{id}
        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateAchievement(int id, [FromBody] Achievement updated)
        {
            var existing = await _context.Achievements.FindAsync(id);
            if (existing == null)
                return NotFound(new { message = "Achievement not found" });

            if (!string.IsNullOrEmpty(updated.Title))
                existing.Title = updated.Title;
            if (!string.IsNullOrEmpty(updated.Description))
                existing.Description = updated.Description;
            if (!string.IsNullOrEmpty(updated.ImageUrl))
                existing.ImageUrl = updated.ImageUrl;
            if (updated.Category != existing.Category)
                existing.Category = updated.Category;
            if (updated.Date != DateTime.MinValue)
                existing.Date = updated.Date;

            existing.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Achievement updated successfully" });
        }

        // DELETE: api/Achievements/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAchievement(int id)
        {
            var existing = await _context.Achievements.FindAsync(id);
            if (existing == null)
                return NotFound(new { message = "Achievement not found" });

            existing.IsDeleted = true;
            existing.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Achievement deleted successfully" });
        }
    }
}
