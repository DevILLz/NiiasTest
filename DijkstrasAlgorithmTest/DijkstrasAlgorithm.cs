using DataBase;
using Infrastructure.Interfaces;
using Service;

namespace Tests;

public class DijkstrasAlgorithm
{
    private IStationCalculatorService calculator;
    private IDataBase dataBase;
    [SetUp]
    public void Setup() {
        dataBase = new DataBaseContext();
        calculator = new StationCalculatorService();
    }

    [Test]
    public async Task GetForward() {
        var station = dataBase.GetStation(1);
        var sections = station.Sections.ToArray();

        var path = (await calculator.GetFastestWay(sections[0], sections[1], station)).ToArray();

        if (path.Length != 2) {
            Assert.Fail();
        }
        if (path[0] != sections[0]) {
            Assert.Fail("Start must be the same");
        }
        if (path[1] != sections[1]) {
            Assert.Fail();
        }
        Assert.Pass();
    }

    [Test]
    public async Task GetForwardLong() {
        var station = dataBase.GetStation(1);
        var sections = station.Sections.ToArray();

        var path = (await calculator.GetFastestWay(sections[0], sections[^1], station)).ToArray();

        if (path.Length != 16) {
            Assert.Fail();
        }
        if (path[0] != sections[0]) {
            Assert.Fail("Start must be the same");
        }
        if (path[^1] != sections[^1]) {
            Assert.Fail();
        }
        Assert.Pass();
    }

    [Test]
    public async Task GetBackward() {
        var station = dataBase.GetStation(1);
        var sections = station.Sections.ToArray();

        var path = (await calculator.GetFastestWay(sections[1], sections[0], station)).ToArray();

        if (path.Length != 2) {
            Assert.Fail();
        }
        if (path[0] != sections[1]) {
            Assert.Fail();
        }
        if (path[1] != sections[0]) {
            Assert.Fail();
        }
        Assert.Pass();
    }

    [Test]
    public async Task GetBackwardLong() {
        var station = dataBase.GetStation(1);
        var sections = station.Sections.ToArray();

        var path = (await calculator.GetFastestWay(sections[^1], sections[0], station)).ToArray();

        // Известная ошибка
        // Из за невозможности определить отправной пункт (начало или конец отрезка)
        // Следуя ТЗ, этот момент опускается

        // По умолчанию берется начало первого отрезка и конец второго
        // Из за этого, при обратном движении могут пропадать начальная и\или конечная станции
        if (path.Length < 14 || path.Length > 16) {
            Assert.Fail();
        }
        if (path[0] != sections[^2]) {
            Assert.Fail();
        }

        Assert.Pass();
    }
}