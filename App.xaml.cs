using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using IO = System.IO;
using UserSettings = Delfinovin.Properties.Settings;

namespace Delfinovin
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private Mutex _appMutex;
        private System.Windows.Forms.NotifyIcon _notifyIcon;
        private GamecubeAdapter _gamecubeAdapter;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            ShutdownMode = ShutdownMode.OnExplicitShutdown;

            createGamecubeAdapter();

            CreateMainWindow();
            if (UserSettings.Default.MinimizeOnStartup == false)
            {
                ShowMainWindow();
            }
                
            CreateNotifyIcon();
            UpdateRunningOldExecutable();
            IsInstanceRunning();
            ApplyThemes();
            _gamecubeAdapter.Start();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
            _gamecubeAdapter.Stop();

            base.OnExit(e);
        }

        private void CreateMainWindow()
        {
            Current.MainWindow = new MainWindow();
        }

        public GamecubeAdapter GetCamecubeAdapter()
        {
            return _gamecubeAdapter;
        }

        private void createGamecubeAdapter()
        {
            _gamecubeAdapter = new GamecubeAdapter();
        }

        private void ShowMainWindow()
        {
            Current.MainWindow.Show();
            Current.MainWindow.Activate();
            Current.MainWindow.Focus();
        }

        private void CloseMainWindow()
        {
            Current.MainWindow.Hide();
        }

        private void CreateNotifyIcon()
        {
            // Create a new NotifyIcon
            _notifyIcon = new System.Windows.Forms.NotifyIcon();

            // Set its hover text to the main window title
            _notifyIcon.Text = Strings.HeaderName;

            // Get the application icon stream
            Stream iconStream = GetResourceStream(new Uri("/Delfinovin;component/Resources/Icons/app.ico", UriKind.Relative)).Stream;

            // Set the notify icon to the application icon
            _notifyIcon.Icon = new System.Drawing.Icon(iconStream);

            // Create a new menu strip
            System.Windows.Forms.ContextMenuStrip notifyIconStrip = new();

            // Add Open/Close options and subscribe to the events
            notifyIconStrip.Items.Add(Strings.Open, null, OnNotifyIconOpenClick);
            notifyIconStrip.Items.Add(Strings.Quit, null, OnNotifyIconQuitClick);

            // Apply the menu strip to the notify icon
            _notifyIcon.ContextMenuStrip = notifyIconStrip;

            _notifyIcon.MouseClick += OnNotifyIconMouseClick;

            // Show the tray icon
            _notifyIcon.Visible = true;
        }

        private void OnNotifyIconMouseClick(object? sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button != System.Windows.Forms.MouseButtons.Left)
                return;

            if (Current.MainWindow == null)
            {
                ShowMainWindow();
            } else
            {
                CloseMainWindow();
            }
        }

        private void OnNotifyIconOpenClick(object? sender, EventArgs e)
        {
            ShowMainWindow();
        }

        private void OnNotifyIconQuitClick(object? sender, EventArgs e)
        {
            Current.Shutdown();
        }

        private void UpdateRunningOldExecutable()
        {
            var currentExecutablePath = Process.GetCurrentProcess().MainModule.FileName;
            string oldName = IO.Path.GetFileNameWithoutExtension(currentExecutablePath);

            if (oldName == "DelfinovinUI")
            {
                IO.File.Move(currentExecutablePath, "Delfinovin.exe");
            }
        }

        private void IsInstanceRunning()
        {
            // Create a new mutex. Check to see if there is a mutex already in play.
            // If so, shutdown this instance so not more than one is running.
            _appMutex = new Mutex(true, "Delfinovin", out bool aIsNewInstance);
            if (!aIsNewInstance)
            {
                Application.Current.Shutdown();
            }
        }

        private void ApplyThemes()
        {
            // Get the application themes and controller colors
            // from our user settings
            string applicationTheme = UserSettings.Default.ApplicationTheme;
            string controllerColor = UserSettings.Default.ControllerColor;

            if (!String.IsNullOrEmpty(controllerColor))
            {
                // Parse our controller color to an enum value
                if (Enum.TryParse(controllerColor, out ControllerColor color))
                {
                    // Case the controllerColor into an uint
                    uint hexColorCode = (uint)color;

                    // Get the color value from the hex code
                    Color convertedColor = ControlExtensions.GetColorFromHex(hexColorCode);

                    // Set the resource "ControllerColor" to with the new color 
                    Application.Current.Resources["ControllerColor"] = new SolidColorBrush(convertedColor);
                }
            }

            if (!String.IsNullOrEmpty(applicationTheme))
            {
                // Update the application-wide theme with the selected one.
                Application.Current.Resources.MergedDictionaries[0] = new ResourceDictionary()
                {
                    Source = new Uri($"/Delfinovin;component/Resources/Themes/{applicationTheme}.xaml", UriKind.Relative)
                };
            }
        }
    }
}
