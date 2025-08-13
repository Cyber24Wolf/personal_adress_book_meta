using System.ComponentModel.DataAnnotations;

namespace PersonalAdressBookMeta.Contracts;

public class CreateContactDTO
{
    [Required, MaxLength(200)] public string     FullName { get; set; } = default!;
    [Required, MaxLength(300)] public string     Address  { get; set; } = default!;
    [Required, MaxLength(50)]  public string     Phone    { get; set; } = default!;
                               public IFormFile? Photo    { get; set; }
}