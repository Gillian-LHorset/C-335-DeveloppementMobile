using FlashCard.Models;
using FlashCard.Services;
using System.Collections.ObjectModel;

namespace FlashCard.Pages;

public partial class ViewDeckPage : ContentPage {
    private JsonDataService _dataService;
    private ObservableCollection<Card> _cards;
    private Deck _deck;
    private int _nextId = 1;

    public ViewDeckPage() {
        InitializeComponent();
        _dataService = new JsonDataService();
        _cards = new ObservableCollection<Card>();
        LoadCards();
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query) {
        if (query.TryGetValue("card", out object? cardObj) && cardObj is Card card) {
            _card = card;
        }

        if (query.TryGetValue("dataService", out object? serviceObj) && serviceObj is JsonDataService service) {
            _dataService = service;
        }
    }

    private async void LoadCards() {
        if (_deck != null) {
            List<Card> loadedCards = await _dataService.LoadCardsAsync(_deck.Id);

            _cards.Clear();
            foreach (Card card in loadedCards) {
                _cards.Add(card);
            }

            if (_cards.Any()) {
                _nextId = _cards.Max(d => d.Id) + 1;
            }

            // Assign ItemsSource ONCE (no need to reassign every time)
            if (CardsCollectionView.ItemsSource == null) {
                CardsCollectionView.ItemsSource = _cards;
            }

            UpdateInfo($"Chargé: {_cards.Count} deck(s)");
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

        _cards.Add(newCard);  // ← La vue se met à jour automatiquement !
        await _dataService.SaveCardsAsync(_cards.ToList());

        // RefreshView();  ← SUPPRIMÉ !
        NewCardRectoEntry.Text = string.Empty;
        NewCardVersoEntry.Text = string.Empty;
        UpdateInfo($"Carte ajoutée.");
    }

    private async void OnEditCardClicked(object sender, EventArgs e) {
        Button? button = sender as Button;
        Card? card = button?.CommandParameter as Card;

        if (card == null) return;

        // Navigate to edit page using Shell
        // Pass deck, dataService and decks list so EditDeckPage can save
        Dictionary<string, object> navigationParameter = new Dictionary<string, object>
        {
        { "card", card },
        { "dataService", _dataService },
        { "cards", _cards }
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

        // Remove deck
        _cards.Remove(card);
        await _dataService.SaveCardsAsync(_cards);

        RefreshView();
        UpdateInfo($"Supprimé: {card.Recto}");
    }

    private void RefreshView() {
        CardsCollectionView.ItemsSource = null;
        CardsCollectionView.ItemsSource = _cards;
    }

    private void UpdateInfo(string message) {
        InfoLabel.Text = $"{DateTime.Now:HH:mm:ss} - {message}";
    }
}