using Flashcard;
using FlashCard.Pages;

namespace FlashCard {
    public partial class App : Application {
        public App() {
            InitializeComponent();

            // Register navigation routes
            Routing.RegisterRoute("EditDeck", typeof(EditDeckPage));

            Routing.RegisterRoute("EditCard", typeof(EditCardPage));

            Routing.RegisterRoute("ViewDeckPage", typeof(Pages.ViewDeckPage));

            Routing.RegisterRoute("QuizPage", typeof(Pages.QuizPage));

            MainPage = new AppShell();
        }
    }
}
