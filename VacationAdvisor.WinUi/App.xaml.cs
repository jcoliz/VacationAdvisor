using System;
using System.Reflection;
using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using VacationAdvisor.WinUi.Options;
using VacationAdvisor.WinUi.Services;
using VacationAdvisor.WinUi.ViewModels;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VacationAdvisor.WinUi;
/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : Application
{
    private Window? _window;
    private readonly IHost? _host;

    /// <summary>
    /// Initializes the singleton application object.  This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        InitializeComponent();

        //
        // Set up .NET generic host
        //
        // https://learn.microsoft.com/en-us/dotnet/core/extensions/generic-host
        //
        _host = new HostBuilder()
            .ConfigureAppConfiguration((context, config) =>
            {
                config.AddTomlFile
                (
                    new EmbeddedFileProvider(Assembly.GetExecutingAssembly()),
                    "config.toml",
                    optional: true,
                    reloadOnChange: false
                );
            })
            .ConfigureServices((context, services) =>
            {
                services.AddSingleton<MainViewModel>();
                services.AddSingleton<MainWindow>();
                services.AddOptions<AiFoundryOptions>()
                    .Bind(context.Configuration.GetSection(AiFoundryOptions.Section))
                    .ValidateOnStart();
                services.AddSingleton<TokenCredential>(sp =>
                {
                    return new DefaultAzureCredential();
                });
                services.AddSingleton<ChatClient>();
            })
            .Build();
    }

    /// <summary>
    /// Invoked when the application is launched.
    /// </summary>
    /// <param name="args">Details about the launch request and process.</param>
    protected async override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        if (_host is null)
        {
            throw new Exception("Host not found");
        }

        await _host.StartAsync();

        _window = _host.Services.GetService<MainWindow>();
        _window!.Activate();
    }
}
