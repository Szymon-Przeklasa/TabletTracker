using TabletTracker.Pages;

namespace TabletTracker
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute("AssignStation", typeof(AssignStationPage));
            Routing.RegisterRoute("AssignClass", typeof(AssignClassPage));
        }
    }
}
