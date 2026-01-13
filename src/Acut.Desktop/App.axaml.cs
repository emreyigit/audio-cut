using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Markup.Xaml;
using Acut.Desktop.ViewModels;
using Acut.Desktop.Views;
using Acut.Core.Audio;

namespace Acut.Desktop;

public partial class App : Application
{
    public static Window? MainWindow { get; private set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            DisableAvaloniaDataAnnotationValidation();

            // Create services
            var audioService = new AudioService();
            var playbackService = new PlaybackService();

            MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(audioService, playbackService),
            };
            desktop.MainWindow = MainWindow;

            // Clean up on exit
            desktop.Exit += (s, e) =>
            {
                playbackService?.Dispose();
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}
