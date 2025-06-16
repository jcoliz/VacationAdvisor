using HereMaps.SearchApi;
using Microsoft.UI.Xaml.Media.Imaging;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VacationAdvisor.WinUi.Services;

namespace VacationAdvisor.WinUi.ViewModels;
public class MainViewModel(ChatClient chatClient, ApiClient hereMapsClient, IDispatcher dispatcher) : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged(string propertyName)
    {
        dispatcher.Dispatch(() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)));
    }

    // Add properties and methods for the MainViewModel here
    // Example: public string Title { get; set; } = "Vacation Advisor";

    /// <summary>
    /// Collection of chat messages exchanged with the agent.
    /// </summary>
    public ObservableCollection<ChatMessageViewModel> Messages { get; } = new();

    /// <summary>
    /// Whether we are currently accepting messages from the user.
    /// If not accepting messages, the UI should disable input controls
    /// and indicate that the agent is processing a request.
    /// </summary>
    public bool AcceptsMessages
    {
        get => _acceptsMessages;
        set
        {
            if (_acceptsMessages != value)
            {
                _acceptsMessages = value;
                OnPropertyChanged(nameof(AcceptsMessages));
            }
        }
    }
    private bool _acceptsMessages = true;

    public async Task SendMessageAsync(string message)
    {
        if (!AcceptsMessages) 
            return;

        AcceptsMessages = false;

        // Create a new thread and send a message to the agent
        var thread = await chatClient.CreateThreadAsync();
        var messages = await chatClient.SendMessageAsync(thread, message);

        // Retrieve messages from the thread
        await foreach (var chatMessage in messages)
        {
            var cvm = new ChatMessageViewModel(chatMessage, dispatcher);
            Messages.Add(cvm);

            // Load images if the message contains any
            if (chatMessage.ImageId is not null)
            {
                _ = Task.Run(async () =>
                {
                    var stream = await chatClient.GetFileContentAsync(chatMessage.ImageId);
                    if (stream is not null)
                    {
                        // Use the dispatcher to update the UI on the main thread
                        dispatcher.Dispatch(() =>
                        {
                            var bitmap = new BitmapImage();
                            bitmap.SetSource(stream.AsRandomAccessStream());
                            cvm.ImageSource = bitmap;
                        });
                    }
                });
            }
        }
        AcceptsMessages = true;
    }

    public async Task<string?> ReverseGeocode(double latitude, double longitude)
    {
        // Use the HereMaps client to reverse geocode the coordinates
        var result = await hereMapsClient.RevgeocodeAsync(at: $"{latitude},{longitude}", types: [ Anonymous22.City ]);

        return result?.Items?.FirstOrDefault()?.Address?.Label;
    }

    public async Task<DisplayResponseCoordinate?> GeocodeAsync(string query)
    {
        // Use the HereMaps client to geocode the query
        var result = await hereMapsClient.GeocodeAsync(q:query);
        return result?.Items?.FirstOrDefault()?.Position;
    }
}
