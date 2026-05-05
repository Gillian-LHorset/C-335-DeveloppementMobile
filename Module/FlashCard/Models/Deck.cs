using System.Text.Json.Serialization;

namespace FlashCard.Models {
    public class Deck {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Computed dynamically, not persisted in JSON
        /// </summary>
        [JsonIgnore]
        public int CardCount { get; set; }

        public Deck() {
            CreatedDate = DateTime.Now;
        }

        public override string ToString() {
            return $"{Name} ({CardCount} cards)";
        }
    }
}
