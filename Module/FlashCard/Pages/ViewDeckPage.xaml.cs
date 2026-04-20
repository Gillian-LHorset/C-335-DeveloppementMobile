using FlashCard.Models;
using FlashCard.Services;

namespace FlashCard.Pages;

public partial class ViewDeckPage : ContentPage {
    private JsonDataService _dataService;
    private List<Card> _cards;
    private Deck _deck;
    private int _nextId = 1;

    public ViewDeckPage() {
        InitializeComponent();
        _dataService = new JsonDataService();
        _cards = new List<Card>();
        LoadCards();
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query) {
        if (query.TryGetValue("deck", out object? deckObj) && deckObj is Deck deck) {
            _deck = deck;
        }

        if (query.TryGetValue("dataService", out object? serviceObj) && serviceObj is JsonDataService service) {
            _dataService = service;
        }
    }

    private async void LoadCards() {
        if (_deck != null) {
            _cards = await _dataService.LoadCardsAsync(_deck.Id);

            // Calculate next ID
            if (_cards.Any()) {
                _nextId = _cards.Max(d => d.Id) + 1;
            }

            RefreshView();
            UpdateInfo($"Chargé: {_cards.Count} deck(s)");
        }
    }

    private void RefreshView() {
        CardsCollectionView.ItemsSource = null;
        CardsCollectionView.ItemsSource = _cards;
    }

    private void UpdateInfo(string message) {
        InfoLabel.Text = $"{DateTime.Now:HH:mm:ss} - {message}";
    }
}