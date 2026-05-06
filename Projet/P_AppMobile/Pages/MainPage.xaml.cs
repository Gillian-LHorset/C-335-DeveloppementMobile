using EpubSharp;
using P_AppMobile.Resources.Models;
using SQLite;
namespace P_AppMobile {
    public partial class MainPage : ContentPage {

        string filePath;

        public MainPage() {
            InitializeComponent();
        }

        protected override async void OnAppearing() {
            base.OnAppearing();
        }


        private async void AddBookClick(object sender, EventArgs e) {
            filePath = await PickCustomFile();


        }

        public async Task<string> PickCustomFile() {
            var customFileType = new FilePickerFileType(
            new Dictionary<DevicePlatform, IEnumerable<string>> {
                { DevicePlatform.Android, new[] { "application/epub+zip", ".epub" } },
                { DevicePlatform.iOS, new[] { "org.idpf.epub-container", ".epub" } },
                { DevicePlatform.macOS, new[] { "org.idpf.epub-container", ".epub" } },
                { DevicePlatform.WinUI, new[] { ".epub" } }
            });

            PickOptions options = new() {
                PickerTitle = "Please select an EPUB file",
                FileTypes = customFileType,
            };

            var result = await FilePicker.Default.PickAsync(options);
            return result?.FullPath ?? string.Empty;
        }

        public async void DbConnexion() {
            var database = new SQLiteAsyncConnection(Path.Combine(FileSystem.AppDataDirectory, "epub.db"));
            await database.CreateTableAsync<EpubFile>();

            string filePath = await PickCustomFile();
            var epubBook = EpubReader.Read(filePath);
            await database.InsertAsync(new EpubFile {
                FilePath = filePath,
                Title = epubBook.Title
            });
        }
    }
}