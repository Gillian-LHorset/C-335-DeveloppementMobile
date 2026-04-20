using Flashcard;

namespace FlashCard {
    public partial class App : Application {
        public App() {
            InitializeComponent();

            // Register navigation routes
            Routing.RegisterRoute("EditDeck", typeof(EditDeckPage));

            Routing.RegisterRoute("ViewDeckPage", typeof(Pages.ViewDeckPage));

            MainPage = new AppShell();
        }
    }
}
