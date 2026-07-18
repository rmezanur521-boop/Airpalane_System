namespace AirplaneSystem.Application.DTOs.Cms;

public class HomepageSettingDto
{
    public Guid Id { get; set; }
    public bool ShowHero { get; set; }
    public bool ShowOffers { get; set; }
    public bool ShowDestinations { get; set; }
    public bool ShowFleet { get; set; }
    public bool ShowServices { get; set; }
    public bool ShowWhyChooseUs { get; set; }
    public bool ShowFooter { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class UpdateHomepageSettingDto
{
    public bool ShowHero { get; set; } = true;
    public bool ShowOffers { get; set; } = true;
    public bool ShowDestinations { get; set; } = true;
    public bool ShowFleet { get; set; } = true;
    public bool ShowServices { get; set; } = true;
    public bool ShowWhyChooseUs { get; set; } = true;
    public bool ShowFooter { get; set; } = true;
}