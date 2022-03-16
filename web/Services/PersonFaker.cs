using Bogus;
using Firebend.AutoCrud.Core.Models.CustomFields;

namespace web.Services;

public class PersonFaker
{
    private static Faker<Models.Person>? _faker;

    public static Faker<Models.Person> Faker
    {
        get
        {
            _faker ??= new Faker<Models.Person>()
                    .StrictMode(true)
                    .RuleFor(x => x.FirstName, f => f.Person.FirstName)
                    .RuleFor(x => x.LastName, f => f.Person.LastName)
                    .RuleFor(x => x.Id, _ => Guid.NewGuid())
                    .RuleFor(x => x.IsDeleted, _ => false)
                    .RuleFor(x => x.FavoritePeople, _ => ContactFaker.Faker.GenerateBetween(5,10))
                    .RuleFor(x => x.Home, _ => AddressFaker.Faker.Generate())
                    .RuleFor(x => x.Work, _ => AddressFaker.Faker.Generate())
                    .RuleFor(x => x.CustomFields, _ => CustomFieldsFaker.Faker.GenerateBetween(5,10))
                ;

            return _faker;
        }
    }
}

public class ContactFaker
{
    private static Faker<Models.Contact>? _faker;

    public static Faker<Models.Contact> Faker
    {
        get
        {
            _faker ??= new Faker<Models.Contact>()
                    .StrictMode(true)
                    .RuleFor(x => x.FirstName, f => f.Person.FirstName)
                    .RuleFor(x => x.LastName, f => f.Person.LastName)
                    .RuleFor(x => x.Email, f => f.Person.Email)
                    .RuleFor(x => x.Phone, f => f.Phone.PhoneNumber());

            return _faker;
        }
    }
}

public class AddressFaker
 {
     private static Faker<Models.Address>? _faker;

     public static Faker<Models.Address> Faker
     {
         get
         {
             _faker ??= new Faker<Models.Address>()
                 .StrictMode(true)
                 .RuleFor(x => x.City, f => f.Address.City())
                 .RuleFor(x => x.State, f => f.Address.State())
                 .RuleFor(x => x.Street, f => f.Address.StreetAddress());

             return _faker;
         }
     }
 }

public class CustomFieldsFaker
{
    private static Faker<CustomFieldsEntity<Guid>>? _faker;

    public static Faker<CustomFieldsEntity<Guid>> Faker
    {
        get
        {
            _faker ??= new Faker<CustomFieldsEntity<Guid>>()
                .StrictMode(false)
                .RuleFor(x => x.Value, f => f.Commerce.Color())
                .RuleFor(x => x.Key, f => f.Commerce.ProductName());

            return _faker;
        }
    }
}