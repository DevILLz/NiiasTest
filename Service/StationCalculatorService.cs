using Domain;
using Infrastructure.Interfaces;
using System.Windows;

namespace Service;

public class StationCalculatorService : IStationCalculatorService
{
    private Graph graphCache;

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

    public async Task<IEnumerable<Point>> GetPeaksOfThePark(RailwayPark park) {
        var sections = new List<TrackSection>();

        var points = GetPoints(park.Tracks.SelectMany(t => t.Sections));
        var disctinctPoints = points.ToList();

        var convex = new ConvexHull.Ouellet.ConvexHull(disctinctPoints);

        var hullsPoints = await Task.Factory.StartNew(() => {
            convex.CalcConvexHull();
            return convex.GetResultsAsArrayOfPoint();
        });
        
        return hullsPoints.Where(points.Contains).Distinct();
    }

    public async Task<IEnumerable<TrackSection>> GetFastestWay(TrackSection start, TrackSection end, RailwayStation station) {
        //добавление вершин

        if (graphCache is null) {
            graphCache = new Graph();
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
        }

        var path = await Task.Factory.StartNew(() => graphCache.GetShortestPath(start.Start, end.End));
        if (path.Count != 0) {
            path.Add(start.Start);
        }
        else {
            path = await Task.Factory.StartNew(() => graphCache.GetShortestPath(start.End, end.Start));
            path.Add(start.End);

        }

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