using Android.App;
using Android.Content.PM;
using Android.Database;
using Android.OS; 
using System.Text;
using System.Xml;

namespace CheckAllocationApp
{
    [Activity(Theme = "@style/MyTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Platforms.Android.DangerousTrustProvider.Register();

        }
    }
}