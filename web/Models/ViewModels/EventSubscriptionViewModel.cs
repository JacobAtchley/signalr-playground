using System.ComponentModel.DataAnnotations;

namespace web.Models.ViewModels;

public class EventSubscriptionViewModel
{
    [Required]
    public EntityTrigger? Trigger { get; set; }

    public string? Filter { get; set; }
}