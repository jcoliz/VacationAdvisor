using Mapsui;
using Mapsui.Nts;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Projections;
using Mapsui.Tiling;
using Mapsui.Styles;
using Mapsui.UI.WinUI;
using Mapsui.Utilities;
using Mapsui.Widgets.ScaleBar;
using Mapsui.Widgets.Zoom;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System.Collections.Generic;
using VacationAdvisor.WinUi.Entities;
using VacationAdvisor.WinUi.ViewModels;
using Windows.UI;
using NetTopologySuite.Geometries;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VacationAdvisor.WinUi;
/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainWindow : Window
{
    private MainViewModel VM { get; init; }
    private MemoryLayer? _pinLayer;

    public MainWindow(MainViewModel viewModel)
    {
        VM = viewModel;

        InitializeComponent();

        MyMap.Map.Layers.Add(OpenStreetMap.CreateTileLayer());
        var paris = SphericalMercator.FromLonLat(2.3522, 48.8566).ToMPoint();

        MyMap.Map.Home = n => n.CenterOnAndZoomTo(paris, 5000f);

        MyMap.Map.Info += Map_Info;

        AddPinLayer();
    }

    private void AddPinLayer()
    {
        var paris = SphericalMercator.FromLonLat(2.3522, 48.8566).ToMPoint();

        // Create a feature for the pin
        var pinFeature = new GeometryFeature
        {
            Geometry = new Point(2.3522, 48.8566),
        };
        pinFeature.Styles.Add(new SymbolStyle
        {
            SymbolScale = 0.8,
            Fill = new Mapsui.Styles.Brush(Mapsui.Styles.Color.Red),
            Outline = new Pen(Mapsui.Styles.Color.White, 2),
            //BitmapId = BitmapRegistry.Instance.Register(EmbeddedResourceLoader.Load("Embedded.Pin.svg",typeof(MainWindow)))
        });

        // Create the memory layer and add the feature
        _pinLayer = new MemoryLayer
        {
            Name = "PinLayer",
            Features = new List<IFeature> { pinFeature }
        };

        MyMap.Map.Layers.Add(_pinLayer);
    }

    private async void Map_Info(object? sender, Mapsui.MapInfoEventArgs e)
    {
        if (e.MapInfo?.WorldPosition != null)
        {
            var lonLat = SphericalMercator.ToLonLat(e.MapInfo.WorldPosition);
            var place = await VM.ReverseGeocode(lonLat.Y, lonLat.X);
            if (place != null)
            {
                await VM.SendMessageAsync(place);
            }
        }
    }

    private async void TextBox_KeyUp(object sender, KeyRoutedEventArgs e)
    {
        var textBox = sender as TextBox;
        if (e.Key == Windows.System.VirtualKey.Enter)
        {
            if (textBox?.Text.Length > 0)
            {
                var coordinates = await VM.GeocodeAsync(textBox.Text);
                if (coordinates != null)
                {
                    var ll = SphericalMercator.FromLonLat(coordinates.Lng, coordinates.Lat).ToMPoint();
                    MyMap.Map.Navigator.CenterOnAndZoomTo(ll, 20f);
                }
                await VM.SendMessageAsync(textBox.Text);
                textBox.Text = string.Empty;
            }
        }
    }

    public static SolidColorBrush MessageTypeToColor(ChatMessage.RoleType type)
    {
        return (type == ChatMessage.RoleType.User) ? new SolidColorBrush(Colors.White) : new SolidColorBrush(Windows.UI.Color.FromArgb(255, 68, 228, 255));
    }

    public static SolidColorBrush MessageTypeToForeground(ChatMessage.RoleType type)
    {
        return (type == ChatMessage.RoleType.User) ? new SolidColorBrush(Colors.Black) : new SolidColorBrush(Windows.UI.Color.FromArgb(255, 80, 80, 80));
    }

    public static HorizontalAlignment MessageTypeToHorizontalAlignment(ChatMessage.RoleType type)
    {
        return (type == ChatMessage.RoleType.User) ? HorizontalAlignment.Right : HorizontalAlignment.Left;

    }

    public static Visibility BoolToVisibleInversed(bool value)
    {
        return value ? Visibility.Collapsed : Visibility.Visible;
    }
}
