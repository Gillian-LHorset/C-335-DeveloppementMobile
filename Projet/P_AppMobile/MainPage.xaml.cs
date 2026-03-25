namespace P_AppMobile {
    public partial class MainPage : ContentPage {
        public MainPage() {
            InitializeComponent();
            Header(layout);
        }

        private async void OnCounterClicked(object sender, EventArgs e) {
            await Shell.Current.GoToAsync("//TestPage");
        }

        public static void Header(Layout layout) {

            Label label = new Label {
                WidthRequest = 500,
                HeightRequest = 50,
                Text = "test",
                BackgroundColor = Color.FromRgb(230, 230, 230),
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Start

            };


            layout.Children.Insert(0, label);
        }
    }

}
