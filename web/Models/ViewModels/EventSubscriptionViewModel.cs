using System.ComponentModel.DataAnnotations;

namespace web.Models.ViewModels;

public class EventSubscriptionViewModel
{
    [Required]
    public string? Trigger { get; set; }

    public string? Filter { get; set; }
}