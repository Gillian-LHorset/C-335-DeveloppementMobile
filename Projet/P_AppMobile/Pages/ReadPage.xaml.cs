using EpubSharp;
using SQLite;

namespace P_AppMobile.Pages;

[QueryProperty(nameof(BookId), "bookId")]
public partial class ReadPage : ContentPage {
    public int BookId { get; set; }

    public ReadPage() {
        InitializeComponent();
    }

    protected override async void OnAppearing() {
        base.OnAppearing();

        await DisplayAlert(BookId.ToString(), "hello", "hello");
        if (BookId > 0) {
            await DisplayAlert("hello2", "hello2", "hello2");
            await LoadEpubFileFromDb(BookId);
        }
    }

    private async Task LoadEpubFileFromDb(int bookId) {
        var database = new SQLiteAsyncConnection(Path.Combine(FileSystem.AppDataDirectory, "epub.db"));
        var book = await database.Table<Resources.Models.EpubFile>()
                                 .FirstOrDefaultAsync(b => b.Id == bookId);

        if (book != null) {
            await LoadEpubFile(book.FilePath);
        }
    }

    private async Task LoadEpubFile(string filePath) {
        try {
            if (File.Exists(filePath)) {
                var epubBook = EpubReader.Read(filePath);
                TitleLabel.Text = epubBook.Title;
                string plainText = epubBook.ToPlainText();
                ContentEditor.Text = plainText;
            }
        } catch (Exception ex) {
            ContentEditor.Text = $"Erreur : {ex.Message}";
        }
    }
}