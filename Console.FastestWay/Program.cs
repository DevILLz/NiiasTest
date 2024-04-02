using DataBase;
using Service;
using System.Text;

IDataBase db = new DataBaseContext();
var station = db.GetStation(1);
var sections = station.Sections.ToList();
Console.WriteLine($"Station: {station.Name} \n");

while (true) {
    Console.ForegroundColor = ConsoleColor.DarkGray;
    foreach (var section in sections) {
        Console.WriteLine($"{section.Id}.{section.Name}");
    }
    Console.ResetColor();
    Console.WriteLine();
    Console.WriteLine($"Select the start section (index or name)");

    var selectStart = Console.ReadLine();

    TrackSection selectedStartSection = null;
    var isIndex = int.TryParse(selectStart, out var indexStart);
    if (isIndex && indexStart > 0 && sections.Count >= indexStart) {
        selectedStartSection = sections[indexStart - 1];
    }
    else {
        selectedStartSection = sections.FirstOrDefault(p => p.Name == selectStart);
    }

    if (selectedStartSection is null) {
        Console.WriteLine("Sequence doesn't contains that item");
        Console.WriteLine("\nContinue? (enter)");
        Console.ReadKey();
        continue;
    }

    Console.WriteLine($"Select the end section (index or name)");
    var selectEnd = Console.ReadLine();

    TrackSection selectedEndSection = null;
    var isIndexEnd = int.TryParse(selectEnd, out var indexEnd);
    if (isIndexEnd && indexEnd > 0 && sections.Count >= indexEnd) {
        selectedEndSection = sections[indexEnd - 1];
    }
    else {
        selectedEndSection = sections.FirstOrDefault(p => p.Name == selectEnd);
    }

    if (selectedEndSection is null) {
        Console.WriteLine("Sequence doesn't contains that item");
        Console.WriteLine("\nContinue? (enter)");
        Console.ReadKey();
        continue;
    }
        
    var calculator = new StationCalculatorService();
    var fastestWay = calculator.GetFastestWay(selectedStartSection, selectedEndSection, station);

    Console.WriteLine();

    var sectionsString = new StringBuilder();
    foreach (var section in fastestWay) {
        sectionsString.Append($"{section.Name} -> ");
    }
    sectionsString.Remove(sectionsString.Length - 3, 2);

    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine(sectionsString.ToString());
    Console.ResetColor();

    Console.WriteLine("\n\nContinue? (enter)");
    Console.ReadKey();
}