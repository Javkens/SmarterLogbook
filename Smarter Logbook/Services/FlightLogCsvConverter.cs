using System.Text;
using Smarter_Logbook.Models;

namespace Smarter_Logbook.Services;

public static class FlightLogCsvConverter
{
    public sealed record OutputColumn(string Key, string Header, Func<FlightLogRowViewModel, string> Value);

    public static readonly IReadOnlyList<OutputColumn> OutputColumns =
    [
        new("date", "Date (yyyy-mm-dd)", flight => flight.Date),
        new("flightNumber", "Flight #", flight => flight.FlightNumber),
        new("aircraft", "Aircraft", flight => flight.Aircraft),
        new("aircraftModel", "Aircraft Model", flight => flight.AircraftModel),
        new("from", "From", flight => flight.From),
        new("to", "To", flight => flight.To),
        new("departed", "Departed", flight => flight.Departed),
        new("arrived", "Arrived", flight => flight.Arrived),
        new("takeoffTime", "Takeoff Time", flight => flight.TakeoffTime),
        new("landingTime", "Landing Time", flight => flight.LandingTime),
        new("total", "Total (hh:mm)", flight => flight.Total),
        new("takeoffsDay", "Takeoffs (day)", flight => flight.TakeoffsDay),
        new("remarks", "Remarks", flight => flight.Remarks)
    ];

    private static readonly string[] RequiredHeaders =
    [
        "Data", "Lista", "SP", "Zad./Cw.", "Lot. odl.", "Odblok.", "Start",
        "Lotn. przyl.", "Ląd.", "Blok.", "Czas lotu", "Starty"
    ];

    public static FlightLogPageViewModel Convert(Stream input)
    {
        using var reader = new StreamReader(input, detectEncodingFromByteOrderMarks: true, leaveOpen: true);
        var records = Parse(reader.ReadToEnd());

        if (records.Count < 2)
        {
            return new FlightLogPageViewModel { ErrorMessage = "The CSV must contain a header row and at least one flight." };
        }

        var headers = records[0]
            .Select((header, index) => new { Header = header.Trim(), Index = index })
            .GroupBy(item => item.Header, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First().Index, StringComparer.OrdinalIgnoreCase);

        var missingHeaders = RequiredHeaders.Where(header => !headers.ContainsKey(header)).ToList();
        if (missingHeaders.Count > 0)
        {
            return new FlightLogPageViewModel
            {
                ErrorMessage = $"The CSV is missing required column(s): {string.Join(", ", missingHeaders)}."
            };
        }

        var flights = records.Skip(1)
            .Where(row => row.Any(value => !string.IsNullOrWhiteSpace(value)))
            .Select(row => CreateFlight(row, headers))
            .ToList();

        return new FlightLogPageViewModel
        {
            Flights = flights,
            SelectedColumns = OutputColumns.Select(column => column.Key).ToList()
        };
    }

    public static byte[] Export(IEnumerable<FlightLogRowViewModel> flights, IEnumerable<string> selectedColumnKeys)
    {
        var selectedKeys = selectedColumnKeys.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var selectedColumns = OutputColumns.Where(column => selectedKeys.Contains(column.Key)).ToList();
        var rows = new List<IReadOnlyList<string>> { selectedColumns.Select(column => column.Header).ToList() };

        rows.AddRange(flights.Select(flight =>
            (IReadOnlyList<string>)selectedColumns.Select(column => column.Value(flight)).ToList()));

        var csv = string.Join("\r\n", rows.Select(row => string.Join(',', row.Select(Escape)))) + "\r\n";
        return new UTF8Encoding(encoderShouldEmitUTF8Identifier: true).GetBytes(csv);
    }

    private static FlightLogRowViewModel CreateFlight(IReadOnlyList<string> row, IReadOnlyDictionary<string, int> headers)
    {
        var aircraftAndModel = Value(row, headers, "SP");
        var separator = aircraftAndModel.IndexOfAny([' ', '\t']);
        var aircraft = separator < 0 ? aircraftAndModel : aircraftAndModel[..separator];
        var aircraftModel = separator < 0 ? string.Empty : aircraftAndModel[(separator + 1)..].Trim();

        return new FlightLogRowViewModel
        {
            Selected = true,
            Date = Value(row, headers, "Data"),
            FlightNumber = Value(row, headers, "Lista"),
            Aircraft = aircraft,
            AircraftModel = aircraftModel,
            From = Value(row, headers, "Lot. odl."),
            To = Value(row, headers, "Lotn. przyl."),
            Departed = Value(row, headers, "Odblok."),
            Arrived = Value(row, headers, "Blok."),
            TakeoffTime = Value(row, headers, "Start"),
            LandingTime = Value(row, headers, "Ląd."),
            Total = Value(row, headers, "Czas lotu"),
            TakeoffsDay = Value(row, headers, "Starty"),
            Remarks = Value(row, headers, "Zad./Cw.")
        };
    }

    private static string Value(IReadOnlyList<string> row, IReadOnlyDictionary<string, int> headers, string header)
    {
        var index = headers[header];
        return index < row.Count ? row[index].Trim() : string.Empty;
    }

    private static string Escape(string? value) => $"\"{(value ?? string.Empty).Replace("\"", "\"\"")}\"";

    private static List<List<string>> Parse(string csv)
    {
        var records = new List<List<string>>();
        var record = new List<string>();
        var field = new StringBuilder();
        var inQuotes = false;

        for (var index = 0; index < csv.Length; index++)
        {
            var character = csv[index];
            if (character == '\"')
            {
                if (inQuotes && index + 1 < csv.Length && csv[index + 1] == '\"')
                {
                    field.Append(character);
                    index++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (character == ',' && !inQuotes)
            {
                record.Add(field.ToString());
                field.Clear();
            }
            else if ((character == '\r' || character == '\n') && !inQuotes)
            {
                if (character == '\r' && index + 1 < csv.Length && csv[index + 1] == '\n')
                {
                    index++;
                }

                record.Add(field.ToString());
                field.Clear();
                records.Add(record);
                record = [];
            }
            else
            {
                field.Append(character);
            }
        }

        if (field.Length > 0 || record.Count > 0)
        {
            record.Add(field.ToString());
            records.Add(record);
        }

        return records;
    }
}
