using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonalAdressBookMeta.Contracts;
using PersonalAdressBookMeta.Data;
using PersonalAdressBookMeta.Domain;

namespace PersonalAdressBookMeta.Controllers;

[ApiController]
[Route("api/contacts")]
public class ContactsController(AppDbContext db, IWebHostEnvironment env) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<Contact>>> Get([FromQuery] int skip = 0, [FromQuery] int take = 50)
    {
        if (take <= 0)
            return BadRequest("Take must be > 0");
        
        if (skip < 0)
            return BadRequest("Skip must be >= 0");

        var items = await db.Contacts
                            .OrderBy(c => c.FullName)
                            .Skip(skip)
                            .Take(take)
                            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Contact>> GetById(int id)
    {
        if  (id <= 0)
            return BadRequest("Id must be > 0");
        
        var contact = await db.Contacts.FindAsync(id);
        if (contact != null)
            return Ok(contact);
        else
            return NotFound();
    }

    [HttpPost]
    [RequestSizeLimit(10_000_000)]
    
    public async Task<ActionResult<Contact>> Create([FromForm] CreateContactDTO dto)
    {
        var contact = new Contact { FullName = dto.FullName, Address = dto.Address, Phone = dto.Phone };
        contact.PhotoUrl = await SavePhotoIfPresent(dto.Photo, env);
        db.Contacts.Add(contact);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = contact.Id }, contact);
    }

    [HttpPut("{id:int}")]
    [RequestSizeLimit(10_000_000)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Contact>> Update(int id, [FromForm] UpdateContactDTO dto)
    {
        if (id <= 0)
            return BadRequest("Id must be > 0");
        
        if (dto.Photo != null && !IsPhotoFile(dto.Photo))
            return BadRequest("Invalid photo format. Supports: .jpg, .jpeg, .png");
        
        var contact = await db.Contacts.FindAsync(id);
        if (contact == null)
            return NotFound();

        contact.FullName = dto.FullName ?? contact.FullName;
        contact.Address  = dto.Address  ?? contact.Address;
        contact.Phone    = dto.Phone    ?? contact.Phone;

        var newUrl = await SavePhotoIfPresent(dto.Photo, env);
        if (newUrl != null)
            contact.PhotoUrl = newUrl;

        await db.SaveChangesAsync();
        return Ok(contact);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        if (id <= 0)
            return BadRequest("Id must be > 0");
        
        var contact = await db.Contacts.FindAsync(id);
        if (contact == null)
            return NotFound();
        
        db.Contacts.Remove(contact);
        await db.SaveChangesAsync();
        return NoContent();
    }

    private static async Task<string?> SavePhotoIfPresent(IFormFile? file, IWebHostEnvironment env)
    {
        if (file == null || file.Length == 0)
            return null;
        
        if (!IsPhotoFile(file))
            throw new InvalidOperationException("Wrong image format. Support only: .jpg, .jpeg, .png");

        var extension = GetPhotoFileExtension(file);
        var uniqueFileName = $"{Guid.NewGuid()}{extension}";
        var uploadsFolder = Path.Combine("wwwroot", "uploads");
        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);

        var filePath = Path.Combine(uploadsFolder, uniqueFileName);
        await using var stream = System.IO.File.Create(filePath);
        await file.CopyToAsync(stream);

        return $"/uploads/{uniqueFileName}";
    }

    private static bool IsPhotoFile(IFormFile? file)
    {
        if (file == null)
            return false;
        
        var extension = GetPhotoFileExtension(file);
        if (extension == string.Empty)
            return false;

        return extension == ".jpg"  || 
               extension == ".jpeg" ||
               extension == ".png";
    }

    private static string GetPhotoFileExtension(IFormFile? file)
    {
        if (file == null)
            return string.Empty;
        
        var extension = Path.GetExtension(file.FileName); 
        if  (string.IsNullOrEmpty(extension))
            return string.Empty;

        return extension.ToLower();
    }
}