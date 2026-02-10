
namespace ServePoint.Cadet.Models.Opportunities;

public sealed class EditModel : CreateEditModelBase
{
    public int Id { get; set; }
    public int ContactId { get; set; }
    public int? AddressId { get; set; }
    public bool IsApproved { get; set; }
}