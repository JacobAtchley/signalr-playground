using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Models.CustomFields;

namespace web.Models;

public class Person : IEntity<Guid>, IActiveEntity, ICustomFieldsEntity<Guid>
{
    public Guid Id { get; set; }
    public bool IsDeleted { get; set; }
    public List<CustomFieldsEntity<Guid>>? CustomFields { get; set; }

    public string? FirstName { get; set; }
    public string? LastName { get; set; }

    public Address? Home { get; set; }
    public Address? Work { get; set; }

    public List<Contact>? FavoritePeople { get; set; }
}