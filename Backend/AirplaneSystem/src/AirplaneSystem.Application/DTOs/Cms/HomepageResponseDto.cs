namespace AirplaneSystem.Application.DTOs.Cms;

public class HomepageResponseDto
{
    public List<HeroSectionDto> Hero { get; set; } = new();
    public List<SpecialOfferDto> Offers { get; set; } = new();
    public List<PopularDestinationDto> Destinations { get; set; } = new();
    public List<WhyChooseUsItemDto> WhyChooseUs { get; set; } = new();
    public List<FleetItemDto> Fleet { get; set; } = new();
    public List<TravelServiceDto> Services { get; set; } = new();
    public AnnouncementBarDto? Announcement { get; set; }
    public NavbarSettingDto Navbar { get; set; } = null!;
    public FooterSettingDto Footer { get; set; } = null!;
}