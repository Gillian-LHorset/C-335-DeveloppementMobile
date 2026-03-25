namespace P_AppMobile;

public partial class NewTestPage : ContentPage {
    public NewTestPage() {
        InitializeComponent();
        MainPage.Header(layout);
    }

    private async void BackHomePage(object sender, EventArgs e) {
        await Shell.Current.GoToAsync("//MainPage");
    }


}