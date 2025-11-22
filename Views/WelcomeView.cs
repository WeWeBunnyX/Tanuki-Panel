using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Threading;
using Avalonia.Animation;

namespace TanukiPanel.Views;

/// <summary>
/// A small welcome view that fades in a message, waits, then fades out and signals completion.
/// This uses simple manual opacity animation so no extra packages are required.
/// </summary>
    public class WelcomeView : UserControl
    {
        private readonly TextBlock _text;

        public WelcomeView()
        {
            _text = new TextBlock
            {
                Text = "Welcome to Tanuki Panel",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 28,
                Opacity = 0
            };

            Content = new Grid
            {
                Children = { _text }
            };


            _text.Transitions = new Transitions
            {
                new DoubleTransition
                { 
                     Property = TextBlock.OpacityProperty,
                    Duration = TimeSpan.FromMilliseconds(700)
                
                }
            };


            _ = RunSequenceAsync();
        }

    private async Task RunSequenceAsync()
    {
        try
        {
            await Dispatcher.UIThread.InvokeAsync(() => _text.Opacity = 1.0);
            await Task.Delay(1400);      
            await Dispatcher.UIThread.InvokeAsync(() => _text.Opacity = 0.0);
        }
        catch
        {
        }

        if (DataContext is ViewModels.WelcomeViewModel vm)
        {
            vm.OnAnimationFinished();
        }
    }
    
}
