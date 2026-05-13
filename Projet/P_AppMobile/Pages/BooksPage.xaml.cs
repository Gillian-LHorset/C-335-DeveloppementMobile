using P_AppMobile.Pages;
using SQLite;

namespace P_AppMobile.Resources.Pages;
public partial class BooksPage : ContentPage {
    public BooksPage() {
        InitializeComponent();
    }

    protected override async void OnAppearing() {
        List<Resources.Models.EpubFile> epubFiles = await GetAllEpubFiles();

        foreach (Resources.Models.EpubFile epubFile in epubFiles) {
            Button book = new Button {
                Text = epubFile.Title,
                BindingContext = epubFile.Id
            };

            book.Clicked += OnOpenBookClicked;
            BooksList.Add(book);
        }
    }

    public async Task<List<Resources.Models.EpubFile>> GetAllEpubFiles() {
        var database = new SQLiteAsyncConnection(Path.Combine(FileSystem.AppDataDirectory, "epub.db"));
        return await database.Table<Resources.Models.EpubFile>().ToListAsync();
    }

    private async void OnOpenBookClicked(object sender, EventArgs e) {
        await Shell.Current.GoToAsync($"{nameof(ReadPage)}?Id={1}");
    }
}