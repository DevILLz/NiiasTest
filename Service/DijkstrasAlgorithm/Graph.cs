namespace Service.DijkstrasAlgorithm;

/// <summary>
/// Граф
/// </summary>
public class Graph
{
    public List<GraphVertex> Vertices { get; }

    public Graph() {
        Vertices = new List<GraphVertex>();
    }

    public void AddVertex(string vertexName) {
        Vertices.Add(new GraphVertex(vertexName));
    }

    /// <summary>
    /// Поиск вершины
    /// </summary>
    /// <param name="vertexName">Название вершины</param>
    /// <returns>Найденная вершина</returns>
    public GraphVertex FindVertex(string vertexName) {
        foreach (var v in Vertices) {
            if (v.Name.Equals(vertexName)) {
                return v;
            }
        }

        return null;
    }

    /// <summary>
    /// Добавление ребра
    /// </summary>
    /// <param name="firstName">Имя первой вершины</param>
    /// <param name="secondName">Имя второй вершины</param>
    /// <param name="weight">Вес ребра соединяющего вершины</param>
    public void AddEdge(string firstName, string secondName, int weight) {
        var v1 = FindVertex(firstName);
        var v2 = FindVertex(secondName);
        if (v2 != null && v1 != null) {
            v1.AddEdge(v2, weight);
            v2.AddEdge(v1, weight);
        }
    }
}