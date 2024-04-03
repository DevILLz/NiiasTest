using Domain;
using System.Windows;

namespace Infrastructure.Interfaces;

public interface IStationCalculatorService
{
    Task<IEnumerable<Point>> GetPeaksOfThePark(RailwayPark park);
    Task<IEnumerable<TrackSection>> GetFastestWay(TrackSection start, TrackSection end, RailwayStation station);
    IEnumerable<TrackSection> GetSectionByPoint(Point point, RailwayPark park);
}
