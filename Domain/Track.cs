namespace Domain;

public class Track
{
    public int Id { get; set; }

    public RailwayPark Park { get; private set; }
    private readonly Lazy<ICollection<TrackSection>> sections = new(new List<TrackSection>());
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
