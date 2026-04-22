using EpubSharp;

namespace P_AppMobile {
    public partial class MainPage : ContentPage {

        private readonly HttpClient _httpClient = new HttpClient();
        String bookPath = "../Epub/don-quixote.epub";
        public MainPage() {
            InitializeComponent();
        }

        protected override async void OnAppearing() {
            base.OnAppearing();
            await LoadEpubAsync(bookPath);
        }

        public async Task LoadEpubAsync(string filePath) {
            EpubBook book = EpubReader.Read(filePath);

            Label label = new Label() {
                Text = book.Title,
            };

            BooksList.Add(label);
        }


        private async void AddBookClick(object sender, EventArgs e) {

        }
    }
}