using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using VacationAdvisor.WinUi.Services;

namespace VacationAdvisor.WinUi.ViewModels;
public class MainViewModel(ChatClient chatClient) : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
            Messages.Add(new ChatMessageViewModel(chatMessage));
        }
        AcceptsMessages = true;
    }
}