using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace pixelpaw
{
    public sealed partial class MainWindow : Window
    {
        private const string SessionFilePath = @"C:\Users\siddh\Desktop\sessions.txt";
        private DispatcherTimer _timer;


        public MainWindow()
        {
            this.InitializeComponent();
            LoadSessionHistory(); // Load existing session history on startup
            var newSessionName = $"Session {GetNextSessionId()}";
            AddSessionEntry(DateTime.Now, newSessionName); // Add new session entry

            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
            _timer.Tick += Timer_Tick;

            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
            var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);
            appWindow.Resize(new Windows.Graphics.SizeInt32(500, 870));
        }

        

        private void Timer_Tick(object sender, object e)
        {
            double currentHeight = AccelerationRectangle.Height;
            double newHeight = currentHeight + 10; // Increase height by 10 units

            if (newHeight > 200) // Limit the height to the maximum height of the Border
            {
                newHeight = 200;
                _timer.Stop(); // Stop the timer when it reaches the maximum height
            }

            AccelerationRectangle.Height = newHeight; // Set the new height to the Rectangle
        }

        private void OnAccelerateButtonClick(object sender, RoutedEventArgs e)
        {
            if (_timer.IsEnabled)
            {
                _timer.Stop(); // Stop the timer if it's running
            }
            else
            {
                _timer.Start(); // Start the timer if it's not running
            }
        }

        private void LoadSessionHistory()
        {
            if (File.Exists(SessionFilePath))
            {
                var lines = File.ReadAllLines(SessionFilePath);
                foreach (var line in lines)
                {
                    var parts = line.Split('|');
                    if (parts.Length == 2 && DateTime.TryParse(parts[0], out var loginTime))
                    {
                        AddSessionEntry(loginTime, parts[1]);
                    }
                }
            }
        }

        private void SaveSessionEntry(DateTime loginTime, string sessionName)
        {
            var entry = $"{loginTime:yyyy-MM-dd HH:mm:ss}|{sessionName}";
            File.AppendAllLines(SessionFilePath, new[] { entry });
        }

        private int GetNextSessionId()
        {
            int nextId = 1; // Default to 1 if no sessions exist

            if (File.Exists(SessionFilePath))
            {
                var lines = File.ReadAllLines(SessionFilePath);
                if (lines.Length > 0)
                {
                    // Parse the last session entry to find the highest ID
                    var lastLine = lines.Last();
                    var lastParts = lastLine.Split('|');
                    if (lastParts.Length == 2 && int.TryParse(lastParts[1].Replace("Session ", ""), out int lastId))
                    {
                        nextId = lastId + 1;
                    }
                }
            }

            return nextId;
        }

        private void AddSessionEntry(DateTime loginTime, string sessionName)
        {
            var listViewItem = new ListViewItem
            {
                Content = new StackPanel
                {
                    Children =
                    {
                        new TextBlock { Text = $"Login time: {loginTime:yyyy-MM-dd | HH:mm:ss}", Margin = new Thickness(0, 0, 0, 5) },
                        new TextBlock { Text = sessionName, FontWeight = FontWeights.Bold }
                    }
                }
            };

            SessionHistoryListView.Items.Insert(0, listViewItem);
            SaveSessionEntry(loginTime, sessionName); // Save entry to file
        }
    }
}
