using System.Runtime.InteropServices;
using System.Text;
using System.Drawing;

namespace GenshinAuto.Windows;

record WindowRecord
{
	public IntPtr HWnd;
	public string Title;
	public Rect Rect;
	public WindowRecord() => (HWnd, Title, Rect) = (0, string.Empty, new Rect());
	public WindowRecord(IntPtr hWnd, string title, Rect rect) => 
		(HWnd, Title, Rect) = (hWnd, title, rect);
}

[StructLayout(LayoutKind.Sequential)]
struct Rect
{
	public int Left, Top, Right, Bottom;
	public override string ToString() => $"({Left}, {Top}), ({Right}, {Bottom})";
}

delegate bool EnumWindowsCallback(IntPtr hWnd, object? lParam);

static class WinCap
{
	public static float DpiFactor = 1.25f;
	public static string Title = "原神";
	public static Mutex mutex = new Mutex();
	[DllImport("user32.dll")]
	private static extern bool EnumWindows(EnumWindowsCallback callback, object? lParam);
	[DllImport("user32.dll")]
	private static extern IntPtr FindWindow(string? className, string windowName);
	[DllImport("user32.dll")]
	private static extern int GetWindowText(IntPtr hWnd, StringBuilder title, int maxCount);
	[DllImport("user32.dll")]
	private static extern bool GetWindowRect(IntPtr hWnd, out Rect rect);
	[DllImport("user32.dll")]
	private static extern bool SetForegroundWindow(IntPtr hWnd);

	private static Image GetWindowByTitle(string title)
	{
		IntPtr hWnd = 0;

		hWnd = FindWindow(null, title);
		SetForegroundWindow(hWnd);
		Rect r;
		GetWindowRect(hWnd, out r);

		Rectangle rect = new Rectangle((int)(r.Left * DpiFactor), (int)(r.Top * DpiFactor), (int)((r.Right - r.Left) * DpiFactor), (int)((r.Bottom - r.Top) * DpiFactor));
		Bitmap bitmap = new Bitmap(rect.Width, rect.Height);
		using(Graphics g = Graphics.FromImage(bitmap))
		{
			g.CopyFromScreen(rect.X, rect.Y, 0, 0, rect.Size);
			(rect.X, rect.Y) = (0, 0);
			g.DrawImage(bitmap, 0, 0, rect, GraphicsUnit.Pixel);
		}
		return bitmap;
	}

	private static DateTime tick;
	public static Task GetImage(CancellationToken token)
	{
		return Task.Run(() => {
			while(!token.IsCancellationRequested)	{
				if((DateTime.Now - tick).Seconds < 2)
					continue;
				tick = DateTime.Now;
				mutex.WaitOne();
				GetWindowByTitle(Title).Save("bin/screen.bmp");
				mutex.ReleaseMutex();
			}
		});
	}
}