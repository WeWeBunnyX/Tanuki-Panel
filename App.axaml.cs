using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using TanukiPanel.Services;
using TanukiPanel.ViewModels;
using TanukiPanel.Views;
using Avalonia.Themes.Fluent;
using Microsoft.Extensions.DependencyInjection;

namespace TanukiPanel;

public partial class App : Application
{
    private ServiceProvider? _serviceProvider;

    public override void Initialize()
    {
        // Configure application in C# (no App.axaml)

        // Register the ViewLocator as a DataTemplate so view models resolve to views
        DataTemplates.Add(new ViewLocator());

        // Add the Fluent theme
        Styles.Add(new FluentTheme());
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();

            // Set up dependency injection
            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();

            // Create MainWindowViewModel and NavigationService
            var mainWindowViewModel = _serviceProvider.GetRequiredService<MainWindowViewModel>();
            var navigationService = _serviceProvider.GetRequiredService<INavigationService>();
            
            // Initialize the ViewModel with navigation service (breaks circular dependency)
            mainWindowViewModel.InitializeWithNavigation(navigationService);

            desktop.MainWindow = new MainWindow
            {
                DataContext = mainWindowViewModel,
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void ConfigureServices(ServiceCollection services)
    {
        // Register services
        services.AddSingleton<IApiKeyPersistence, ApiKeyPersistence>();
        
        // Register MainWindowViewModel first
        services.AddSingleton<MainWindowViewModel>();
        
        // Register INavigationService as a factory that gets MainWindowViewModel after it's created
        services.AddSingleton<INavigationService>(sp => 
        {
            var mainVM = sp.GetRequiredService<MainWindowViewModel>();
            return new NavigationService(mainVM);
        });

        // Register GitLab API Service (will be initialized with API key when needed)
        services.AddSingleton<IGitLabApiService>(sp =>
        {
            // Get the API key from persistence
            var persistence = sp.GetRequiredService<IApiKeyPersistence>();
            var config = persistence.LoadApiKey();
            
            if (config != null && !string.IsNullOrEmpty(config.ApiKey))
            {
                // Extract GitLab instance URL (default to gitlab.com if not specified)
                string gitlabUrl = "https://gitlab.com";
                return new GitLabApiService(gitlabUrl, config.ApiKey);
            }

            // Return a dummy service if no API key is available
            return new GitLabApiService("https://gitlab.com", "");
        });
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}
