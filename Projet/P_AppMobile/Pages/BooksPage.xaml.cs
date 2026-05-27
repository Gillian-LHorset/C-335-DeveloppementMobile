using EpubSharp;
using P_AppMobile.Pages;
using SQLite;

namespace P_AppMobile.Resources.Pages;
public partial class BooksPage : ContentPage {
    public BooksPage() {
        InitializeComponent();
    }

    /// <summary>
    /// Charge all books' name and cover from the db and display them on the page
    /// </summary>
    protected override async void OnAppearing() {
        base.OnAppearing();
        BooksList.Clear();

        //load every epub (the complet model) from the db
        List<Resources.Models.EpubFile> epubFiles = await GetAllEpubFiles();

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

            // the discard is here because the Task send back a value that we don t use
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
            BooksList.Children.Add(bookContainer);
        }
    }

    public async Task<List<Resources.Models.EpubFile>> GetAllEpubFiles() {
        var database = new SQLiteAsyncConnection(Path.Combine(FileSystem.AppDataDirectory, "epub.db"));
        await database.CreateTableAsync<Resources.Models.EpubFile>();
        return await database.Table<Resources.Models.EpubFile>().ToListAsync();
    }

    private async void OnOpenBookClicked(object sender, EventArgs e) {
        if (sender is VisualElement clickedElement && clickedElement.BindingContext is int bookId) {
            await Shell.Current.GoToAsync($"{nameof(ReadPage)}?bookId={bookId}");
        }
    }
}