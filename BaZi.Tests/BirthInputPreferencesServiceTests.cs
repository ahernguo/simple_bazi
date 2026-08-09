using BaZi.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Maui.Storage;
using Xunit;

namespace BaZi.Tests {

    public sealed class BirthInputPreferencesServiceTests {
        [Fact]
        public void SaveHome_ValidSelection_CanBeLoaded() {
            var service = CreateService();
            var expected = new BirthInputSelection(1, 1988, 8, 8, 14, 30);

            service.SaveHome(expected);

            Assert.Equal(expected, service.LoadHome());
        }

        [Fact]
        public void SaveCompatibility_UnknownBirthTime_PreservesSentinels() {
            var service = CreateService();
            var expected = new BirthInputSelection(
                2,
                1990,
                1,
                1,
                BirthInputSelection.UnknownHour,
                BirthInputSelection.UnknownMinute
            );

            service.SaveCompatibility(expected);

            Assert.Equal(expected, service.LoadCompatibility());
            Assert.Null(service.LoadHome());
        }

        [Fact]
        public void SaveHome_InvalidSelection_Throws() {
            var service = CreateService();
            var invalid = new BirthInputSelection(0, 1990, 1, 1, 12, 0);

            Assert.Throws<ArgumentException>(() => service.SaveHome(invalid));
        }

        private static BirthInputPreferencesService CreateService() {
            return new BirthInputPreferencesService(
                new FakePreferences(),
                NullLogger<BirthInputPreferencesService>.Instance
            );
        }

        private sealed class FakePreferences : IPreferences {
            private readonly Dictionary<(string Key, string? SharedName), object> _values = [];

            public bool ContainsKey(string key, string? sharedName = null) {
                return _values.ContainsKey((key, sharedName));
            }

            public void Remove(string key, string? sharedName = null) {
                _values.Remove((key, sharedName));
            }

            public void Clear(string? sharedName = null) {
                foreach (var key in _values.Keys.Where(key => key.SharedName == sharedName).ToArray()) {
                    _values.Remove(key);
                }
            }

            public void Set<T>(string key, T value, string? sharedName = null) {
                _values[(key, sharedName)] = value!;
            }

            public T Get<T>(string key, T defaultValue, string? sharedName = null) {
                return _values.TryGetValue((key, sharedName), out object? value) && value is T typedValue
                    ? typedValue
                    : defaultValue;
            }
        }
    }
}
