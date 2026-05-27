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

                // the container of the book
                VerticalStackLayout bookContainer = new VerticalStackLayout {
                    BackgroundColor = Colors.LightGray,
                    Margin = new Thickness(0, 0, 10, 10),
                    HorizontalOptions = LayoutOptions.Fill,
                    Padding = new Thickness(10),
                    BindingContext = epubFile.Id
                };

                Image coverImage = new Image {
                    HeightRequest = 150,
                    HorizontalOptions = LayoutOptions.Center,
                    Aspect = Aspect.AspectFit,
                    Margin = new Thickness(0, 0, 0, 10),
                    WidthRequest = 100,
                    BackgroundColor = Colors.LightGray,
                    InputTransparent = true
                };

                _ = Task.Run(async () => {
                    try {
                        // path from the phone
                        string bookPath = epubFile.FilePath;

                        if (!File.Exists(bookPath)) {
                            return;
                        }

                        // get all infos from the book
                        EpubBook book = EpubReader.Read(bookPath);

                        if (book.CoverImage != null && book.CoverImage.Length > 0) {
                            // image for the cache
                            string fileName = Path.GetFileName(bookPath);
                            string coverPath = Path.Combine(FileSystem.CacheDirectory, fileName + ".jpg");

                            if (!File.Exists(coverPath)) {
                                await File.WriteAllBytesAsync(coverPath, book.CoverImage);
                            }

                            MainThread.BeginInvokeOnMainThread(() => {
                                // change the cover dynamicaly
                                coverImage.Source = ImageSource.FromFile(coverPath);
                            });
                        }
                    } catch (Exception ex) {
                    }
                });

                Label bookInfoLabel = new Label {
                    Text = $"{epubFile.Title}\n{epubFile.Author}",
                    // when is clicked, redirect to the read page with the id of the book from the db
                    BindingContext = epubFile.Id,
                    BackgroundColor = Colors.Transparent,
                    TextColor = Colors.Black,
                    HorizontalOptions = LayoutOptions.Fill,
                    InputTransparent = true
                };

                // simulate the OnClick method
                var tapGesture = new TapGestureRecognizer();
                tapGesture.Tapped += (sender, e) => {
                    OnOpenBookClicked(sender, e);
                };
                bookContainer.GestureRecognizers.Add(tapGesture);

                // add to the frontend
                bookContainer.Children.Add(coverImage);
                bookContainer.Children.Add(bookInfoLabel);
                RecentBook.Children.Add(bookContainer);
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

        private async void OnOpenBookClicked(object sender, EventArgs e) {
            if (sender is VisualElement clickedElement && clickedElement.BindingContext is int bookId) {
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
            await LoadRecentBooks();
            await DisplayAlert("Succès", "Livre chargé : " + epubBook.Title, "OK");
        }

        public async Task<List<Resources.Models.EpubFile>> GetAllEpubFiles() {
            var database = new SQLiteAsyncConnection(Path.Combine(FileSystem.AppDataDirectory, "epub.db"));
            await database.CreateTableAsync<Resources.Models.EpubFile>();
            return await database.Table<Resources.Models.EpubFile>().ToListAsync();
        }
    }
}