using EventManagementAPI.Data;
using EventManagementAPI.Dtos;
using EventManagementAPI.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SchoolInfoController: ControllerBase
    {
        private readonly AppDbContext _context;

        public SchoolInfoController(AppDbContext context)
        {
            _context = context;
        }

        // GET
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var info = await _context.SchoolInfos.ToListAsync();
            return Ok(info);
        }


        // POST
        [HttpPost]
        public async Task<IActionResult> Create(SchoolInfoCreateDto dto)
        {
            var school = new SchoolInfo
            {
                SchoolName = dto.SchoolName,
                Address = dto.Address,
                Email = dto.Email,
                Phone = dto.Phone,
                LogoUrl = dto.LogoUrl,
                About = dto.About
            };

            _context.SchoolInfos.Add(school);
            await _context.SaveChangesAsync();

            return Ok(school);
        }

        // PUT
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, SchoolInfoUpdateDto dto)
        {
            var school = await _context.SchoolInfos.FindAsync(id);
            if (school == null) return NotFound();

            school.SchoolName = dto.SchoolName;
            school.Address = dto.Address;
            school.Email = dto.Email;
            school.Phone = dto.Phone;
            school.LogoUrl = dto.LogoUrl;
            school.About = dto.About;

            await _context.SaveChangesAsync();
            return Ok(school);
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> Patch(int id, [FromBody] JsonPatchDocument<SchoolInfo> patchDoc)
        {
            if (patchDoc == null)
                return BadRequest();

            var school = await _context.SchoolInfos.FindAsync(id);
            if (school == null)
                return NotFound();

            patchDoc.ApplyTo(school, ModelState);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _context.SaveChangesAsync();

            return Ok(school);
        }


        // DELETE
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var school = await _context.SchoolInfos.FindAsync(id);
            if (school == null) return NotFound();

            _context.SchoolInfos.Remove(school);
            await _context.SaveChangesAsync();

            return Ok("Deleted Successfully");
        }


    }
}
