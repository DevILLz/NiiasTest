namespace Domain;

public class RailwayStation
{
    public int Id { get; set; }
    public string Name { get; set; }

    private readonly Lazy<ICollection<TrackSection>> sections = new(new List<TrackSection>());
    public IEnumerable<TrackSection> Sections => sections.Value;

    private readonly Lazy<ICollection<RailwayPark>> parks = new(new List<RailwayPark>());
    public IEnumerable<RailwayPark> Parks => parks.Value;

    private readonly Lazy<ICollection<Track>> tracks = new(new List<Track>());
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