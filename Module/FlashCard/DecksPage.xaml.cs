using FlashCard.Models;
using FlashCard.Pages;
using FlashCard.Services;

namespace FlashCard {
    public partial class DecksPage : ContentPage {
        private JsonDataService _dataService;
        private List<Deck> _decks;
        private List<Deck> _filteredDecks;
        private int _nextId = 1;

        public DecksPage() {
            InitializeComponent();
            _dataService = new JsonDataService();
            _decks = new List<Deck>();
            _filteredDecks = new List<Deck>();
            LoadDecks();
        }

        private async void LoadDecks() {
            List<Deck> loadedDecks = await _dataService.LoadDecksAsync();

            List<Models.Card> allCards = await _dataService.LoadAllCardsAsync();

            _decks.Clear();
            foreach (Deck deck in loadedDecks) {
                deck.CardCount = allCards.Count(c => c.DeckFk == deck.Id);
                _decks.Add(deck);
            }

            if (_decks.Any()) {
                _nextId = _decks.Max(d => d.Id) + 1;
            }

            ApplyFilter();
            UpdateInfo($"Chargé: {_decks.Count} deck(s)");
        }


        private void ApplyFilter() {
            string search = SearchEntry?.Text?.Trim() ?? string.Empty;

            if (string.IsNullOrEmpty(search)) {
                _filteredDecks = _decks.ToList();
            } else if (search.Contains('|')) {
                string[] orTerms = search.Split('|', StringSplitOptions.RemoveEmptyEntries);
                _filteredDecks = _decks
                    .Where(d => orTerms.Any(term =>
                        d.Name.Contains(term.Trim(), StringComparison.OrdinalIgnoreCase)))
                    .ToList();
            } else {
                string[] andTerms = search.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                _filteredDecks = _decks
                    .Where(d => andTerms.All(term =>
                        d.Name.Contains(term, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
            }

            DecksCollectionView.ItemsSource = null;
            DecksCollectionView.ItemsSource = _filteredDecks;
        }

        private void OnSearchTextChanged(object sender, TextChangedEventArgs e) {
            ApplyFilter();
            UpdateSuggestions(e.NewTextValue);

            string mode = string.Empty;
            string text = e.NewTextValue ?? string.Empty;
            if (text.Contains('|')) mode = " [OU]";
            else if (text.Contains(' ')) mode = " [ET]";

            UpdateInfo($"Filtre{mode}: '{text}' → {_filteredDecks.Count} résultat(s)");
        }

        private void OnClearSearchClicked(object sender, EventArgs e) {
            SearchEntry.Text = string.Empty;
            SuggestionsFrame.IsVisible = false;
            ApplyFilter();
            UpdateInfo("Filtre effacé");
        }


        private void OnSearchFocused(object sender, FocusEventArgs e) {
            UpdateSuggestions(SearchEntry.Text);
        }

        private void OnSearchUnfocused(object sender, FocusEventArgs e) {
            Task.Delay(200).ContinueWith(_ =>
                MainThread.BeginInvokeOnMainThread(() =>
                    SuggestionsFrame.IsVisible = false));
        }

        private void UpdateSuggestions(string? input) {
            string search = input?.Trim() ?? string.Empty;

            if (string.IsNullOrEmpty(search) || search.Length < 2) {
                SuggestionsFrame.IsVisible = false;
                return;
            }

            string lastTerm = search;
            if (search.Contains('|')) {
                string[] parts = search.Split('|');
                lastTerm = parts.Last().Trim();
            } else if (search.Contains(' ')) {
                string[] parts = search.Split(' ');
                lastTerm = parts.Last().Trim();
            }

            if (string.IsNullOrEmpty(lastTerm) || lastTerm.Length < 2) {
                SuggestionsFrame.IsVisible = false;
                return;
            }

            List<Deck> suggestions = _decks
                .Where(d => d.Name.Contains(lastTerm, StringComparison.OrdinalIgnoreCase))
                .Take(5)
                .ToList();

            if (suggestions.Count == 0) {
                SuggestionsFrame.IsVisible = false;
                return;
            }

            SuggestionsView.ItemsSource = suggestions;
            SuggestionsFrame.IsVisible = true;
        }

        private void OnSuggestionTapped(object sender, ItemTappedEventArgs e) {
            if (e.Item is Deck selected) {
                string currentText = SearchEntry.Text ?? string.Empty;

                int lastPipe = currentText.LastIndexOf('|');
                int lastSpace = currentText.LastIndexOf(' ');
                int lastSeparator = Math.Max(lastPipe, lastSpace);

                if (lastSeparator >= 0) {
                    string prefix = currentText.Substring(0, lastSeparator + 1);
                    SearchEntry.Text = prefix + selected.Name;
                } else {
                    SearchEntry.Text = selected.Name;
                }

                SuggestionsFrame.IsVisible = false;
                SuggestionsView.SelectedItem = null;
                ApplyFilter();
            }
        }

        private void UpdateInfo(string message) {
            InfoLabel.Text = $"{DateTime.Now:HH:mm:ss} - {message}";
        }

        /// <summary>
        /// Add a deck
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnAddDeckClicked(object sender, EventArgs e) {
            string? name = NewDeckEntry.Text?.Trim();

            if (string.IsNullOrEmpty(name)) {
                await DisplayAlert("Erreur", "Veuillez entrer un nom", "OK");
                return;
            }

            Deck newDeck = new Deck {
                Id = _nextId++,
                Name = name
            };

            _decks.Add(newDeck);
            await _dataService.SaveDecksAsync(_decks.ToList());

            ApplyFilter();
            NewDeckEntry.Text = string.Empty;
            UpdateInfo($"Ajouté: {name}");

        }

        public async void OnViewDeckClicked(object sender, EventArgs e) {
            Button? button = sender as Button;
            Deck? deck = button?.CommandParameter as Deck;

            ViewDeckPage dPage = new ViewDeckPage(deck);
            dPage.BindingContext = deck;
            await Navigation.PushAsync(dPage);
        }

        /// <summary>
        /// Delete a deck
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnEditDeckClicked(object sender, EventArgs e) {
            Button? button = sender as Button;
            Deck? deck = button?.CommandParameter as Deck;

            if (deck == null) return;

            // Navigate to edit page using Shell
            // Pass deck, dataService and decks list so EditDeckPage can save
            Dictionary<string, object> navigationParameter = new Dictionary<string, object>
            {
                { "deck", deck },
                { "dataService", _dataService },
                { "decks", _decks }
            };
            await Shell.Current.GoToAsync("EditDeck", navigationParameter);
        }

        // Refresh view when returning from edit page
        protected override void OnAppearing() {
            base.OnAppearing();
            LoadDecks();
        }

        private async void OnDeleteDeckClicked(object sender, EventArgs e) {
            Button? button = sender as Button;
            Deck? deck = button?.CommandParameter as Deck;

            if (deck == null) return;

            bool confirm = await DisplayAlert(
                "Confirmation",
                $"Voulez-vous vraiment supprimer '{deck.Name}' ?",
                "Supprimer",
                "Annuler"
            );

            if (!confirm) return;

            _decks.Remove(deck);
            await _dataService.SaveDecksAsync(_decks.ToList());

            ApplyFilter();
            UpdateInfo($"Supprimé: {deck.Name}");
        }
    }
}