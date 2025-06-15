using System.Collections.Generic;
using System.Collections.ObjectModel;
using Mapsui.Extensions;
using Mapsui.Projections;
using Mapsui.Tiling;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using VacationAdvisor.WinUi.Entities;
using Windows.UI;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VacationAdvisor.WinUi;
/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        MyMap.Map.Layers.Add(OpenStreetMap.CreateTileLayer());
        var saoPaulo = SphericalMercator.FromLonLat(-46.633, -23.55).ToMPoint();

        // Fix: Use the correct method to set the center and zoom level
        MyMap.Map.Home = n => n.CenterOnAndZoomTo(saoPaulo, 200f);
    }

    public ObservableCollection<ChatMessage> Messages { get; } = new();

    public bool AcceptsMessages
    {
        get; set;
    } 
    = true;

    private async void TextBox_KeyUp(object sender, KeyRoutedEventArgs e)
    {
        var textBox = sender as TextBox;
        if (e.Key == Windows.System.VirtualKey.Enter)
        {
            if (textBox?.Text.Length > 0)
            {
                var message = new ChatMessage
                {
                    Role = textBox.Text[0] == '!' ? ChatMessage.RoleType.Assistant : ChatMessage.RoleType.User,
                    Contents = new List<ChatMessage.Content>
                    {
                        new()
                        {
                            Type = ChatMessage.Content.ContentType.Text,
                            Text = textBox.Text
                        }
                    }
                };
                Messages.Add(message);
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
