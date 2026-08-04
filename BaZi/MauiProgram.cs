using BaZi.Services;
using Fluxor;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;
using System.Reflection;

#if WINDOWS
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Windows.Graphics;
using WinRT.Interop;
#endif

namespace BaZi {
    public static class MauiProgram {
        public static MauiApp CreateMauiApp() {
            /* 取得建立程式的建置器 */
            var builder = MauiApp.CreateBuilder();
            builder.UseMauiApp<App>()
                .ConfigureFonts(    // 將字型加入程式
                    fonts => fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular")
                )
#if WINDOWS
                .ConfigureLifecycleEvents(  // 若為 Win APP (WinUI 3)，則設定預設視窗大小，並讓視窗置中
                    events => events.AddWindows(
                        windows => windows.OnWindowCreated(
                            window => {
                                // 取得視窗的原生控制權
                                var nativeWindow = window as MauiWinUIWindow;
                                var windowHandle = WindowNative.GetWindowHandle(nativeWindow);
                                var windowId = Win32Interop.GetWindowIdFromWindow(windowHandle);
                                var appWindow = AppWindow.GetFromWindowId(windowId);

                                // 設定程式開啟時的寬度與高度 (單位為像素)
                                var width = 1400;
                                var height = 800;
                                appWindow.Resize(new SizeInt32(width, height));

                                // 讓視窗啟動時居中
                                var displayArea = DisplayArea.GetFromWindowId(windowId, DisplayAreaFallback.Nearest);
                                if (displayArea is not null) {
                                    var centeredPosition = new PointInt32(
                                        (displayArea.WorkArea.Width - width) / 2,
                                        (displayArea.WorkArea.Height - height) / 2
                                    );
                                    appWindow.Move(centeredPosition);
                                }
                            }
                        )
                    )
                );
#else
                ;
#endif
            /* 啟用 WebView 支援 */
            builder.Services.AddMauiBlazorWebView();

            // 初始化 log4net。預設 log4net.config 於設定上會複製到 {outdir}\Configurations\log4net.config
            var logRepository = log4net.LogManager.GetRepository(Assembly.GetExecutingAssembly());
            var configFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configurations", "log4net.config");
            if (File.Exists(configFile)) {
                log4net.Config.XmlConfigurator.Configure(logRepository, new FileInfo(configFile));
            }

            // 註冊服務，後續就可以從 Dispatcher 來呼叫、存取
            builder.Services.AddSingleton<BaZiService>();
            builder.Services.AddSingleton<TenGodAnalysisService>();
            builder.Services.AddSingleton<CompatibilityService>();
            builder.Services.AddSingleton<FortuneService>();
            builder.Services.AddSingleton<SecurityService>();
            builder.Services.AddScoped<Store.BaZiEffects>();

            // 加入 Fluxor 服務
            builder.Services.AddFluxor(options => options.ScanAssemblies(Assembly.GetExecutingAssembly()));

#if DEBUG
            // 於 Debug 時，啟用 Web View 的開發工具，並在訊息輸出中，加入 Debug 串流
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
