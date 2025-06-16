using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using System;
using System.ComponentModel;
using VacationAdvisor.WinUi.Entities;
using VacationAdvisor.WinUi.Services;
using Windows.UI;

namespace VacationAdvisor.WinUi.ViewModels;

public class ChatMessageViewModel(ChatMessage message, IDispatcher dispatcher) : INotifyPropertyChanged
{
    private ImageSource? _imageSource;
    public ImageSource? ImageSource
    {
        get => _imageSource;
        set
        {
            if (_imageSource != value)
            {
                _imageSource = value;
                OnPropertyChanged(nameof(ImageSource));
                OnPropertyChanged(nameof(ImageVisibility));                
            }
        }
    }

    public string Text
    {
        get => message.Text ?? string.Empty;
    }

    public DateTimeOffset CreatedAt
    {
        get => message.CreatedAt;
    }

    public SolidColorBrush BackgroundColor {
        get
        {
            return (message.Role == ChatMessage.RoleType.User)
                ? new SolidColorBrush(Colors.White)
                : new SolidColorBrush(Color.FromArgb(255, 68, 228, 255));
        }
    }

    public SolidColorBrush ForegroundColor {
        get
        {
            return (message.Role == ChatMessage.RoleType.User)
                ? new SolidColorBrush(Colors.Black)
                : new SolidColorBrush(Color.FromArgb(255, 80, 80, 80));
        }
    }

    public HorizontalAlignment HorizontalAlignment
    {
        get
        {
            return (message.Role == ChatMessage.RoleType.User)
                ? HorizontalAlignment.Right
                : HorizontalAlignment.Left;
        }
    }

    public Visibility ImageVisibility
    {
        get => _imageSource is null ? Visibility.Collapsed : Visibility.Visible;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged(string propertyName)
    {
        dispatcher.Dispatch(() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)));
    }
}
