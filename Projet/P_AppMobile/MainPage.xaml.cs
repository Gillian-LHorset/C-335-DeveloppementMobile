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

            Grid grid = new Grid {
                RowDefinitions =
                {
                    new RowDefinition { Height = new GridLength(100) }
                },
                ColumnDefinitions =
                {
                    new ColumnDefinition(),
                    new ColumnDefinition(),
                    new ColumnDefinition()
                }
            };

            Label label = new Label {
                WidthRequest = 500,
                HeightRequest = 50,
                Text = "test",
                BackgroundColor = Color.FromRgb(230, 230, 230),
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Start
            };

            grid.Add(label, 0, 0);
            grid.Add(label, 0, 2);


            layout.Children.Insert(0, grid); // TODO : ne fonctionne pas, à regler

        }
    }

}
