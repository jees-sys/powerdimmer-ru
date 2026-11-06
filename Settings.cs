using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;

namespace PowerDimmer
{
    /// <summary>
    /// Своя простая реализация настроек: обычный JSON-файл в %APPDATA%\PowerDimmer.
    /// Запись — через временный файл и копирование (без File.Replace, который падал).
    /// </summary>
    public class Settings : ISettings, INotifyPropertyChanged
    {
        private static readonly string FilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "PowerDimmer", "settings.json");

        private bool _activeOnLaunch = true;
        private bool _dimmingEnabled;
        private bool _dimTaskbar = true;
        private int _brightness = 50;
        private bool _windowShadeEnabled = true;

        public bool ActiveOnLaunch
        {
            get => _activeOnLaunch;
            set { if (_activeOnLaunch != value) { _activeOnLaunch = value; OnChanged(); Save(); } }
        }

        public bool DimmingEnabled
        {
            get => _dimmingEnabled;
            set { if (_dimmingEnabled != value) { _dimmingEnabled = value; OnChanged(); Save(); } }
        }

        public bool DimTaskbar
        {
            get => _dimTaskbar;
            set { if (_dimTaskbar != value) { _dimTaskbar = value; OnChanged(); Save(); } }
        }

        public int Brightness
        {
            get => _brightness;
            set { if (_brightness != value) { _brightness = value; OnChanged(); Save(); } }
        }

        public bool WindowShadeEnabled
        {
            get => _windowShadeEnabled;
            set { if (_windowShadeEnabled != value) { _windowShadeEnabled = value; OnChanged(); Save(); } }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public void Load()
        {
            try
            {
                if (!File.Exists(FilePath)) return;
                var json = File.ReadAllText(FilePath);
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                if (root.TryGetProperty("activeOnLaunch", out var a)) _activeOnLaunch = a.GetBoolean();
                if (root.TryGetProperty("dimmingEnabled", out var d)) _dimmingEnabled = d.ValueKind == JsonValueKind.True;
                if (root.TryGetProperty("dimTaskbar", out var t)) _dimTaskbar = t.GetBoolean();
                if (root.TryGetProperty("brightness", out var b)) _brightness = b.GetInt32();
                if (root.TryGetProperty("windowShadeEnabled", out var w)) _windowShadeEnabled = w.GetBoolean();
            }
            catch { /* повреждённый файл — остаёмся на дефолтах */ }
        }

        public void Save()
        {
            var data = new
            {
                activeOnLaunch = _activeOnLaunch,
                dimmingEnabled = _dimmingEnabled,
                dimTaskbar = _dimTaskbar,
                brightness = _brightness,
                windowShadeEnabled = _windowShadeEnabled,
            };
            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            for (var attempt = 0; attempt < 3; attempt++)
            {
                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
                    var tmp = FilePath + ".tmp";
                    File.WriteAllText(tmp, json);
                    File.Copy(tmp, FilePath, true);
                    try { File.Delete(tmp); } catch { }
                    return;
                }
                catch (IOException)
                {
                    Thread.Sleep(150); // файл занят (антивирус и т.п.) — пробуем ещё
                }
                catch (Exception)
                {
                    return; // не роняем приложение из-за настроек
                }
            }
        }
    }
}
