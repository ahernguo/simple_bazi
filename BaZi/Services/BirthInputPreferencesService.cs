using System.Globalization;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Storage;

namespace BaZi.Services {

    /// <summary>可保存的出生資料選擇。</summary>
    public sealed record BirthInputSelection(
        int Gender,
        int Year,
        int Month,
        int Day,
        int Hour,
        int Minute
    ) {
        public const int UnknownHour = 25;
        public const int UnknownMinute = 61;

        public bool IsValid() {
            if (Gender is not 1 and not 2
                || Year < 1900
                || Year > DateTime.Now.Year
                || Month is < 1 or > 12
                || Day < 1
                || Day > DateTime.DaysInMonth(Year, Month)) {
                return false;
            }

            bool isHourValid = Hour is >= 0 and < 24 || Hour == UnknownHour;
            bool isMinuteValid = Minute is >= 0 and < 60 || Minute == UnknownMinute;
            return isHourValid && isMinuteValid;
        }
    }

    /// <summary>保存與還原排盤及合盤表單的最後一次有效輸入。</summary>
    public sealed class BirthInputPreferencesService {
        private const string HomeInputKey = "birth-input.home.v1";
        private const string CompatibilityInputKey = "birth-input.compatibility.v1";

        private readonly IPreferences _preferences;
        private readonly ILogger<BirthInputPreferencesService> _logger;

        public BirthInputPreferencesService(
            IPreferences preferences,
            ILogger<BirthInputPreferencesService> logger
        ) {
            _preferences = preferences;
            _logger = logger;
        }

        public BirthInputSelection? LoadHome() => Load(HomeInputKey);

        public BirthInputSelection? LoadCompatibility() => Load(CompatibilityInputKey);

        public void SaveHome(BirthInputSelection selection) => Save(HomeInputKey, selection);

        public void SaveCompatibility(BirthInputSelection selection) => Save(CompatibilityInputKey, selection);

        private BirthInputSelection? Load(string key) {
            try {
                string serializedSelection = _preferences.Get(key, string.Empty);
                if (string.IsNullOrWhiteSpace(serializedSelection)) {
                    return null;
                }

                var selection = Deserialize(serializedSelection);
                if (selection?.IsValid() == true) {
                    return selection;
                }

                _logger.LogWarning("忽略無效的出生資料偏好設定。Key: {PreferenceKey}", key);
            } catch (Exception ex) {
                _logger.LogWarning(ex, "無法讀取出生資料偏好設定。Key: {PreferenceKey}", key);
            }

            return null;
        }

        private void Save(string key, BirthInputSelection selection) {
            ArgumentNullException.ThrowIfNull(selection);
            if (!selection.IsValid()) {
                throw new ArgumentException("出生資料偏好設定的內容無效。", nameof(selection));
            }

            try {
                string serializedSelection = Serialize(selection);
                _preferences.Set(key, serializedSelection);
            } catch (Exception ex) {
                // 偏好設定是便利功能，寫入失敗不應阻止主要排盤流程。
                _logger.LogWarning(ex, "無法儲存出生資料偏好設定。Key: {PreferenceKey}", key);
            }
        }

        private static string Serialize(BirthInputSelection selection) {
            return string.Create(
                CultureInfo.InvariantCulture,
                $"{selection.Gender}|{selection.Year}|{selection.Month}|{selection.Day}|{selection.Hour}|{selection.Minute}"
            );
        }

        private static BirthInputSelection? Deserialize(string serializedSelection) {
            string[] fields = serializedSelection.Split('|');
            if (fields.Length != 6) {
                return null;
            }

            var values = new int[fields.Length];
            for (var index = 0; index < fields.Length; index++) {
                if (!int.TryParse(
                    fields[index],
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out values[index]
                )) {
                    return null;
                }
            }

            return new BirthInputSelection(
                values[0],
                values[1],
                values[2],
                values[3],
                values[4],
                values[5]
            );
        }
    }
}
