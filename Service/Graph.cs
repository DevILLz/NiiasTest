using System.Windows;

namespace Service;

internal class Graph
{
    private readonly Dictionary<Point, Dictionary<Point, int>> vertices = [];

    public void AddVertex(Point point, Dictionary<Point, int> edges) {
        vertices[point] = edges;
    }

    public List<Point> GetShortestPath(Point start, Point finish) {
        var previous = new Dictionary<Point, Point>();
        var distances = new Dictionary<Point, int>();
        var nodes = new List<Point>();

        List<Point> path = null;

        foreach (var vertex in vertices) {
            if (vertex.Key == start) {
                distances[vertex.Key] = 0;
            }
            else {
                distances[vertex.Key] = int.MaxValue;
            }

            nodes.Add(vertex.Key);
        }

        while (nodes.Count != 0) {
            nodes.Sort((x, y) => distances[x] - distances[y]);

            var smallest = nodes[0];
            nodes.Remove(smallest);

            if (smallest == finish) {
                path = new List<Point>();
                while (previous.ContainsKey(smallest)) {
                    path.Add(smallest);
                    smallest = previous[smallest];
                }

                break;
            }

            if (distances[smallest] == int.MaxValue) {
                break;
            }

            foreach (var neighbor in vertices[smallest]) {
                var alt = distances[smallest] + neighbor.Value;
                if (alt < distances[neighbor.Key]) {
                    distances[neighbor.Key] = alt;
                    previous[neighbor.Key] = smallest;
                }
            }
        }

        return path;
    }
}