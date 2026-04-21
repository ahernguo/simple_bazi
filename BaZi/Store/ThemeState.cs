using Fluxor;

namespace BaZi.Store {
    [FeatureState]
    public class ThemeState {
        public bool IsDarkMode { get; }
        public bool IsSidebarVisible { get; }

        public ThemeState() {
            IsDarkMode = true; // 預設暗黑模式
            IsSidebarVisible = true;
        }

        public ThemeState(bool isDarkMode, bool isSidebarVisible) {
            IsDarkMode = isDarkMode;
            IsSidebarVisible = isSidebarVisible;
        }
    }

    public class ToggleThemeAction { }
    public class ToggleSidebarAction { }

    public static class ThemeReducers {
        [ReducerMethod]
        public static ThemeState OnToggleTheme(ThemeState state, ToggleThemeAction action) =>
            new ThemeState(!state.IsDarkMode, state.IsSidebarVisible);

        [ReducerMethod]
        public static ThemeState OnToggleSidebar(ThemeState state, ToggleSidebarAction action) =>
            new ThemeState(state.IsDarkMode, !state.IsSidebarVisible);
    }
}
