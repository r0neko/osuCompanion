using System;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace osuCompanion
{
    class MSN
    {
        private const string lpClassName = "MsnMsgrUIManager";
        private WNDCLASS lpWndClass;
        private IntPtr m_hwnd;
        private WndProc WndProcc;
        private OsuStatus lastStatus;
        public event EventHandler<OsuStatus> MessageReceived;

        [DllImport("user32.dll")]
        private static extern ushort RegisterClassW([In] ref WNDCLASS lpWndClass);

        [DllImport("user32.dll")]
        private static extern IntPtr CreateWindowExW(uint dwExStyle, [MarshalAs(UnmanagedType.LPWStr)] string lpClassName, [MarshalAs(UnmanagedType.LPWStr)] string lpWindowName, uint dwStyle, int x, int y, int nWidth, int nHeight, IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);

        [DllImport("user32.dll")]
        private static extern IntPtr DefWindowProcW(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool DestroyWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern bool TranslateMessage([In] ref MSG lpMsg);

        [DllImport("user32.dll")]
        static extern IntPtr DispatchMessage([In] ref MSG lpmsg);

        [DllImport("user32.dll")]
        static extern sbyte GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin,
           uint wMsgFilterMax);
        private delegate IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        private struct WNDCLASS
        {
            public uint style;
            public WndProc lpfnWndProc;
            public int cbClsExtra;
            public int cbWndExtra;
            public IntPtr hInstance;
            public IntPtr hIcon;
            public IntPtr hCursor;
            public IntPtr hbrBackground;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string lpszMenuName;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string lpszClassName;
        }

        private struct COPYDATASTRUCT
        {
            public IntPtr dwData;
            public int cbData;
            public IntPtr lpData;
        }

        public enum Status
        {
            Null,
            Listening,
            Editing,
            Playing,
            Watching
        };

        public class OsuStatus : EventArgs
        {
            public Status status { get; set; }
            public String artist { get; set; }
            public String title { get; set; }
            public String difficulty { get; set; }

        }

        private IntPtr CustomWndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            if (msg == 0x4a)
            {
                COPYDATASTRUCT copydatastruct =
                    (COPYDATASTRUCT)Marshal.PtrToStructure(lParam, typeof(COPYDATASTRUCT));

                var ptr = copydatastruct.lpData;
                if (ptr != IntPtr.Zero)
                {
                    string str = Marshal.PtrToStringUni(ptr, copydatastruct.cbData / 2);
                    string[] separator = new string[] { @"\0" };
                    string[] sourceArray = str.Split(separator, StringSplitOptions.None);
                    if (sourceArray.Length > 8)
                    {
                        for (int i = 0; i < sourceArray.Length; i++)
                        {
                            String status = sourceArray[3].Split(new[] { ' ' }, 2)[0];
                            Status st = status == "Listening" ? Status.Listening
                                        : status == "Playing" ? Status.Playing
                                            : status == "Watching" ? Status.Watching
                                                : status == "Editing" ? Status.Editing
                                                    : Status.Null;

                            OsuStatus stat = new OsuStatus();
                            stat.status = st;
                            stat.artist = sourceArray[5];
                            stat.title = sourceArray[4];
                            stat.difficulty = sourceArray[7];

                            if (this.lastStatus == null || this.lastStatus.status != stat.status || this.lastStatus.artist != stat.artist || this.lastStatus.title != stat.title)
                            {
                                if (this.lastStatus == null) this.lastStatus = new OsuStatus();
                                this.lastStatus.status = stat.status;
                                this.lastStatus.artist = stat.artist;
                                this.lastStatus.title = stat.title;
                                this.lastStatus.difficulty = stat.difficulty;
                                EventHandler<OsuStatus> handler = MessageReceived;
                                if (handler != null)
                                {
                                    handler(this, stat);
                                }
                            }
                        }
                    }
                }
            }
            return DefWindowProcW(hWnd, msg, wParam, lParam);
        }

        public void init()
        {
            WndProcc = CustomWndProc;
            lpWndClass = new WNDCLASS
            {
                lpszClassName = lpClassName,
                lpfnWndProc = WndProcc
            };
            ushort num = RegisterClassW(ref lpWndClass);
            int num2 = Marshal.GetLastWin32Error();
            if ((num == 0) && (num2 != 0x582))
            {
                throw new Exception("Could not register window class");
            }
            m_hwnd = CreateWindowExW(0, lpClassName, string.Empty, 0, 0, 0, 0, 0, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
        }

        public void Update()
        {
            MSG msg;
            if (GetMessage(out msg, IntPtr.Zero, 0, 0) != 0)
            {
                TranslateMessage(ref msg);
                DispatchMessage(ref msg);
            }
        }

    }
}
