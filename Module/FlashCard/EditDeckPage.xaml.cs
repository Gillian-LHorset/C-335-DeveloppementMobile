using FlashCard.Models;
using FlashCard.Services;
using System.Collections.ObjectModel;

namespace Flashcard {
    public partial class EditDeckPage : ContentPage, IQueryAttributable {
        private Deck _deck;
        private JsonDataService _dataService;
        private ObservableCollection<Deck> _decks;

        public EditDeckPage() {
            InitializeComponent();
        }

        // Receive navigation parameters
        public void ApplyQueryAttributes(IDictionary<string, object> query) {
            if (query.TryGetValue("deck", out object? deckObj) && deckObj is Deck deck) {
                _deck = deck;

                // Initialize fields
                NameEntry.Text = deck.Name;
            }

            if (query.TryGetValue("dataService", out object? serviceObj) && serviceObj is JsonDataService service) {
                _dataService = service;
            }

            if (query.TryGetValue("decks", out object? decksObj) && decksObj is ObservableCollection<Deck> decks) {
                _decks = decks;
            }
        }

        private async void OnSaveClicked(object sender, EventArgs e) {
            string? newName = NameEntry.Text?.Trim();

            if (string.IsNullOrWhiteSpace(newName)) {
                await DisplayAlert("Erreur", "Le nom ne peut pas etre vide", "OK");
                return;
            }

            // Update deck name
            _deck.Name = newName;

            // Save immediately to JSON
            await _dataService.SaveDecksAsync(_decks.ToList());

            await Shell.Current.GoToAsync("..");
        }

        private async void OnCancelClicked(object sender, EventArgs e) {
            await Shell.Current.GoToAsync("..");
        }
    }
}