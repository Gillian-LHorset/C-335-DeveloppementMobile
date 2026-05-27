using EpubSharp;
using SQLite;

namespace P_AppMobile.Pages;

[QueryProperty(nameof(BookId), "bookId")]
public partial class ReadPage : ContentPage {
    public int BookId { get; set; }

    private List<string> _pages = new List<string>();
    private int _currentPageIndex = 0;
    public ReadPage() {
        InitializeComponent();
    }

    protected override async void OnAppearing() {
        base.OnAppearing();

        if (BookId > 0) {
            await LoadEpubFileFromDb(BookId);
        }
    }

    private async Task LoadEpubFileFromDb(int bookId) {
        // db connect
        var database = new SQLiteAsyncConnection(Path.Combine(FileSystem.AppDataDirectory, "epub.db"));
        // get the book from the db where the id is equal to the id pass by the navigation
        var book = await database.Table<Resources.Models.EpubFile>()
                                 .FirstOrDefaultAsync(b => b.Id == bookId);

        if (book != null) {
            TitleLabel.Text = book.Title;
            AuthorLabel.Text = book.Author;
            await LoadEpubFile(book.FilePath, book.LastReadPage);
        }
    }

    private async Task LoadEpubFile(string filePath, int lastReadPage) {
        try {
            if (File.Exists(filePath)) {
                // get all data of a book
                var epubBook = await Task.Run(() => EpubReader.Read(filePath));
                // the all text of the book
                string plainText = await Task.Run(() => epubBook.ToPlainText());

                // 1500 = the size of the page
                _pages = SplitTextIntoPages(plainText, 1500);
                _currentPageIndex = Math.Min(lastReadPage, _pages.Count - 1);
                if (_currentPageIndex < 0) {
                    _currentPageIndex = 0;
                }

                MainThread.BeginInvokeOnMainThread(() => {
                    DisplayCurrentPage();
                });
            } else {
                ContentEditor.Text = "Erreur : Fichier introuvable.";
            }
        } catch (Exception ex) {
            ContentEditor.Text = $"Erreur lors du chargement : {ex.Message}";
        }
    }

    private List<string> SplitTextIntoPages(string text, int pageSize) {
        var pages = new List<string>();
        if (string.IsNullOrWhiteSpace(text)) {
            pages.Add("Aucun contenu disponible.");
            return pages;
        }

        int index = 0;
        while (index < text.Length) {
            if (index + pageSize >= text.Length) {
                pages.Add(text.Substring(index).Trim());
                break;
            }

            int targetEnd = index + pageSize;
            int actualEnd = targetEnd;

            for (int i = 0; i < 100; i++) {
                if (targetEnd - i < text.Length && targetEnd - i >= index) {
                    char c = text[targetEnd - i];
                    if (char.IsWhiteSpace(c)) {
                        actualEnd = targetEnd - i;
                        break;
                    }
                }
            }

            string pageContent = text.Substring(index, actualEnd - index).Trim();
            if (!string.IsNullOrEmpty(pageContent)) {
                pages.Add(pageContent);
            }
            index = actualEnd;
        }

        if (pages.Count == 0) {
            pages.Add("Aucun contenu disponible.");
        }
        return pages;
    }

    private void DisplayCurrentPage() {
        if (_pages == null || _pages.Count == 0) {
            ContentEditor.Text = "Livre vide.";
            PageIndicatorLabel.Text = "Page 0 / 0";
            PrevButton.IsEnabled = false;
            NextButton.IsEnabled = false;
            return;
        }

        ContentEditor.Text = _pages[_currentPageIndex];
        PageIndicatorLabel.Text = $"Page {_currentPageIndex + 1} / {_pages.Count}";

        PrevButton.IsEnabled = _currentPageIndex > 0;
        NextButton.IsEnabled = _currentPageIndex < _pages.Count - 1;
    }

    private async Task SaveCurrentPageIndex() {
        if (BookId > 0) {
            try {
                var database = new SQLiteAsyncConnection(Path.Combine(FileSystem.AppDataDirectory, "epub.db"));
                var book = await database.Table<Resources.Models.EpubFile>()
                                         .FirstOrDefaultAsync(b => b.Id == BookId);
                if (book != null) {
                    book.LastReadPage = _currentPageIndex;
                    await database.UpdateAsync(book);
                }
            } catch {
            }
        }
    }

    private async void OnPrevPageClicked(object sender, EventArgs e) {
        if (_currentPageIndex > 0) {
            _currentPageIndex--;
            DisplayCurrentPage();
            await SaveCurrentPageIndex();
        }
    }

    private async void OnNextPageClicked(object sender, EventArgs e) {
        if (_currentPageIndex < _pages.Count - 1) {
            _currentPageIndex++;
            DisplayCurrentPage();
            await SaveCurrentPageIndex();
        }
    }
}