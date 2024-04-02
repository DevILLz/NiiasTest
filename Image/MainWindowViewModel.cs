using DataBase;
using Service;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;
using System.ComponentModel;
using System.Windows.Threading;
using System.Windows.Controls;

namespace Image;

public class MainWindowViewModel : INotifyPropertyChanged
{
    private readonly Canvas stationDrawArea;
    private readonly RailwayStation station;
    private readonly int graphScale = 30;

    private RailwayPark selectedPark;

    public event PropertyChangedEventHandler PropertyChanged;

    public ObservableCollection<RailwayPark> Parks { get; set; }
    public RailwayPark SelectedPark { 
        get => selectedPark;
        set {
            selectedPark = value;
            NotifyPropertyChanged("SelectedPark");
            CalculateArea();
        } 
    }

    public MainWindowViewModel(Canvas stationDrawArea) {
        this.stationDrawArea = stationDrawArea;
        IDataBase db = new DataBaseContext();
        station = db.GetStation(1); // пока хватит и одной

        Parks = new ObservableCollection<RailwayPark>(station.Parks);
        SelectedPark = Parks[0]; // Не безопасно, но если у нас нет парков, то и приложения не будет

        CreateStation();
    }

    private void CreateStation() {
        foreach (var section in station.Sections) {
            var sectionLine = new Line {
                Stroke = section.Track.Park.Id == SelectedPark.Id
                    ? Brushes.Blue
                    : Brushes.Black,
                X1 = section.Start.X * graphScale,
                X2 = section.End.X * graphScale,
                Y1 = section.Start.Y * graphScale,
                Y2 = section.End.Y * graphScale,

                StrokeThickness = 2
            };
            stationDrawArea.Children.Add(sectionLine);
        }
    }

    private void CalculateArea() {
        stationDrawArea.Children.Clear();
        foreach (var section in station.Sections) {
            var sectionLine = new Line {
                Stroke = section.Track.Park.Id == SelectedPark.Id
                    ? Brushes.Blue
                    : Brushes.Black,
                X1 = section.Start.X * graphScale,
                X2 = section.End.X * graphScale,
                Y1 = section.Start.Y * graphScale,
                Y2 = section.End.Y * graphScale,

                StrokeThickness = 2
            };
            stationDrawArea.Children.Add(sectionLine);
        }

        var parkArea = new StationCalculatorService().GetPeaksOfThePark(SelectedPark);
        stationDrawArea.Children.Add(new Polygon {
            Points = new PointCollection(parkArea.Select(p => new Point { X = p.X * graphScale, Y = p.Y * graphScale })),
            Fill = new SolidColorBrush(Color.FromArgb(60, 155, 255, 255))
        });


        foreach (var point in parkArea) {
            stationDrawArea.Children.Add(new Ellipse {
                Height = 6,
                Width = 6,
                Fill = Brushes.Red,
                Margin = new Thickness {
                    Left = (point.X * graphScale) - 3,
                    Top = (point.Y * graphScale) - 3
                }
            });
        }
    }

    protected void NotifyPropertyChanged(string propertyName) {
        PropertyChangedEventHandler propertyChanged = PropertyChanged;
        if (propertyChanged != null) {
            Application.Current.Dispatcher.BeginInvoke(new Action(() => propertyChanged(this, new PropertyChangedEventArgs(propertyName))), DispatcherPriority.Normal);
        }
    }
}