using Mapsui.Extensions;
using Mapsui.Projections;
using Mapsui.Tiling;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using VacationAdvisor.WinUi.Entities;
using VacationAdvisor.WinUi.ViewModels;
using Windows.UI;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VacationAdvisor.WinUi;
/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainWindow : Window
{
    private MainViewModel VM { get; init; }

    public MainWindow(MainViewModel viewModel)
    {
        VM = viewModel;

        InitializeComponent();

        MyMap.Map.Layers.Add(OpenStreetMap.CreateTileLayer());
        var saoPaulo = SphericalMercator.FromLonLat(-46.633, -23.55).ToMPoint();

        // Fix: Use the correct method to set the center and zoom level
        MyMap.Map.Home = n => n.CenterOnAndZoomTo(saoPaulo, 200f);

        MyMap.Map.Info += Map_Info;
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
        return (type == ChatMessage.RoleType.User) ? new SolidColorBrush(Colors.White) : new SolidColorBrush(Color.FromArgb(255, 68, 228, 255));
    }

    public static SolidColorBrush MessageTypeToForeground(ChatMessage.RoleType type)
    {
        return (type == ChatMessage.RoleType.User) ? new SolidColorBrush(Colors.Black) : new SolidColorBrush(Color.FromArgb(255, 80, 80, 80));
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
