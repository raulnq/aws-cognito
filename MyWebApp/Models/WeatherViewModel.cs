namespace MyWebApp.Models;

public class WeatherViewModel
{
    public int TemperatureC { get; set; }
    public int TemperatureF { get; set; }
    public string? Summary { get; set; }
    public DateOnly Date { get; set; }
}