namespace Domain;

public class RailwayPark
{
    public int Id { get; set; }
    public string Name { get; set; }

    private readonly Lazy<ICollection<Track>> tracks = new(new List<Track>());
    public IEnumerable<Track> Tracks => tracks.Value;

    public void AddTrack(Track track) {
        // Тут можно вносить дополнительные проверки при необходимости
        tracks.Value.Add(track);
        track.AddToPark(this);
    }
    public override string ToString() => Name;
}
