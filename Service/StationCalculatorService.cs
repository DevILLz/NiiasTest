using DataBase;
using System.Windows;

namespace Service;

public class StationCalculatorService
{
    public IEnumerable<Point> GetPeaksOfThePark(RailwayPark park) {
        var sections = new List<TrackSection>();
        var points = GetPoints(park.Tracks.SelectMany(t => t.Sections));
        var disctinctPoints = points.ToList();
        var convex = new ConvexHull.Ouellet.ConvexHull(disctinctPoints);
        convex.CalcConvexHull();
        var hullsPoints = convex.GetResultsAsArrayOfPoint();
        return hullsPoints.Where(points.Contains);
    }


    public IEnumerable<TrackSection> GetSectionByPoint(Point point, RailwayPark park) {
        var sectionsWithThisPoint = new List<TrackSection>();
        foreach (var track in park.Tracks) {
            foreach (var section in track.Sections) {
                if (section.Start == point || section.End == point)
                    sectionsWithThisPoint.Add(section);
            }
        }

        return sectionsWithThisPoint;
    }

    private Graph graphCache = new Graph();
    public IEnumerable<TrackSection> GetFastestWay(TrackSection start, TrackSection end, RailwayStation station) {
        //добавление вершин


        var points = GetPoints(station.Sections);
        foreach (var point in points) {
            var edges = new Dictionary<Point, int>();
            foreach (var secttion in station.Sections.Where(s => s.Start == point || s.End == point)) {
                if (secttion.End == point) {
                    edges.Add(secttion.Start, 1);
                }
                else {
                    edges.Add(secttion.End, 1);
                }
            }
            graphCache.AddVertex(point, edges);
        }

        var path = graphCache.GetShortestPath(start.Start, end.End);
        path.Add(start.Start);

        var sectionsInTheWay = new List<TrackSection>();
        for (int i = path.Count - 1; i > 0; i--) {
            sectionsInTheWay.Add(station.Sections.First(s => (s.Start == path[i] && s.End == path[i - 1]) || (s.End == path[i] && s.Start == path[i - 1])));
        }

        return sectionsInTheWay;
    }

    private IEnumerable<Point> GetPoints(IEnumerable<TrackSection> sections) {
        var points = new List<Point>();
        foreach (var section in sections) {
            points.Add(section.Start);
            points.Add(section.End);
        }
        return points.Distinct();
    }
}