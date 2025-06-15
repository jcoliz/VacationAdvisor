using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Mapsui;
using Mapsui.Extensions;
using Mapsui.Projections;
using Mapsui.Tiling;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using static System.Net.Mime.MediaTypeNames;
using static VacationAdvisor.WinUi.Message;

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

    public ObservableCollection<Message> Messages { get; } = new();

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
                Messages.Add(new Message(textBox.Text,DateTime.Now,Message.PhiMessageType.User));
                textBox.Text = string.Empty;
            }
        }
    }

    public static SolidColorBrush PhiMessageTypeToColor(PhiMessageType type)
    {
        return (type == PhiMessageType.User) ? new SolidColorBrush(Colors.White) : new SolidColorBrush(Color.FromArgb(255, 68, 228, 255));
    }

    public static SolidColorBrush PhiMessageTypeToForeground(PhiMessageType type)
    {
        return (type == PhiMessageType.User) ? new SolidColorBrush(Colors.Black) : new SolidColorBrush(Color.FromArgb(255, 80, 80, 80));
    }

    public static Visibility BoolToVisibleInversed(bool value)
    {
        return value ? Visibility.Collapsed : Visibility.Visible;
    }
}

public partial class Message //: ObservableObject // CommunityToolkit.Mvvm.ComponentModel ?
{
    //[ObservableProperty]
    public string Text;
    public DateTime MsgDateTime
    {
        get; private set;
    }

    public enum PhiMessageType
    {
        User,
        Bot
    }

    public PhiMessageType Type
    {
        get; set;
    }
    public HorizontalAlignment MsgAlignment => Type == PhiMessageType.User ? HorizontalAlignment.Right : HorizontalAlignment.Left;
    public Message(string text, DateTime dateTime, PhiMessageType type)
    {
        Text = text;
        MsgDateTime = dateTime;
        Type = type;
    }

    public override string ToString()
    {
        return MsgDateTime.ToString() + " " + Text;
    }
}
