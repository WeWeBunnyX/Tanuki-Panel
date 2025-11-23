using System;

namespace TanukiPanel.ViewModels;

public class WelcomeViewModel : ViewModelBase
{
    private readonly Action<ViewModelBase> _navigate;

    public WelcomeViewModel(Action<ViewModelBase> navigate)
    {
        _navigate = navigate;
    }

    public void OnAnimationFinished()
    {
        _navigate(new ApiKeyViewModel(_navigate));
    }
}
