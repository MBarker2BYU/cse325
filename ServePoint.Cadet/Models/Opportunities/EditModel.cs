
namespace ServePoint.Cadet.Models.Opportunities;

internal sealed class EditModel : CreateEditModelBase
{
    public int Id { get; set; }
    public int ContactId { get; set; }
    public int? AddressId { get; set; }
    public bool IsApproved { get; set; }
}