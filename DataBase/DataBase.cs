using Domain;
using Infrastructure.Interfaces;
using System.Windows;

namespace DataBase;

public class DataBaseContext : IDataBase
{
    private readonly Lazy<ICollection<RailwayStation>> stations = new(CreateStations);

    public RailwayStation GetStation(int id) {
        return stations.Value.First(s => s.Id == id);
    }

    private static ICollection<RailwayStation> CreateStations() {
        var stations = new List<RailwayStation> {
            Seed()
        };
        return stations;
    }

    private static RailwayStation Seed() {
        var station = new RailwayStation { Id = 1, Name = "EasyStation" };

        var park1 = new RailwayPark { Id = 1, Name = "P1" };
        var park2 = new RailwayPark { Id = 2, Name = "P2" };

        var trak11 = new Track { Id = 1 };
        var trak12 = new Track { Id = 2 };
        var trak13 = new Track { Id = 3 };
        var trak14 = new Track { Id = 4 };



        var trak22 = new Track { Id = 5 };
        var trak23 = new Track { Id = 6 };
        var trak24 = new Track { Id = 7 };



        var sectionId = 1;
         //↓ 3 простые прямые линии
        foreach (var item in Enumerable.Range(0, 10)) {
            trak11.AddSection(new TrackSection {
                Id = sectionId++,
                Name = $"S1_P1_T1_TS{item}",
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

        foreach (var item in Enumerable.Range(0, 8)) {
            trak14.AddSection(new TrackSection {
                Id = sectionId++,
                Name = $"S1_P1_T4_TS{item}",
                Start = new Point { X = item, Y = item },
                End = new Point { X = item + 1, Y = item + 1 },
            });
        }
        park1.AddTrack(trak11);
        park1.AddTrack(trak12);
        park2.AddTrack(trak13);
        park1.AddTrack(trak14);

        foreach (var item in Enumerable.Range(0, 7)) {
            trak22.AddSection(new TrackSection {
                Id = sectionId++,
                Name = $"S1_P2_T1_TS{item}",
                Start = new Point { X = item + 3, Y = item },
                End = new Point { X = item + 4, Y = item + 1 },
            });
        }

        foreach (var item in Enumerable.Range(0, 9)) {
            trak23.AddSection(new TrackSection {
                Id = sectionId++,
                Name = $"S1_P2_T2_TS{item}",
                Start = new Point { X = item + 5, Y = item },
                End = new Point { X = item + 6, Y = item + 1 },
            });
        }

        foreach (var item in Enumerable.Range(0, 9)) {
            trak24.AddSection(new TrackSection {
                Id = sectionId++,
                Name = $"S1_P2_T3_TS{item}",
                Start = new Point { X = item + 7, Y = item },
                End = new Point { X = item + 8, Y = item + 1 },
            });
        }
        park2.AddTrack(trak22);
        park2.AddTrack(trak23);
        park2.AddTrack(trak24);

        station.AddPark(park1);
        station.AddPark(park2);
        return station;
    }
}
