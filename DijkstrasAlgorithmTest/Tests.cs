using DataBase;
using Service;

namespace Tests;

public class DijkstrasAlgorithm
{
    private StationCalculatorService calculator;
    private IDataBase dataBase;
    [SetUp]
    public void Setup() {
        dataBase = new DataBaseContext();
        calculator = new StationCalculatorService();
    }

    [Test]
    public void GetForward() {
        var station = dataBase.GetStation(1);
        var sections = station.Sections.ToArray();

        var path = calculator.GetFastestWay(sections[0], sections[1], station).ToArray();

        if (path.Length != 2) {
            Assert.Fail();
        }
        if (path[0] != sections[0]) {
            Assert.Fail();
        }
        if (path[1] != sections[1]) {
            Assert.Fail();
        }
        Assert.Pass();
    }

    [Test]
    public void GetForwardLong() {
        var station = dataBase.GetStation(1);
        var sections = station.Sections.ToArray();

        var path = calculator.GetFastestWay(sections[0], sections[54], station).ToArray();

        if (path.Length != 12) {
            Assert.Fail();
        }
        if (path[0] != sections[0]) {
            Assert.Fail();
        }
        if (path[11] != sections[54]) {
            Assert.Fail();
        }
        Assert.Pass();
    }

    [Test]
    public void GetBackward() {
        var station = dataBase.GetStation(1);
        var sections = station.Sections.ToArray();

        var path = calculator.GetFastestWay(sections[1], sections[0], station).ToArray();

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
    public void GetBackwardLong() {
        var station = dataBase.GetStation(1);
        var sections = station.Sections.ToArray();

        var path = calculator.GetFastestWay(sections[54], sections[0], station).ToArray();

        // Известная ошибка
        // Из за невозможности определить отправной пункт (начало или конец отрезка)
        // Следуя ТЗ, этот момент опускается

        // По умолчанию берется начало первого отрезка и конец второго
        // Из за этого, при обратном движении могут пропадать начальная и\или конечная станции
        if (path.Length != 10) {
            Assert.Fail();
        }
        if (path[0] != sections[54]) {
            Assert.Fail();
        }
        if (path[9] != sections[0]) {
            Assert.Fail();
        }
        Assert.Pass();
    }
}