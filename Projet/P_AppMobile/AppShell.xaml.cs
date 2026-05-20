using P_AppMobile.Pages;

namespace P_AppMobile {
    public partial class AppShell : Shell {
        public AppShell() {
            InitializeComponent();

            Routing.RegisterRoute(nameof(ReadPage), typeof(ReadPage));
        }
    }
}
