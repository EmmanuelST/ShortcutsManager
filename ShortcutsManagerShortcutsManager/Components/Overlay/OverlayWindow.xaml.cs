using System.Windows;
using System.Windows.Threading;

namespace ShortcutsManager.Components.Overlay;

public partial class OverlayWindow : Window
{
    private readonly DispatcherTimer _timer;

    public OverlayWindow(string message)
    {
        InitializeComponent();
        MessageText.Text = message;

        Loaded += (s, e) =>
        {
            const int bottomMargin = 25;

            // Centrado horizontal y cerca del borde inferior
            Left = (SystemParameters.WorkArea.Width - Width) / 2 + SystemParameters.WorkArea.Left;
            Top = SystemParameters.WorkArea.Bottom - Height - bottomMargin;
        };


        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(2)
        };
        _timer.Tick += (s, e) =>
        {
            _timer.Stop();
            Close();
        };
    }

    public void ShowAndAutoClose()
    {
        Show();
        _timer.Start();
    }

    public static void ShowOverlay(string message)
    {
        var overlay = new OverlayWindow(message);
        overlay.ShowAndAutoClose();
    }
}