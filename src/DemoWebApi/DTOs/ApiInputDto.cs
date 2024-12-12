namespace DemoWebApi.DTOs;

public class ApiInputDto
{
    public string SourceFolder { get; set; } = string.Empty;
    public string CompareFolder { get; set; } = string.Empty;
    public string? CompareType { get; set; }
}