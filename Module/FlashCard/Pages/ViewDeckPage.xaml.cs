using FlashCard.Models;
using FlashCard.Services;
using System.Collections.ObjectModel;

namespace FlashCard.Pages;

public partial class ViewDeckPage : ContentPage {
    private JsonDataService _dataService;
    private ObservableCollection<Card> _cards;
    private Deck _deck;
    private int _nextId = 1;

    public ViewDeckPage(Deck deck) {
        InitializeComponent();
        _deck = deck;
        _dataService = new JsonDataService();
        _cards = new ObservableCollection<Card>();
        LoadCards();
    }

    private async void LoadCards() {
        if (_deck == null) return;

        List<Card> loadedCards = await _dataService.LoadCardsAsync(_deck.Id);

        _cards.Clear();
        foreach (Card card in loadedCards) {
            _cards.Add(card);
        }

        // Calculate next ID from ALL cards (not just this deck) to avoid collisions
        List<Card> allCards = await _dataService.LoadAllCardsAsync();
        if (allCards.Any()) {
            _nextId = allCards.Max(c => c.Id) + 1;
        }

        // Assign ItemsSource ONCE (no need to reassign every time)
        if (CardsCollectionView.ItemsSource == null) {
            CardsCollectionView.ItemsSource = _cards;
        }
    }

    private async void OnAddCardClicked(object sender, EventArgs e) {
        string? recto = NewCardRectoEntry.Text?.Trim();
        string? verso = NewCardVersoEntry.Text?.Trim();

        if (string.IsNullOrEmpty(recto)) {
            await DisplayAlert("Erreur", "Veuillez entrer une question à la carte.", "OK");
            return;
        }

        if (string.IsNullOrEmpty(verso)) {
            await DisplayAlert("Erreur", "Veuillez entrer une réponse à la carte.", "OK");
            return;
        }

        Card newCard = new Card {
            Id = _nextId++,
            Recto = recto,
            Verso = verso,
            DeckFk = _deck.Id
        };

        _cards.Add(newCard);
        await _dataService.SaveCardsForDeckAsync(_deck.Id, _cards.ToList());

        NewCardRectoEntry.Text = string.Empty;
        NewCardVersoEntry.Text = string.Empty;
        UpdateInfo($"Carte ajoutée.");
    }

    private async void OnEditCardClicked(object sender, EventArgs e) {
        Button? button = sender as Button;
        Card? card = button?.CommandParameter as Card;

        if (card == null) return;

        // Navigate to edit page, pass card, dataService and cards list
        Dictionary<string, object> navigationParameter = new Dictionary<string, object>
        {
            { "card", card },
            { "dataService", _dataService },
            { "cards", _cards },
            { "deckId", _deck.Id }
        };
        await Shell.Current.GoToAsync("EditCard", navigationParameter);
    }

    private async void OnDeleteCardClicked(object sender, EventArgs e) {
        Button? button = sender as Button;
        Card? card = button?.CommandParameter as Card;

        if (card == null) return;

        // Confirm deletion
        bool confirm = await DisplayAlert(
            "Confirmation",
            $"Voulez-vous vraiment supprimer la question '{card.Recto}' ?",
            "Supprimer",
            "Annuler"
        );

        if (!confirm) return;

        // Remove card
        _cards.Remove(card);
        await _dataService.SaveCardsForDeckAsync(_deck.Id, _cards.ToList());

        UpdateInfo($"Supprimé: {card.Recto}");
    }

    private void RefreshView() {
        CardsCollectionView.ItemsSource = null;
        CardsCollectionView.ItemsSource = _cards;
    }

    private void UpdateInfo(string message) {
        InfoLabel.Text = $"{DateTime.Now:HH:mm:ss} - {message}";
    }

    protected override void OnAppearing() {
        base.OnAppearing();

        // Reload cards to reflect modifications from EditCardPage
        LoadCards();
    }
}