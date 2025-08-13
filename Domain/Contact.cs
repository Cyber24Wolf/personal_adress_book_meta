namespace PersonalAdressBookMeta.Domain;

public class Contact
{
    public int      Id        { get; set; }
    public string   FullName  { get; set; } = default!;
    public string   Address   { get; set; } = default!;
    public string   Phone     { get; set; } = default!;
    public string?  PhotoUrl  { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}