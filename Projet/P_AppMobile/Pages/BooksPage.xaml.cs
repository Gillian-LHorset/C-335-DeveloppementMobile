using EpubSharp;
using P_AppMobile.Pages;
using SQLite;

namespace P_AppMobile.Resources.Pages;
public partial class BooksPage : ContentPage {
    public BooksPage() {
        InitializeComponent();
    }

    protected override async void OnAppearing() {
        base.OnAppearing();
        BooksList.Clear();
        List<Resources.Models.EpubFile> epubFiles = await GetAllEpubFiles();

        foreach (Resources.Models.EpubFile epubFile in epubFiles) {
            VerticalStackLayout bookContainer = new VerticalStackLayout {
                BackgroundColor = Colors.LightGray,
                Margin = new Thickness(0, 0, 10, 10),
                HorizontalOptions = LayoutOptions.Fill,
                Padding = new Thickness(10)
            };

            Image coverImage = new Image {
                HeightRequest = 150,
                HorizontalOptions = LayoutOptions.Center,
                Aspect = Aspect.AspectFit,
                Margin = new Thickness(0, 0, 0, 10),
                WidthRequest = 100,
                BackgroundColor = Colors.LightGray
            };

            // the discard is here because the Task send back a value that we don t use
            _ = Task.Run(async () => {
                try {
                    // path from the phone
                    string bookPath = epubFile.FilePath;

                    if (!File.Exists(bookPath)) {
                        return;
                    }

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

            Button bookButton = new Button {
                Text = $"{epubFile.Title}\n{epubFile.Author}",
                BindingContext = epubFile.Id,
                BackgroundColor = Colors.Transparent,
                TextColor = Colors.Black,
                HorizontalOptions = LayoutOptions.Fill
            };
            bookButton.Clicked += OnOpenBookClicked;

            bookContainer.Children.Add(coverImage);
            bookContainer.Children.Add(bookButton);
            BooksList.Children.Add(bookContainer);
        }
    }

    public async Task<List<Resources.Models.EpubFile>> GetAllEpubFiles() {
        var database = new SQLiteAsyncConnection(Path.Combine(FileSystem.AppDataDirectory, "epub.db"));
        await database.CreateTableAsync<Resources.Models.EpubFile>();
        return await database.Table<Resources.Models.EpubFile>().ToListAsync();
    }

    private async void OnOpenBookClicked(object sender, EventArgs e) {
        if (sender is Button clickedButton && clickedButton.BindingContext is int bookId) {
            await Shell.Current.GoToAsync($"{nameof(ReadPage)}?bookId={bookId}");
        }
    }
}