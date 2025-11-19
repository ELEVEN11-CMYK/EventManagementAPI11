using Microsoft.AspNetCore.Mvc;
using EventManagementAPI.Data;
using EventManagementAPI.Models;
using Microsoft.AspNetCore.Authorization;
using EventManagementAPI.Dtos;
using Microsoft.EntityFrameworkCore;


namespace EventManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EventsController(AppDbContext context)
        {
            _context = context;
        }

        // POST: api/events
        [HttpPost]
        public async Task<IActionResult> CreateEvent([FromBody] Event newEvent)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _context.Events.Add(newEvent);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEventById), new { id = newEvent.Id }, newEvent);
        }

        // 🟡 PATCH: api/events/{id}  ✅ Added new partial update feature
        [HttpPatch("{id}")]
       // [Authorize(Roles = "Admin")] // <-- Optional (remove while testing)
        public async Task<IActionResult> UpdateEvent(int id, [FromBody] UpdateEventDto dto)
        {
            var eventEntity = await _context.Events.FindAsync(id);
            if (eventEntity == null)
                return NotFound(new { message = "Event not found" });

            // ✅ Partial update only if field is provided
            if (!string.IsNullOrEmpty(dto.Title))
                eventEntity.Title = dto.Title;

            if (!string.IsNullOrEmpty(dto.Description))
                eventEntity.Description = dto.Description;

            if (dto.EventDate.HasValue)
                eventEntity.EventDate = dto.EventDate.Value;

            if (!string.IsNullOrEmpty(dto.EventType))
            {
                var validTypes = new[] { "collage","school","cultural","educational","sports" };

                if (!validTypes.Contains(dto.EventType.ToLower()))
                    return BadRequest(new { message = "Invalid event type" });

                eventEntity.EventType = dto.EventType;
            }

            if (!string.IsNullOrEmpty(dto.Location))
                eventEntity.Location = dto.Location;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Event updated successfully" });
        }

        //// 🔵 GET: api/events ✅ Added list + search + filter + sort
        //[HttpGet]
        //[AllowAnonymous]
        //public async Task<IActionResult> GetAllEvents(
        //    string? search = null,
        //    string? eventType = null,
        //    string? dateFilter = null,   // upcoming | past | today
        //    string? sortBy = "eventDate" // eventDate | title
        //)
        //{
        //    var query = _context.Events.Where(e => !e.IsDeleted).AsQueryable();
        //    var currentDate = DateTime.UtcNow.Date;

        //    // 🔍 Search support
        //    if (!string.IsNullOrEmpty(search))
        //        query = query.Where(e => e.Title.Contains(search));

        //    // 🎯 Filter by event type
        //    if (!string.IsNullOrEmpty(eventType))
        //        query = query.Where(e => e.EventType.ToLower() == eventType.ToLower());

        //    // ⏰ Date filters
        //    if (dateFilter == "upcoming")
        //        query = query.Where(e => e.EventDate > currentDate);
        //    else if (dateFilter == "past")
        //        query = query.Where(e => e.EventDate < currentDate);
        //    else if (dateFilter == "today")
        //        query = query.Where(e => e.EventDate.Date == currentDate);

        //    // ↕️ Sorting support
        //    query = sortBy switch
        //    {
        //        "title" => query.OrderBy(e => e.Title),
        //        _ => query.OrderBy(e => e.EventDate)
        //    };

        //    var events = await query.ToListAsync();
        //    return Ok(events);
        //}

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllEvents(
    string? search = null,
    string? eventType = null,
    string? dateFilter = null,
    string? sortBy = "eventDate"
)
        {
            var query = _context.Events.Where(e => !e.IsDeleted).AsQueryable();
            var currentDate = DateTime.UtcNow.Date;

            // 🔍 Search support (NULL SAFE)
            if (!string.IsNullOrEmpty(search))
                query = query.Where(e =>
                    (e.Title ?? "").ToLower().Contains(search.ToLower()) ||
                    (e.Description ?? "").ToLower().Contains(search.ToLower())
                );

            // 🎯 Filter by event type (NULL SAFE + CASE INSENSITIVE)
            if (!string.IsNullOrEmpty(eventType))
                query = query.Where(e =>
                    (e.EventType ?? "").ToLower().Contains(eventType.ToLower())
                );

            // ⏰ Date filters
            if (dateFilter == "upcoming")
                query = query.Where(e => e.EventDate > currentDate);

            else if (dateFilter == "past")
                query = query.Where(e => e.EventDate < currentDate);

            else if (dateFilter == "today")
                query = query.Where(e => e.EventDate.Date == currentDate);

            // ↕️ Sorting support (NULL SAFE)
            query = sortBy switch
            {
                "title" => query.OrderBy(e => e.Title ?? ""),
                _ => query.OrderBy(e => e.EventDate)
            };

            var events = await query.ToListAsync();
            return Ok(events);
        }

        // 🟣 GET: api/events/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetEventById(int id)
        {
            var evt = await _context.Events.FindAsync(id);
            if (evt == null)
                return NotFound(new { message = "Event not found" });

            return Ok(evt);
        }
        // 🟢 POST: api/events/upload-logo
        [HttpPost("upload-logo")]
        public async Task<IActionResult> UploadEventLogo(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "No file uploaded" });

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
                return BadRequest(new { message = "Invalid file type. Allowed: .jpg, .jpeg, .png, .webp" });

            if (file.Length > 2 * 1024 * 1024)
                return BadRequest(new { message = "File too large. Max 2 MB allowed" });

            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", "EventLogos");
            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var fileUrl = $"{Request.Scheme}://{Request.Host}/uploads/eventlogos/{fileName}";

            return Ok(new
            {
                message = "File uploaded successfully",
                fileUrl
            });
        }

        // 🔴 DELETE: api/events/{id}/logo
        [HttpDelete("{id}/logo")]
        public async Task<IActionResult> DeleteEventLogo(int id)
        {
            var evt = await _context.Events.FindAsync(id);
            if (evt == null)
                return NotFound(new { message = "Event not found." });

            if (string.IsNullOrEmpty(evt.EventLogo))
                return BadRequest(new { message = "No logo found for this event." });

            var fileName = Path.GetFileName(new Uri(evt.EventLogo).LocalPath);
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", "EventLogos", fileName);

            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);

            evt.EventLogo = null;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Event logo deleted successfully." });
        }


    }
}

