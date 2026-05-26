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
            Button book = new Button {
                Text = $"{epubFile.Title} - {epubFile.Author}",
                BindingContext = epubFile.Id,
                BackgroundColor = Colors.LightGray,
                Margin = new Thickness(0, 0, 10, 10),
                HorizontalOptions = LayoutOptions.Fill
            };

            book.Clicked += OnOpenBookClicked;
            BooksList.Children.Add(book);
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