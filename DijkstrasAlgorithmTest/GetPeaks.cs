using DataBase;
using Infrastructure.Interfaces;
using Service;

namespace Tests;

public class GetPeaks
{
    private IStationCalculatorService calculator;
    private IDataBase dataBase;
    [SetUp]
    public void Setup() {
        dataBase = new DataBaseContext();
        calculator = new StationCalculatorService();
    }

    [Test]
    public async Task GetPeaksOfFirstPark() {
        var station = dataBase.GetStation(1);
        var park = station.Parks.First();

        var peaks = (await calculator.GetPeaksOfThePark(park)).ToArray();

        if (peaks.Length != 6) {
            Assert.Fail();
        }
        if (peaks[0].X != 8 || peaks[0].Y != 8) {
            Assert.Fail();
        }
        if (peaks[5].X != 14 || peaks[5].Y != 3) {
            Assert.Fail();
        }
        Assert.Pass();
    }

    [Test]
    public async Task GetPeaksOfSecondPark() {
        var station = dataBase.GetStation(1);
        var park = station.Parks.Last();

        var peaks = (await calculator.GetPeaksOfThePark(park)).ToArray();

        if (peaks.Length != 5) {
            Assert.Fail();
        }
        if (peaks[0].X != 14 || peaks[0].Y != 9) {
            Assert.Fail();
        }
        if (peaks[4].X != 16 || peaks[4].Y != 9) {
            Assert.Fail();
        }
        Assert.Pass();
    }
}