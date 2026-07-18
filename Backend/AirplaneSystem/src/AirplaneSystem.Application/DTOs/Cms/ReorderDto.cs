namespace AirplaneSystem.Application.DTOs.Cms;

public class ReorderItemDto
{
    public Guid Id { get; set; }
    public int Order { get; set; }
}

public class ReorderRequestDto
{
    public List<ReorderItemDto> Items { get; set; } = new();
}