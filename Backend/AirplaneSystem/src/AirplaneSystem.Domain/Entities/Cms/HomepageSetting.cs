using AirplaneSystem.Domain.Common;

namespace AirplaneSystem.Domain.Entities.Cms;

public class HomepageSetting : BaseEntity
{
    public bool ShowHero { get; set; } = true;
    public bool ShowOffers { get; set; } = true;
    public bool ShowDestinations { get; set; } = true;
    public bool ShowFleet { get; set; } = true;
    public bool ShowServices { get; set; } = true;
    public bool ShowWhyChooseUs { get; set; } = true;
    public bool ShowFooter { get; set; } = true;
}