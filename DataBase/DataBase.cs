using System.Windows;
using System.Xml.Linq;

namespace DataBase;

public interface IDataBase
{
    RailwayStation GetStation(int id);
}

public class DataBaseContext : IDataBase
{
    private readonly Lazy<ICollection<RailwayStation>> stations = new(CreateStations);

    public RailwayStation GetStation(int id) {
        return stations.Value.First(s => s.Id == id);
    }

    private static ICollection<RailwayStation> CreateStations() {
        var stations = new List<RailwayStation> {
            CreateStation()
        };
        return stations;
    }

    private static RailwayStation CreateStation() {
        var station = new RailwayStation { Id = 1, Name = "EasyStation" };

        var park1 = new RailwayPark { Id = 1, Name = "P1" };
        var park2 = new RailwayPark { Id = 2, Name = "P2" };

        station.AddPark(park1);
        station.AddPark(park2);

        var trak11 = new Track { Id = 1 };
        var trak12 = new Track { Id = 2 };
        var trak13 = new Track { Id = 3 };

        park1.AddTrack(trak11);
        park1.AddTrack(trak12);
        park1.AddTrack(trak13);

        var trak21 = new Track { Id = 4 };
        var trak22 = new Track { Id = 5 };
        var trak23 = new Track { Id = 6 };
        var trak24 = new Track { Id = 7 };

        park2.AddTrack(trak21);
        park2.AddTrack(trak22);
        park2.AddTrack(trak23);
        park2.AddTrack(trak24);

        var sectionId = 1;
         //↓ 3 простые прямые линии
        foreach (var item in Enumerable.Range(0, 10)) {
            trak11.AddSection(new TrackSection {
                Id = sectionId++,
                Name = $"S1_P1_T{item}",
                Start = new Point { X = item, Y = 1 },
                End = new Point { X = item + 1, Y = 1 },
            });
        }

        foreach (var item in Enumerable.Range(2, 12)) {
            trak12.AddSection(new TrackSection {
                Id = sectionId++,
                Name = $"S1_P1_T2_TS{item}",
                Start = new Point { X = item, Y = 3 },
                End = new Point { X = item + 1, Y = 3 },
            });
        }

        foreach (var item in Enumerable.Range(1, 9)) {
            trak13.AddSection(new TrackSection {
                Id = sectionId++,
                Name = $"S1_P1_T3_TS{item}",
                Start = new Point { X = item, Y = 5 },
                End = new Point { X = item + 1, Y = 5 },
            });
        }


         //↓ 2 простые прямые линии, пересекающие 3 верхние
        foreach (var item in Enumerable.Range(0, 8)) {
            trak21.AddSection(new TrackSection {
                Id = sectionId++,
                Name = $"S1_P2_T1_TS{item}",
                Start = new Point { X = item, Y = item },
                End = new Point { X = item + 1, Y = item + 1 },
            });
        }

        foreach (var item in Enumerable.Range(0, 9)) {
            trak22.AddSection(new TrackSection {
                Id = sectionId++,
                Name = $"S1_P2_T2_TS{item}",
                Start = new Point { X = item + 4, Y = item },
                End = new Point { X = item + 5, Y = item + 1 },
            });
        }

        foreach (var item in Enumerable.Range(0, 9)) {
            trak23.AddSection(new TrackSection {
                Id = sectionId++,
                Name = $"S1_P2_T3_TS{item}",
                Start = new Point { X = item + 3, Y = item },
                End = new Point { X = item + 4, Y = item + 1 },
            });
        }

        foreach (var item in Enumerable.Range(0, 8)) {
            trak24.AddSection(new TrackSection {
                Id = sectionId++,
                Name = $"S1_P12_T4_TS{item}",
                Start = new Point { X = item, Y = 8 },
                End = new Point { X = item + 1, Y = 8 },
            });
        }

        station.AddPark(park1);
        station.AddPark(park2);
        return station;
    }
}

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

public class Track
{
    public int Id { get; set; }

    public RailwayPark Park { get; private set; }
    private Lazy<ICollection<TrackSection>> sections = new(new List<TrackSection>());
    public IEnumerable<TrackSection> Sections => sections.Value;

    public void AddSection(TrackSection section) {
        // Тут можно вносить дополнительные проверки при необходимости
        sections.Value.Add(section);
        section.AddToTrack(this);
    }

    public void AddToPark(RailwayPark park) {
        Park = park;
    }
}

public class RailwayPark
{
    public int Id { get; set; }
    public string Name { get; set; }

    private Lazy<ICollection<Track>> tracks = new(new List<Track>());
    public IEnumerable<Track> Tracks => tracks.Value;

    public void AddTrack(Track track) {
        // Тут можно вносить дополнительные проверки при необходимости
        tracks.Value.Add(track);
        track.AddToPark(this);
    }
    public override string ToString() => Name;
}


public class RailwayStation
{
    public int Id { get; set; }
    public string Name { get; set; }

    private Lazy<ICollection<TrackSection>> sections = new(new List<TrackSection>());
    public IEnumerable<TrackSection> Sections => sections.Value;

    private Lazy<ICollection<RailwayPark>> parks = new(new List<RailwayPark>());
    public IEnumerable<RailwayPark> Parks => parks.Value;

    private Lazy<ICollection<Track>> tracks = new(new List<Track>());
    public IEnumerable<Track> Tracks => tracks.Value;

    public void AddPark(RailwayPark park) {
        // Тут можно вносить дополнительные проверки при необходимости
        parks.Value.Add(park);
        foreach (var track in park.Tracks) {
            AddTrack(track);
        }
    }

    public void AddTrack(Track track) {
        tracks.Value.Add(track);
        foreach (var section in track.Sections) {
            AddSection(section);
        }
    }

    public void AddSection(TrackSection section) {
        sections.Value.Add(section);
        section.AddToStation(this);
    }
}