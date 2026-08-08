> [!WARNING]
> 此專案為個人測試用途，內容仍有疏漏、解讀不全，仍須由命理師解讀

# 專案說明

以《徐玉蘭的人生八字應用課》來整理內容，搭配 MAUI 提供 Windows, iOS, Android 跨平台之應用程式

部分內容是搭配 Google Gemini 3.1 Pro 所產生的描述且含有個人解讀，可能會與上課影片稍有不同

判讀上以窮舉的方式來列出所有可能性，預計未來會加入 AI 自動判讀功能

# 專案架構

此方案採用 .NET 10 的 .NET MAUI Blazor Hybrid，並以 Fluxor 管理前端狀態。主要目錄如下：

```text
BaZi/
├─ BaZi.slnx                  # 方案檔
├─ README.md
├─ BaZi/                      # MAUI Blazor Hybrid 應用程式
│  ├─ BaZi.csproj             # 目標平台、套件與資源設定
│  ├─ MauiProgram.cs          # 共用啟動設定與相依性注入
│  ├─ App.xaml(.cs)           # 建立應用程式視窗
│  ├─ MainPage.xaml(.cs)      # 承載 BlazorWebView
│  ├─ Components/
│  │  ├─ Pages/               # 可路由頁面
│  │  ├─ Layout/              # 共用版面與導覽列
│  │  ├─ Compatibility/       # 合盤功能元件
│  │  └─ Routes.razor         # Blazor 路由與 Fluxor 初始化
│  ├─ Models/                 # 八字、流年、流月與合盤等領域模型
│  ├─ Services/               # 排盤、十神、流年與合盤等應用邏輯
│  ├─ Store/                  # Fluxor State、Action、Effect、Reducer
│  ├─ Platforms/              # Android、iOS、Mac Catalyst、Windows 平台入口與設定
│  ├─ Resources/              # 圖示、啟動畫面、字型、圖片與原始資源
│  ├─ wwwroot/                # Blazor 靜態網頁、CSS 與 JavaScript
│  ├─ Configurations/         # log4net 設定
│  ├─ Includes/               # 隨程式提供的 lunar.dll
│  └─ package.cmd             # Windows x64 自包含封裝腳本
└─ BaZi.Tests/                # xUnit 單元測試
```

主要分層與責任：

- `Components/Pages/` 負責畫面與使用者互動，根路由 `/` 由 `Home.razor` 提供。
- `Services/` 封裝排盤、十神分析、流年流月及合盤計算。
- `Store/` 使用 Fluxor 實作單向資料流；頁面派送 Action，Effect 呼叫服務，Reducer 更新 State。
- `Models/` 保存領域模型與列舉，避免計算規則散落於 UI。
- `Platforms/` 僅放置各平台的原生啟動程式與平台設定，共用邏輯集中於 `MauiProgram.cs`。

# 程式進入點

各平台先進入 `Platforms/<平台>/` 下的原生入口，再統一呼叫 `MauiProgram.CreateMauiApp()`：

| 平台 | 原生入口 | 說明 |
| --- | --- | --- |
| Windows | `Platforms/Windows/App.xaml.cs` | `MauiWinUIApplication` 建立 MAUI 應用程式。 |
| Android | `Platforms/Android/MainApplication.cs` | `MauiApplication` 建立 MAUI 應用程式；`MainActivity.cs` 是啟動 Activity。 |
| iOS | `Platforms/iOS/Program.cs`、`AppDelegate.cs` | `UIApplication.Main` 啟動後由 AppDelegate 建立 MAUI 應用程式。 |
| Mac Catalyst | `Platforms/MacCatalyst/Program.cs`、`AppDelegate.cs` | 與 iOS 類似，透過 AppDelegate 進入共用啟動流程。 |

共用啟動流程如下：

1. `MauiProgram.CreateMauiApp()` 設定 `zh-TW` 文化特性、MAUI Blazor WebView、log4net、應用服務與 Fluxor；Windows 另設定初始視窗大小與位置。
2. `App.CreateWindow()` 建立以 `MainPage` 為內容的 MAUI 視窗。
3. `MainPage.xaml` 建立 `BlazorWebView`，載入 `wwwroot/index.html`，並將 `Routes.razor` 掛載至 `#app`。
4. `Routes.razor` 初始化 Fluxor Store 與 Blazor Router，再依 `@page` 路由顯示對應頁面；預設進入 `Home.razor`。

# 執行方式

## 開發環境

- .NET 10 SDK
- 目標平台對應的 .NET MAUI 工作負載
- Windows 開發建議使用支援 .NET 10 與 MAUI 的 Visual Studio，並安裝「.NET 多平台應用程式 UI 開發」工作負載
- Windows 最低支援版本為 Windows 10 1809（10.0.17763.0）

以下命令均以本 README 所在的方案根目錄執行；若目前位於包含 `筆記/`、`講義/` 與 `BaZi/` 的 workspace 根目錄，請先執行 `Set-Location BaZi`。

可先確認開發環境：

```powershell
dotnet --version
dotnet workload list
dotnet workload restore BaZi/BaZi.csproj
```

## Windows 命令列執行

```powershell
# 還原相依套件
dotnet restore BaZi.slnx

# 建置 Windows 版本
dotnet build BaZi.slnx -f net10.0-windows10.0.19041.0

# 執行應用程式
dotnet run --project BaZi/BaZi.csproj -f net10.0-windows10.0.19041.0
```

## Visual Studio 執行

1. 開啟 `BaZi.slnx`。
2. 將 `BaZi` 設為啟始專案。
3. 選擇 `Windows Machine` 目標。
4. 按 `F5` 偵錯執行，或按 `Ctrl+F5` 執行但不啟動偵錯工具。

專案也宣告 `net10.0-android`、`net10.0-ios` 與 `net10.0-maccatalyst`。Android 需準備 Android SDK 與模擬器或實機；iOS 與 Mac Catalyst 需在具備 Xcode 的 macOS 環境建置與執行。

## 測試與格式檢查

```powershell
dotnet test BaZi.Tests/BaZi.Tests.csproj
dotnet format BaZi.slnx --verify-no-changes
```

## Windows 發行封裝

`package.cmd` 會產生 Windows x64 自包含的 7-Zip 壓縮檔。執行前請先確認 `7z` 位於 `PATH`，並依發行版本更新腳本內的 `APP_VERSION`：

```powershell
Set-Location BaZi
.\package.cmd
```

# 專案參考

- [darkthread/lunar-csharp](https://github.com/darkthread/lunar-csharp/)
    - 詳細說明請參考 [純 .NET 農民曆程式庫 - lunar-csharp 之繁體中文化](https://blog.darkthread.net/blog/lunar-csharp-support-zh-tw/)
