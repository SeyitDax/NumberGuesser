namespace NumberGuesser.Tracking;

using System.Runtime.InteropServices;
using System.Diagnostics;

using NumberGuesser.Core;
using NumberGuesser.Core.Events;
using NumberGuesser.Models;

public class ResistanceMonitor : IDisposable
{
	private bool _started;
	private bool _disposed;

	private int _ctrlCAttempts;
	private bool _monitoring;

	private int _lostFocusCount;
	private bool _lostFocus;

	private CancellationTokenSource _cts;
	private CancellationToken _token;
	private Task _task;

	private IntPtr _targetWindow;

	[DllImport("user32.dll")]
	private static extern IntPtr GetForegroundWindow();

	[DllImport("user32.dll")]
	private static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);

	[DllImport("user32.dll")]
	private static extern IntPtr GetWindowLongPtrW(IntPtr hWnd, int nIndex);

	private const uint GW_OWNER = 4;
	private const int GWL_EXSTYLE = -20;
	private const long WS_EX_TOOLWINDOW = 0x00000080;

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern IntPtr CreateToolhelp32Snapshot(uint dwFlags, uint th32ProcessID);

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern bool CloseHandle(IntPtr hObject);

	[DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
	private static extern bool Process32FirstW(IntPtr hSnapshot, ref PROCESSENTRY32 lppe);

	[DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
	private static extern bool Process32NextW(IntPtr hSnapshot, ref PROCESSENTRY32 lppe);

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	private struct PROCESSENTRY32
	{
		public uint dwSize;
		public uint cntUsage;
		public uint th32ProcessID;
		public IntPtr th32DefaultHeapID;
		public uint th32ModuleID;
		public uint cntThreads;
		public uint th32ParentProcessID;
		public int pcPriClassBase;
		public uint dwFlags;
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
		public string szExeFile;
	}

	private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

	[DllImport("user32.dll")]
	private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

	[DllImport("user32.dll")]
	static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

	[DllImport("user32.dll")]
	private static extern bool IsWindowVisible(IntPtr hWnd);

	public ResistanceMonitor(EventBus eventBus)
	{
		_cts = new();
		_task = Task.CompletedTask;
		eventBus.Subscribe<CtrlCPressedEvent>( e => _ctrlCAttempts++ );
	}

	public void Start()
	{
		if (!_started && !_disposed)
		{
			_started = true;
			_monitoring = true;

			_token = _cts.Token;

			_targetWindow = GetVisibleTerminalWindow();

			if (_targetWindow == IntPtr.Zero)
				Debug.WriteLine($"[ERROR]: Terminal window couldn't be found at handle: {_targetWindow}");
			else
			{
				_task = Task.Run(async () =>
				{
					try
					{
						while (_monitoring)
						{
							bool isFocused = GetForegroundWindow() == _targetWindow;

							if (!isFocused && !_lostFocus)
							{
								_lostFocusCount++;
								_lostFocus = true;
							}
							else if (isFocused && _lostFocus)
							{
								_lostFocus = false;
							}

							await Task.Delay(500, _token);
						}
					}
					catch (OperationCanceledException)
					{
					}
				});
			}
		}
	}

	// GetConsoleWindow()/the shell process's own window is ConPTY's hidden pseudo-console
	// window, not the terminal emulator window the user actually sees and focuses (e.g.
	// Windows Terminal hosting pwsh). Walking up the process ancestry to the emulator's PID
	// is what finds the real one.
	private IntPtr GetVisibleTerminalWindow()
	{
		var currentProcessID = Environment.ProcessId;
		var processTree = new Dictionary<uint, uint>();

		const uint TH32CS_SNAPPROCESS = 0x00000002;
		IntPtr snapshot = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0);

		var entry = new PROCESSENTRY32();
		entry.dwSize = (uint)Marshal.SizeOf<PROCESSENTRY32>();

		try
		{
			if (!Process32FirstW(snapshot, ref entry))
			{
				return IntPtr.Zero;
			}
			do
			{
				processTree.Add(entry.th32ProcessID, entry.th32ParentProcessID);
			}
			while (Process32NextW(snapshot, ref entry));
		}
		finally
		{
			CloseHandle(snapshot);
		}

		const int maxAncestorDepth = 10;
		for (int i = 0; i < maxAncestorDepth; i++)
		{
			if (processTree.TryGetValue((uint)currentProcessID, out uint parent))
			{
				var hWnd = FindVisibleWindowForProcess(parent);
				if (hWnd != IntPtr.Zero) return hWnd;
			}
			if (parent != 0)
				currentProcessID = (int)parent;
			else
				break;
		}

		return IntPtr.Zero;
	}

	private IntPtr FindVisibleWindowForProcess(uint pid)
	{
		var found = IntPtr.Zero;

		EnumWindows((hWnd, lParam) => {
			GetWindowThreadProcessId(hWnd, out uint windowId);

			if (pid != windowId) return true;

			// IsWindowVisible alone isn't enough: ConPTY's pseudo-console window (class
			// PseudoConsoleWindow) is visible and owned by the shell's own PID, but it's an
			// owned, WS_EX_TOOLWINDOW-styled helper window rather than a real top-level app
			// window. Requiring no owner and no tool-window style is the same heuristic
			// Explorer/Alt-Tab use to filter out windows like it.
			IntPtr exStyle = GetWindowLongPtrW(hWnd, GWL_EXSTYLE);

			bool hasOwner = GetWindow(hWnd, GW_OWNER) != IntPtr.Zero;
			bool hasToolWindowStyle = ((long)exStyle & WS_EX_TOOLWINDOW) != 0;

			bool isRealWindow = IsWindowVisible(hWnd) && !hasOwner && !hasToolWindowStyle;
			if (isRealWindow)
			{
				found = hWnd;
				return false;
			}
			return true;
		}, IntPtr.Zero);

		return found;
	}

	public ResistanceSnapshot GetResistanceSnapshot() => new ResistanceSnapshot(_ctrlCAttempts, _lostFocusCount);

	public void Dispose()
	{
		if (!_disposed)
		{
			_disposed = true;

			_monitoring = false;
			_cts.Cancel();
			_task.Wait();
			_cts.Dispose();
		}
	}
}
