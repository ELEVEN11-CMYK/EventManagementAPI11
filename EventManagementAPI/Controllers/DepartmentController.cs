using EventManagementAPI.Data;
using EventManagementAPI.Dtos;
using EventManagementAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DepartmentController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DepartmentController(AppDbContext context)
        {
            _context = context;
        }

        // CREATE
        [HttpPost]
        public async Task<IActionResult> CreateDepartment(DepartmentDto dto)
        {
            var department = new Department
            {
                Name = dto.Name,
                Description = dto.Description
            };

            _context.Departments.Add(department);
            await _context.SaveChangesAsync();

            return Ok(department);
        }

        // GET ALL
        [HttpGet]
        public async Task<IActionResult> GetAllDepartments()
        {
            var departments = await _context.Departments
                                .Where(d => !d.IsDeleted)
                                .ToListAsync();
            return Ok(departments);
        }

        // GET BY ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDepartmentById(int id)
        {
            var department = await _context.Departments.FindAsync(id);

            if (department == null || department.IsDeleted)
                return NotFound("Department not found");

            return Ok(department);
        }

        // UPDATE
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDepartment(int id, DepartmentDto dto)
        {
            var department = await _context.Departments.FindAsync(id);

            if (department == null || department.IsDeleted)
                return NotFound("Department not found");

            department.Name = dto.Name;
            department.Description = dto.Description;
            department.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok(department);
        }

        // DELETE (Soft Delete)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDepartment(int id)
        {
            var department = await _context.Departments.FindAsync(id);

            if (department == null || department.IsDeleted)
                return NotFound("Department not found");

            department.IsDeleted = true;
            await _context.SaveChangesAsync();

            return Ok("Department Deleted Successfully");
        }
    }
}
    
