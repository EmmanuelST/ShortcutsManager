using AudioSwitcher.AudioApi.CoreAudio;
using ShortcutsManager.Components.Overlay;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using WindowsInput.Events;


namespace ShortcutsManager
{
    public partial class MainWindow : Window
    {
        private const int HOTKEY_ID_1 = 9001;
        private const int HOTKEY_ID_2 = 9002;
        private const int HOTKEY_ID_3 = 9003;
        private const int WM_HOTKEY = 0x0312;

        private const uint MOD_ALT = 0x0001;
        private const uint MOD_CONTROL = 0x0002;

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);


        private HwndSource _source = null!;

        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            var helper = new WindowInteropHelper(this);
            _source = HwndSource.FromHwnd(helper.Handle);
            _source.AddHook(HwndHook);

            // Registramos los hotkeys
            RegisterHotKey(helper.Handle, HOTKEY_ID_1, MOD_ALT | MOD_CONTROL, (uint)KeyInterop.VirtualKeyFromKey(Key.D1));
            RegisterHotKey(helper.Handle, HOTKEY_ID_2, MOD_ALT | MOD_CONTROL, (uint)KeyInterop.VirtualKeyFromKey(Key.D2));
            RegisterHotKey(helper.Handle, HOTKEY_ID_3, MOD_ALT | MOD_CONTROL, (uint)KeyInterop.VirtualKeyFromKey(Key.D3));
        }

        protected override void OnClosed(EventArgs e)
        {
            var handle = new WindowInteropHelper(this).Handle;
            UnregisterHotKey(handle, HOTKEY_ID_1);
            UnregisterHotKey(handle, HOTKEY_ID_2);
            UnregisterHotKey(handle, HOTKEY_ID_3);

            _source.RemoveHook(HwndHook);
            base.OnClosed(e);
        }
        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_HOTKEY)
            {
                int id = wParam.ToInt32();
                string activeApp = GetActiveProcessName();
                switch (id)
                {
                    case HOTKEY_ID_1:
                        ExecuteShortcut1(activeApp);
                        break;
                    case HOTKEY_ID_2:
                        ExecuteShortcut2(activeApp);
                        break;
                    case HOTKEY_ID_3:
                        ExecuteShortcut3(activeApp);
                        break;
                    default:
                        return IntPtr.Zero; // Not a registered hotkey
                }

                handled = true;
            }

            return IntPtr.Zero;
        }

        private async void ExecuteShortcut1(string appName)
        {
            bool? micMuted = null;
            if (appName.Contains("Discord", StringComparison.OrdinalIgnoreCase))
            {
                await SimulateKeyPress(KeyCode.Control, KeyCode.Alt, KeyCode.S);
            }
            else
            {¡
                micMuted = await ToggleSystemMic();
            }
            string micMutedText = micMuted.HasValue ? (micMuted.Value ? "ON" : "OFF") : "Desconocido";
            var overlayMessage = $"{micMutedText} Shortcut #1 En aplicación: {appName}";
            var overlay = new OverlayWindow(overlayMessage);
            overlay.ShowAndAutoClose();
        }

        private void ExecuteShortcut2(string appName)
        {
            var overlayMessage = $"Shortcut #2 En aplicación: {appName}";
            var overlay = new OverlayWindow(overlayMessage);
            overlay.ShowAndAutoClose();
        }

        private void ExecuteShortcut3(string appName)
        {
            var overlayMessage = $"Shortcut #3 En aplicación: {appName}";
            var overlay = new OverlayWindow(overlayMessage);
            overlay.ShowAndAutoClose();
        }

        private async Task SimulateKeyPress(params KeyCode[] Keys)
        {
            await WindowsInput.Simulate.Events()
                .ClickChord(Keys)
                .Invoke();
        }

        private string GetActiveWindowTitle()
        {
            IntPtr hWnd = GetForegroundWindow();
            if (hWnd == IntPtr.Zero)
                return "";

            var buffer = new StringBuilder(256);
            GetWindowText(hWnd, buffer, buffer.Capacity);
            return buffer.ToString();
        }

        private async Task<bool?> ToggleSystemMic()
        {
            var controller = new CoreAudioController();
            var defaultMic = await controller.GetDefaultDeviceAsync(AudioSwitcher.AudioApi.DeviceType.Capture, AudioSwitcher.AudioApi.Role.Communications);

            if (defaultMic != null)
            {
                bool isMuted = defaultMic.IsMuted;
                await defaultMic.MuteAsync(!isMuted);
                return !isMuted; // Retorna el nuevo estado del micrófono
            }
            return null; // Si no se pudo obtener el micrófono por alguna razón
        }
        private string GetActiveProcessName()
        {
            IntPtr hWnd = GetForegroundWindow();
            if (hWnd == IntPtr.Zero)
                return string.Empty;

            uint processId;
            _ = GetWindowThreadProcessId(hWnd, out processId);

            try
            {
                var process = Process.GetProcessById((int)processId);
                return process.ProcessName; // Ej: "Discord", "chrome", "explorer"
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}