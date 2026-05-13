using P_AppMobile.Pages;

namespace P_AppMobile {
    public partial class App : Application {
        public App() {
            InitializeComponent();

            MainPage = new AppShell();


            Routing.RegisterRoute("//ReadPage", typeof(ReadPage));
        }
    }
}
