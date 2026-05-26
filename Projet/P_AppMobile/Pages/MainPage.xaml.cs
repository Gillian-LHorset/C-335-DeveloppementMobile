using EpubSharp;
using P_AppMobile.Pages;
using SQLite;
namespace P_AppMobile {
    public partial class MainPage : ContentPage {

        public MainPage() {
            InitializeComponent();
        }

        protected override async void OnAppearing() {
            base.OnAppearing();
            await LoadRecentBooks();
        }

        private async Task LoadRecentBooks() {
            RecentBook.Clear();
            List<Resources.Models.EpubFile> epubFiles = await GetLatestEpubFiles(5);
            foreach (Resources.Models.EpubFile epubFile in epubFiles) {
                Button bookButton = new Button {
                    Text = $"{epubFile.Title} - {epubFile.Author}",
                    BindingContext = epubFile.Id,
                    BackgroundColor = Colors.LightGray,
                    HorizontalOptions = LayoutOptions.Fill
                };
                bookButton.Clicked += OnBookButtonClicked;
                RecentBook.Add(bookButton);
            }
        }
        public async Task<List<Resources.Models.EpubFile>> GetLatestEpubFiles(int limit = 5) {
            var database = new SQLiteAsyncConnection(Path.Combine(FileSystem.AppDataDirectory, "epub.db"));
            await database.CreateTableAsync<Resources.Models.EpubFile>();
            return await database.Table<Resources.Models.EpubFile>()
                                 .OrderByDescending(b => b.UploadedAt)
                                 .Take(limit)
                                 .ToListAsync();
        }

        private async void OnBookButtonClicked(object? sender, EventArgs e) {
            if (sender is Button clickedButton && clickedButton.BindingContext is int bookId) {
                await Shell.Current.GoToAsync($"{nameof(ReadPage)}?bookId={bookId}");
            }
        }


        private async void AddBookClick(object sender, EventArgs e) {
            AddEpubFile();
            //filePath = await PickCustomFile();


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

        public async void AddEpubFile() {
            var database = new SQLiteAsyncConnection(Path.Combine(FileSystem.AppDataDirectory, "epub.db"));
            await database.CreateTableAsync<Resources.Models.EpubFile>();

            string filePath = await PickCustomFile();
            var epubBook = EpubReader.Read(filePath);

            string title = epubBook.Title ?? "Titre inconnu";
            string author = epubBook.Authors?.FirstOrDefault() ?? "Auteur inconnu";
            string uploadedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            var existingBook = await database.Table<Resources.Models.EpubFile>().FirstOrDefaultAsync(b => b.Title == title);

            if (existingBook != null) {
                existingBook.FilePath = filePath;
                existingBook.Author = author;
                existingBook.UploadedAt = uploadedAt;
                await database.UpdateAsync(existingBook);
                await LoadRecentBooks();
                await DisplayAlert("Succès", $"Livre mis à jour : {title}", "OK");
                return;
            }

            await database.InsertAsync(new Resources.Models.EpubFile {
                Title = title,
                Author = author,
                FilePath = filePath,
                UploadedAt = uploadedAt
            });


            Label label = new Label {
                Text = title
            };
            RecentBook.Add(label);

            await DisplayAlert("Succès", "Livre chargé : " + epubBook.Title, "OK");
        }

        public async Task<List<Resources.Models.EpubFile>> GetAllEpubFiles() {
            var database = new SQLiteAsyncConnection(Path.Combine(FileSystem.AppDataDirectory, "epub.db"));
            await database.CreateTableAsync<Resources.Models.EpubFile>();
            return await database.Table<Resources.Models.EpubFile>().ToListAsync();
        }
    }
}