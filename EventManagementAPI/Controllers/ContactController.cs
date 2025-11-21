using EventManagementAPI.Data;
using EventManagementAPI.Dtos;
using EventManagementAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace EventManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContactController: ControllerBase
    {

        private readonly AppDbContext _context;

        public ContactController(AppDbContext context)
        {
            _context = context;
        }

        // 📘 1. Create Contact Message (Public)
        [HttpPost]
        public async Task<IActionResult> Create(ContactCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Validation failed.", errors = ModelState });

            var contact = new ContactMessage
            {
                Name = dto.Name,
                Email = dto.Email,
                Phone = dto.Phone,
                Message = dto.Message
            };

            _context.ContactMessages.Add(contact);
            await _context.SaveChangesAsync();

            return StatusCode(201, new
            {
                message = "Your message has been submitted successfully.",
                status = true
            });
        }

        // 📘 2. Get All (Admin)
        [HttpGet]
        public async Task<IActionResult> GetAll(
            int page = 1,
            int limit = 10,
            string search = "",
            bool? isRead = null)
        {
            var query = _context.ContactMessages
                .Where(c => !c.IsDeleted);

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(c =>
                    c.Name.Contains(search) ||
                    c.Email.Contains(search) ||
                    c.Phone.Contains(search));
            }

            if (isRead.HasValue)
                query = query.Where(c => c.IsRead == isRead.Value);

            var total = await query.CountAsync();

            var data = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * limit)
                .Take(limit)
                .Select(c => new ContactReadDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Email = c.Email,
                    Phone = c.Phone,
                    Message = c.Message,
                    IsRead = c.IsRead,
                    IsDeleted = c.IsDeleted,
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();

            return Ok(new { total, page, records = data });
        }

        // 📘 3. Get Single Message (Admin)
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var contact = await _context.ContactMessages.FindAsync(id);
            if (contact == null)
                return NotFound();

            return Ok(contact);
        }

        // 📘 4. Mark as Read
        [HttpPatch("{id}/mark-read")]
        public async Task<IActionResult> MarkRead(int id)
        {
            var contact = await _context.ContactMessages.FindAsync(id);
            if (contact == null)
                return NotFound();

            contact.IsRead = true;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Message marked as read." });
        }

        // 📘 5. Reply (Optional - Logic only)
        [HttpPost("{id}/reply")]
        public async Task<IActionResult> Reply(int id, [FromBody] ContactReplyDto dto)
        {
            var contact = await _context.ContactMessages.FindAsync(id);

            if (contact == null)
                return NotFound("Contact message not found");

            contact.ReplyMessage = dto.ReplyMessage;
            contact.RepliedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Reply sent successfully" });
        }
        // 📘 6. Soft Delete
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var contact = await _context.ContactMessages.FindAsync(id);
            if (contact == null)
                return NotFound();

            contact.IsDeleted = true;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Contact message deleted successfully." });
        }

        // 📘 7. Restore Soft Deleted
        [HttpPatch("{id}/restore")]
        public async Task<IActionResult> Restore(int id)
        {
            var contact = await _context.ContactMessages.FindAsync(id);
            if (contact == null)
                return NotFound();

            contact.IsDeleted = false;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Contact message restored successfully." });
        }

    }
}
