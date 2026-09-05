using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Smarter_Logbook.Models;
using Smarter_Logbook.Services;

namespace Smarter_Logbook.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View(new FlightLogPageViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Upload(IFormFile? csvFile)
    {
        if (csvFile is null || csvFile.Length == 0)
        {
            return View("Index", new FlightLogPageViewModel { ErrorMessage = "Choose a CSV file to upload." });
        }

        if (!Path.GetExtension(csvFile.FileName).Equals(".csv", StringComparison.OrdinalIgnoreCase))
        {
            return View("Index", new FlightLogPageViewModel { ErrorMessage = "Only .csv files can be uploaded." });
        }

        try
        {
            using var stream = csvFile.OpenReadStream();
            return View("Index", FlightLogCsvConverter.Convert(stream));
        }
        catch (Exception)
        {
            return View("Index", new FlightLogPageViewModel
            {
                ErrorMessage = "The file could not be read as a valid CSV file."
            });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Download(FlightLogPageViewModel model)
    {
        var csv = FlightLogCsvConverter.Export(model.Flights);
        return File(csv, "text/csv; charset=utf-8", "smarter-logbook.csv");
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
