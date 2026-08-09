using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;

namespace BaZi {
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity {
        protected override void OnCreate(Bundle? savedInstanceState) {
            base.OnCreate(savedInstanceState);
            SetStatusBarTheme(isDarkMode: true);
        }

        public void SetStatusBarTheme(bool isDarkMode) {
            var window = Window;
            if (window is null) {
                return;
            }

            if (OperatingSystem.IsAndroidVersionAtLeast(30)) {
                var insetsController = window.InsetsController;
                if (insetsController is null) {
                    return;
                }

                var lightStatusBars = (int)WindowInsetsControllerAppearance.LightStatusBars;
                insetsController.SetSystemBarsAppearance(isDarkMode ? 0 : lightStatusBars, lightStatusBars);
                return;
            }

#pragma warning disable CS0618
            var systemUiVisibility = window.DecorView.SystemUiVisibility;
            var lightStatusBar = (StatusBarVisibility)SystemUiFlags.LightStatusBar;
            window.DecorView.SystemUiVisibility = isDarkMode
                ? systemUiVisibility & ~lightStatusBar
                : systemUiVisibility | lightStatusBar;
#pragma warning restore CS0618
        }
    }
}
