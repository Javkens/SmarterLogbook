namespace Smarter_Logbook.Models;

public class FlightLogPageViewModel
{
    public List<FlightLogRowViewModel> Flights { get; set; } = [];

    public string? ErrorMessage { get; set; }
}

public class FlightLogRowViewModel
{
    public string Date { get; set; } = string.Empty;
    public string FlightNumber { get; set; } = string.Empty;
    public string Aircraft { get; set; } = string.Empty;
    public string AircraftModel { get; set; } = string.Empty;
    public string From { get; set; } = string.Empty;
    public string To { get; set; } = string.Empty;
    public string Departed { get; set; } = string.Empty;
    public string Arrived { get; set; } = string.Empty;
    public string TakeoffTime { get; set; } = string.Empty;
    public string LandingTime { get; set; } = string.Empty;
    public string Total { get; set; } = string.Empty;
    public string TakeoffsDay { get; set; } = string.Empty;
    public string Remarks { get; set; } = string.Empty;
}
