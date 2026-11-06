using System;
using System.Drawing;
using System.Windows.Forms;

using ModernNotifyIcon.Theme;
using System.Collections.Generic;
using System.Reflection;

namespace PowerDimmer
{
    public class NotifyIconController
    {
        internal Action? ExitClicked;
        public NotifyIcon NotifyIcon;

        public NotifyIconController(ISettings settings)
        {
            NotifyIcon = NotifyIconBuilder
                .Create()
                .Configure(builder => builder
                    .AddToggle(option => option
                        .SetText("Затемнение активно")
                        .SetChecked(settings.ActiveOnLaunch)
                        .ConfigureItem(item =>
                        {
                            item.ShortcutKeyDisplayString = "Win+Ctrl+Alt+D";

                            settings.PropertyChanged += (_, e) =>
                            {
                                if (e.PropertyName == nameof(settings.DimmingEnabled))
                                {
                                    item.Checked = settings.DimmingEnabled;
                                }
                            };
                        })
                        .AddHandler((b) => settings.DimmingEnabled = b))
                    .AddToggle(option => option
                        .SetText("Затемнять панель задач")
                        .SetChecked(settings.DimTaskbar)
                        .AddHandler((b) => settings.DimTaskbar = b))
                    .AddToggle(option => option
                        .SetText("Активно при запуске")
                        .SetChecked(settings.ActiveOnLaunch)
                        .AddHandler((b) => settings.ActiveOnLaunch = b))
                    .AddSeparator()
                    .AddButton(option => option
                        .SetText("Режим: выкл (0)")
                        .AddHandler(() => { settings.Brightness = 0; settings.DimmingEnabled = false; }))
                    .AddButton(option => option
                        .SetText("Режим: слабое (30)")
                        .AddHandler(() => { settings.Brightness = 30; settings.DimmingEnabled = true; }))
                    .AddButton(option => option
                        .SetText("Режим: среднее (60)")
                        .AddHandler(() => { settings.Brightness = 60; settings.DimmingEnabled = true; }))
                    .AddButton(option => option
                        .SetText("Режим: глухое (100)")
                        .AddHandler(() => { settings.Brightness = 100; settings.DimmingEnabled = true; }))
                    .AddSeparator()
                    .AddItem(new TrackBarMenuItem(settings))
                    .AddSeparator()
                    .AddButton(option => option
                        .SetText("В&ыход")
                        .AddHandler(() => ExitClicked?.Invoke())))
                .Build(Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location)!);

            NotifyIcon.Text = "Затемнение окон — PowerDimmer";
            NotifyIcon.Visible = true;
        }
    }

    // https://stackoverflow.com/a/24825487
    public class TrackBarWithoutFocus : TrackBar
    {
        private const int WM_SETFOCUS = 0x0007;

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_SETFOCUS)
            {
                return;
            }

            base.WndProc(ref m);
        }
    }

    // https://stackoverflow.com/questions/4339143/adding-a-trackbar-control-to-a-contextmenu
    public class TrackBarMenuItem : ToolStripControlHost
    {
        private TrackBar trackBar;

        public TrackBarMenuItem(ISettings settings) : base(new ContainerControl())
        {
            BackColor = ThemeDictionary.ChromeMidium;

            var brightnessLabel = new Label()
            {
                Parent = Control,
                Text = "Режим затемнения (0–100)",
                TextAlign = ContentAlignment.MiddleCenter,
            };

            trackBar = new TrackBarWithoutFocus
            {
                Parent = Control,
                Top = 22,
                Minimum = 0,
                Maximum = 100,
                TickFrequency = 1,
                SmallChange = 5,
                LargeChange = 20,
                TickStyle = TickStyle.None,
                Value = settings.Brightness,
            };
            // Hack to restore hover-highlights after interacting
            // with trackbar
            trackBar.Click += (_, _) => Parent.Focus();

            var valueBox = new TextBox()
            {
                Parent = trackBar,
                Top = 28,
                Left = 1,
                Enabled = false,
                BackColor = ThemeDictionary.ChromeMidium,
                TextAlign = HorizontalAlignment.Center,
                BorderStyle = BorderStyle.None,
                Text = settings.Brightness.ToString()
            };

            var saveTimer = new System.Windows.Forms.Timer { Interval = 400 };
            saveTimer.Tick += (_, _) => { saveTimer.Stop(); TrySave(); };

            trackBar.ValueChanged += (o, s) =>
                {
                    valueBox.Text = trackBar.Value.ToString();
                    saveTimer.Stop();
                    saveTimer.Start();
                };

            void TrySave()
            {
                try { settings.Brightness = trackBar.Value; }
                catch (System.IO.IOException) { /* файл настроек временно занят — пропускаем, не роняем приложение */ }
                catch (Exception) { }
            }
        }
    }
}