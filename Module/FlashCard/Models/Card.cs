namespace FlashCard.Models {
    public class Card {

        public int Id { get; set; }
        public string Recto { get; set; } = string.Empty;
        public string Verso { get; set; } = string.Empty;
        public int DeckFk { get; set; }
        public DateTime CreatedDate { get; set; }

        public Card() {
            CreatedDate = DateTime.Now;
        }
    }
}
