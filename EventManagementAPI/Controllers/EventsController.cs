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
                var validTypes = new[] { "school", "corporate", "community" };
                if (!validTypes.Contains(dto.EventType.ToLower()))
                    return BadRequest(new { message = "Invalid event type" });

                eventEntity.EventType = dto.EventType;
            }

            if (!string.IsNullOrEmpty(dto.Location))
                eventEntity.Location = dto.Location;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Event updated successfully" });
        }

        // 🔵 GET: api/events ✅ Added list + search + filter + sort
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllEvents(
            string? search = null,
            string? eventType = null,
            string? dateFilter = null,   // upcoming | past | today
            string? sortBy = "eventDate" // eventDate | title
        )
        {
            var query = _context.Events.Where(e => !e.IsDeleted).AsQueryable();
            var currentDate = DateTime.UtcNow.Date;

            // 🔍 Search support
            if (!string.IsNullOrEmpty(search))
                query = query.Where(e => e.Title.Contains(search));

            // 🎯 Filter by event type
            if (!string.IsNullOrEmpty(eventType))
                query = query.Where(e => e.EventType.ToLower() == eventType.ToLower());

            // ⏰ Date filters
            if (dateFilter == "upcoming")
                query = query.Where(e => e.EventDate > currentDate);
            else if (dateFilter == "past")
                query = query.Where(e => e.EventDate < currentDate);
            else if (dateFilter == "today")
                query = query.Where(e => e.EventDate.Date == currentDate);

            // ↕️ Sorting support
            query = sortBy switch
            {
                "title" => query.OrderBy(e => e.Title),
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
    }
    }

