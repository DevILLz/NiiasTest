using DataBase;
using System.Windows;

namespace Service;

public class StationCalculatorService
{
    public IEnumerable<Point> GetPeaksOfThePark(RailwayPark park) {
        var peaks = new List<Point>();
        foreach (var track in park.Tracks) {
            foreach (var section in track.Sections) {
                peaks.Add(section.Start);
                peaks.Add(section.End);
            }
        }

        var disctinctPoints = peaks.Distinct().ToList();
        var convex = new ConvexHull.Ouellet.ConvexHull(disctinctPoints);
        convex.CalcConvexHull();
        var hullsPoints = convex.GetResultsAsArrayOfPoint();

        return hullsPoints;
    }
}
