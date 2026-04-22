using FlashCard.Models;
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
        public async Task<List<Card>> LoadCardsAsync(int deckFk) {
            try {
                if (!File.Exists(_cardFilePath)) {
                    return new List<Card>();
                }

                string json = await File.ReadAllTextAsync(_cardFilePath);
                List<Card>? cards = JsonSerializer.Deserialize<List<Card>>(json);

                if (cards == null) {
                    return new List<Card>();
                }

                // On retourne directement la liste filtrée
                return cards.Where(card => card.DeckFk == deckFk).ToList();

            } catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine($"Error loading: {ex.Message}");
                return new List<Card>();
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