namespace PersonalAdressBookMeta.Contracts;

public class UpdateContactDTO
{
    public string?    FullName { get; set; }
    public string?    Address  { get; set; }
    public string?    Phone    { get; set; }
    public IFormFile? Photo    { get; set; }
}