namespace CheckAllocationApp
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new AppShell() { };
        }
    }
}