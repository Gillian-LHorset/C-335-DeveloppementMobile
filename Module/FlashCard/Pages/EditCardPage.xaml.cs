namespace FlashCard.Pages;

using FlashCard.Models;
using FlashCard.Services;

public partial class EditCardPage : ContentPage, IQueryAttributable {
    private Card _card;
    private JsonDataService _dataService;
    private List<Card> _cards;

    public EditCardPage() {
        InitializeComponent();
    }

    // Receive navigation parameters
    public void ApplyQueryAttributes(IDictionary<string, object> query) {
        if (query.TryGetValue("card", out object? deckObj) && deckObj is Card card) {
            _card = card;

            // Initialize fields
            RectoEntry.Text = card.Recto;
            VersoEntry.Text = card.Verso;
        }

        if (query.TryGetValue("dataService", out object? serviceObj) && serviceObj is JsonDataService service) {
            _dataService = service;
        }

        if (query.TryGetValue("cards", out object? decksObj) && decksObj is List<Card> cards) {
            _cards = cards;
        }
    }

    private async void OnSaveClicked(object sender, EventArgs e) {
        string? newRecto = RectoEntry.Text?.Trim();
        string? newVerso = VersoEntry.Text?.Trim();

        if (string.IsNullOrWhiteSpace(newRecto)) {
            await DisplayAlert("Erreur", "La question ne peut pas être vide", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(newVerso)) {
            await DisplayAlert("Erreur", "La réponse ne peut pas être vide", "OK");
            return;
        }

        // Update deck
        _card.Recto = newRecto;
        _card.Verso = newVerso;

        // Save immediately to JSON
        await _dataService.SaveCardsAsync(_cards);

        await Shell.Current.GoToAsync("..");
    }

    private async void OnCancelClicked(object sender, EventArgs e) {
        await Shell.Current.GoToAsync("..");
    }
}