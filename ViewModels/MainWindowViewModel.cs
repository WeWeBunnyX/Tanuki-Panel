namespace TanukiPanel.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
	private ViewModelBase? _currentViewModel;

	public ViewModelBase? CurrentViewModel
	{
		get => _currentViewModel;
		set => SetProperty(ref _currentViewModel, value);
	}

	public MainWindowViewModel()
	{
		// Start with the welcome VM and provide a navigation callback
		CurrentViewModel = new WelcomeViewModel(NavigateTo);
	}

	private void NavigateTo(ViewModelBase vm) => CurrentViewModel = vm;
}
