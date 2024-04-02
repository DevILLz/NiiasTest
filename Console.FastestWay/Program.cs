using DataBase;
using Service;

IDataBase db = new DataBaseContext();
var station = db.GetStation(1);

var parks = station.Parks.ToList();
Console.WriteLine($"Station: {station.Name}");
Console.WriteLine($"Parks count: {parks.Count}");

while (true) {
    foreach (var park in parks) {
        Console.WriteLine($"{park.Id}.{park.Name}");
    }
    Console.WriteLine($"Select the park for which you want to get the peaks (index or name)");

    var select = Console.ReadLine();

    RailwayPark selectedPark = null;
    var isIndex = int.TryParse(select, out var index);
    if (isIndex && index > 0 && (parks.Count - 1) <= index) {
        selectedPark = parks[index - 1];
    }
    else {
        selectedPark = parks.FirstOrDefault(p => p.Name == select);
    }

    if (selectedPark is null)
        Console.WriteLine("Sequence doesn't contains that item");
    var calculator = new StationCalculatorService();

    var parkArea = calculator.GetPeaksOfThePark(selectedPark);

    Console.WriteLine("Peaks:");
    foreach (var point in parkArea) {
        Console.Write($"Point - X:{point.X} Y:{point.Y} ");
        Console.Write("Section - ");
        foreach (var section in calculator.GetSectionByPoint(point, selectedPark)) {
            Console.Write($"{section.Name} ");
        } // может быть несколько секций, если есть пересечения

        Console.WriteLine();
    }
}