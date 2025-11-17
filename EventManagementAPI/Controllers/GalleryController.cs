using EventManagementAPI.Data;
using EventManagementAPI.Dtos;
using EventManagementAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace EventManagementAPI.Controllers
{
    //public class GalleryController : ControllerBase
    //
        [Route("api/[controller]")]
        [ApiController]
        public class GalleryController : ControllerBase
        {
            private readonly AppDbContext _context;
            private readonly IWebHostEnvironment _env;

            public GalleryController(AppDbContext context, IWebHostEnvironment env)
            {
                _context = context;
                _env = env;
            }

            // ------------------------ CREATE GALLERY ------------------------
            [HttpPost]
            public async Task<IActionResult> CreateGallery([FromForm] GalleryCreateDto dto)
            {
                if (dto.Images == null || dto.Images.Length == 0)
                    return BadRequest("At least one image is required");

                string[] allowedTypes = { ".jpg", ".jpeg", ".png", ".webp" };

                foreach (var file in dto.Images)
                {
                    var ext = Path.GetExtension(file.FileName).ToLower();
                    if (!allowedTypes.Contains(ext))
                        return BadRequest("Invalid file type. Only JPG, PNG, WEBP allowed");

                    if (file.Length > 5 * 1024 * 1024)
                        return BadRequest("Max file size is 5 MB");
                }

                var gallery = new Gallery
                {
                    Title = dto.Title,
                    Description = dto.Description,
                    Category = dto.Category,
                    Created = DateTime.Now,
                    Updated = DateTime.Now
                };

                _context.Galleries.Add(gallery);
                await _context.SaveChangesAsync();

            //var folderPath = Path.Combine(_env.WebRootPath, "uploads/gallery", gallery.Id.ToString());
            //Directory.CreateDirectory(folderPath);

            var root = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var folderPath = Path.Combine(root, "uploads", "gallery", gallery.Id.ToString());

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            gallery.Images = new List<GalleryImage>();

                foreach (var file in dto.Images)
                {
                    var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                    var fullPath = Path.Combine(folderPath, fileName);

                    using (var stream = new FileStream(fullPath, FileMode.Create))
                        await file.CopyToAsync(stream);

                    gallery.Images.Add(new GalleryImage
                    {
                        ImageUrl = $"/uploads/gallery/{gallery.Id}/{fileName}",
                        GalleryId = gallery.Id
                    });
                }

                await _context.SaveChangesAsync();
                return Ok("Gallery item created successfully");
            }

            // ------------------------ UPDATE GALLERY ------------------------
            [HttpPatch("{id}")]
            public async Task<IActionResult> UpdateGallery(int id, [FromForm] GalleryUpdateDto dto)
            {
                var gallery = await _context.Galleries.Include(g => g.Images).FirstOrDefaultAsync(g => g.Id == id);
                if (gallery == null)
                    return NotFound("Gallery item not found");

                if (dto.Title != null) gallery.Title = dto.Title;
                if (dto.Description != null) gallery.Description = dto.Description;
                if (dto.Category != null) gallery.Category = dto.Category;
                if (dto.IsActive != null) gallery.IsActive = dto.IsActive.Value;
                gallery.Updated = DateTime.Now;

                // Delete images
                if (dto.DeleteImageIds != null)
                {
                    foreach (var imgId in dto.DeleteImageIds)
                    {
                        var image = gallery.Images.FirstOrDefault(i => i.Id == imgId);
                        if (image != null)
                        {
                            var fullPath = Path.Combine(_env.WebRootPath, image.ImageUrl.TrimStart('/'));
                            if (System.IO.File.Exists(fullPath))
                                System.IO.File.Delete(fullPath);

                            _context.GalleryImages.Remove(image);
                        }
                    }
                }

                // Add new images
                if (dto.NewImages != null)
                {
                    var folderPath = Path.Combine(_env.WebRootPath, "uploads/gallery", gallery.Id.ToString());
                    Directory.CreateDirectory(folderPath);

                    foreach (var file in dto.NewImages)
                    {
                        var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                        var fullPath = Path.Combine(folderPath, fileName);

                        using (var stream = new FileStream(fullPath, FileMode.Create))
                            await file.CopyToAsync(stream);

                        _context.GalleryImages.Add(new GalleryImage
                        {
                            GalleryId = gallery.Id,
                            ImageUrl = $"/uploads/gallery/{gallery.Id}/{fileName}"
                        });
                    }
                }

                await _context.SaveChangesAsync();
                return Ok("Gallery updated successfully");
            }

            // ------------------------ GET ALL ------------------------
            [HttpGet]
            public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] string? category,
                                                    [FromQuery] string? sortBy = "date")
            {
                var query = _context.Galleries
                    .Where(g => !g.IsDeleted)
                    .Include(g => g.Images)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(search))
                    query = query.Where(g => g.Title.Contains(search));

                if (!string.IsNullOrEmpty(category))
                    query = query.Where(g => g.Category == category);

                query = sortBy == "title"
                    ? query.OrderBy(g => g.Title)
                    : query.OrderByDescending(g => g.Created);

                var result = query.Select(g => new
                {
                    g.Id,
                    g.Title,
                    g.Created,
                    g.Category,
                    Thumbnail = g.Images.FirstOrDefault().ImageUrl,
                    ImageCount = g.Images.Count
                });

                return Ok(await result.ToListAsync());
            }

            // ------------------------ GET BY ID ------------------------
            [HttpGet("{id}")]
            public async Task<IActionResult> GetById(int id)
            {
                var gallery = await _context.Galleries
                    .Include(g => g.Images)
                    .FirstOrDefaultAsync(g => g.Id == id && !g.IsDeleted);

                if (gallery == null)
                    return NotFound("Gallery item not found");

                return Ok(gallery);
            }

            // ------------------------ DELETE GALLERY ------------------------
            [HttpDelete("{id}")]
            public async Task<IActionResult> DeleteGallery(int id)
            {
                var gallery = await _context.Galleries.FindAsync(id);
                if (gallery == null)
                    return NotFound("Gallery item not found");

                gallery.IsDeleted = true;
                await _context.SaveChangesAsync();

                return Ok("Gallery deleted successfully");
            }

            // ------------------------ DELETE A SINGLE IMAGE ------------------------
            [HttpDelete("image/{imageId}")]
            public async Task<IActionResult> DeleteImage(int imageId)
            {
                var image = await _context.GalleryImages.FindAsync(imageId);
                if (image == null)
                    return NotFound("Image not found");

                var fullPath = Path.Combine(_env.WebRootPath, image.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(fullPath))
                    System.IO.File.Delete(fullPath);

                _context.GalleryImages.Remove(image);
                await _context.SaveChangesAsync();

                return Ok("Image removed successfully");
            }
        }
    }

