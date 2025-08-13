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
        if (file is null || file.Length == 0) 
            return null;

        var wwwroot = env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var photosDir = Path.Combine(wwwroot, "photos");
        Directory.CreateDirectory(photosDir);

        var name = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var path = Path.Combine(photosDir, name);
        await using var stream = System.IO.File.Create(path);
        await file.CopyToAsync(stream);

        return $"/photos/{name}";
    }
}