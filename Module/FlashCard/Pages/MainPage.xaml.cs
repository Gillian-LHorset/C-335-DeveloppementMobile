namespace FlashCard {
    public partial class MainPage : ContentPage {
        int count = 0;

        public MainPage() {
            InitializeComponent();
        }
        private async void OnProfilClicked(object sender, EventArgs e) {
            await Shell.Current.GoToAsync("//DecksPage");
        }

    }

}
