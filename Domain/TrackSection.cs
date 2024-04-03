using System.Windows;

namespace Domain;

public class TrackSection
{
    public int Id { get; set; }
    public required string Name { get; set; }

    public Point Start { get; set; }
    public Point End { get; set; }

    public Track Track { get; private set; }
    public RailwayStation Station { get; private set; }

    public void AddToTrack(Track track) {
        // Тут можно вносить дополнительные проверки при необходимости
        Track = track;
    }

    public void AddToStation(RailwayStation station) {
        Station = station;
    }
}
