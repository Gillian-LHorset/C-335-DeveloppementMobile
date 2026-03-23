namespace FlashCard;

public partial class TestPage : ContentPage
{
	public TestPage()
	{
		InitializeComponent();
	}

    private async void OnRetourClicked(object sender, EventArgs e) {
        await Shell.Current.GoToAsync("//MainPage");
    }
}