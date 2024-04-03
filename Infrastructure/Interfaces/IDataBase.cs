using Domain;

namespace Infrastructure.Interfaces;

public interface IDataBase
{
    RailwayStation GetStation(int id);
}
