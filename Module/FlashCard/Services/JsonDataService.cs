using FlashCard.Models;
using System.Diagnostics;
using System.Text.Json;

namespace FlashCard.Services {
    public class JsonDataService {
        private readonly string _deckFilePath;
        private readonly string _cardFilePath;

        public JsonDataService() {
            // Path to store the JSON file in app data
            _deckFilePath = Path.Combine(
                FileSystem.AppDataDirectory,
                "decks.json"
            );

            _cardFilePath = Path.Combine(
                FileSystem.AppDataDirectory,
                "cards.json"
            );
        }

        // about decks
        public async Task<List<Deck>> LoadDecksAsync() {
            try {
                if (!File.Exists(_deckFilePath)) {
                    return new List<Deck>();
                }

                string json = await File.ReadAllTextAsync(_deckFilePath);
                List<Deck>? decks = JsonSerializer.Deserialize<List<Deck>>(json);

                return decks ?? new List<Deck>();
            } catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine($"Error loading: {ex.Message}");
                return new List<Deck>();
            }
        }

        public async Task SaveDecksAsync(List<Deck> decks) {
            try {
                JsonSerializerOptions options = new JsonSerializerOptions {
                    WriteIndented = true
                };
                string json = JsonSerializer.Serialize(decks, options);
                await File.WriteAllTextAsync(_deckFilePath, json);
            } catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine($"Error saving: {ex.Message}");
            }
        }

        // about cards

        /// <summary>
        /// Load all cards from the JSON file (all decks)
        /// </summary>
        public async Task<List<Card>> LoadAllCardsAsync() {
            try {
                if (!File.Exists(_cardFilePath)) {
                    return new List<Card>();
                }

                string json = await File.ReadAllTextAsync(_cardFilePath);
                List<Card>? cards = JsonSerializer.Deserialize<List<Card>>(json);

                return cards ?? new List<Card>();
            } catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine($"Error loading all cards: {ex.Message}");
                return new List<Card>();
            }
        }

        /// <summary>
        /// Load cards for a specific deck
        /// </summary>
        public async Task<List<Card>> LoadCardsAsync(int deckFk) {
            List<Card> allCards = await LoadAllCardsAsync();
            return allCards.Where(card => card.DeckFk == deckFk).ToList();
        }

        /// <summary>
        /// Save cards for a specific deck, preserving cards from other decks
        /// </summary>
        public async Task SaveCardsForDeckAsync(int deckFk, List<Card> deckCards) {
            try {
                // Load all existing cards
                List<Card> allCards = await LoadAllCardsAsync();

                // Remove old cards for this deck
                allCards.RemoveAll(c => c.DeckFk == deckFk);

                // Add the updated cards for this deck
                allCards.AddRange(deckCards);

                // Save all cards back
                await SaveCardsAsync(allCards);
            } catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine($"Error saving cards for deck: {ex.Message}");
            }
        }

        public async Task SaveCardsAsync(List<Card> cards) {
            try {
                JsonSerializerOptions options = new JsonSerializerOptions {
                    WriteIndented = true
                };
                string json = JsonSerializer.Serialize(cards, options);
                await File.WriteAllTextAsync(_cardFilePath, json);
            } catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine($"Error saving: {ex.Message}");
            }
        }

        public string GetFilePath() {
            return _deckFilePath;
        }
    }
}