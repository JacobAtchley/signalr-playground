using Bogus;
using Coravel.Invocable;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Microsoft.AspNetCore.JsonPatch;
using Person = web.Models.Person;

namespace web.Services;

public class PersonEntityPump : IInvocable
{
    private readonly ILogger<PersonEntityPump> _logger;
    private readonly IEntityCreateService<Guid, Person> _createService;
    private readonly IEntityDeleteService<Guid, Person> _deleteService;
    private readonly IEntityUpdateService<Guid, Person> _updateService;

    public PersonEntityPump(IEntityCreateService<Guid, Person> createService,
        IEntityUpdateService<Guid, Person> updateService,
        IEntityDeleteService<Guid, Person> deleteService,
        ILogger<PersonEntityPump> logger)
    {
        _createService = createService;
        _updateService = updateService;
        _deleteService = deleteService;
        _logger = logger;
    }

    public async Task Invoke()
    {
        try
        {
            var person = PersonFaker.Faker.Generate();
            var faker = new Faker();

            var created = await _createService.CreateAsync(person);

            await Task.Delay(500);

            var patch = new JsonPatchDocument<Person>();
            patch.Replace(x => x.LastName, faker.Person.LastName);
            patch.Add(x => x.FavoritePeople, ContactFaker.Faker.Generate());

            await _updateService.PatchAsync(created.Id, patch);
            await Task.Delay(500);

            await _deleteService.DeleteAsync(created.Id);
            await Task.Delay(500);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error in person entity pump");
        }
    }
}