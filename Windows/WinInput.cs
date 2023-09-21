using System.Runtime.InteropServices;

namespace GenshinAuto.Windows;

enum InputType : uint
{
	Mouse = 0, Key = 1, Hardware = 2
}

[Flags]
enum MouseEventFlag : uint
{
	Move = 0x0001,
	LeftDown = 0x0002,
	LeftUp = 0x0004,
	RightDown = 0x0008,
	RightUp = 0x0010,
	MiddleDown = 0x0020,
	MiddleUp = 0x0040,
	XDown = 0x0080,
	XUp = 0x0100,
	Wheel = 0x0800,
	HWeel = 0x1000,
	VirtualDesk = 0x4000,
	Absolute = 0x8000
}

[Flags]
enum KeyEventFlag : uint
{
	ExtendedKey = 0x1,
	KeyUp = 0x2,
	Unicode = 0x4,
	ScanCode = 0x8
}

enum ScanCode : ushort
{
	Esc = 0x01, K_1 = 0x02, K_2 = 0x03, K_3 = 0x04, K_4 = 0x05, K_5 = 0x06, K_6 = 0x07, K_7 = 0x08, K_8 = 0x09, K_9 = 0x0a, K_0 = 0x0b, K_sub = 0x0c, K_eq = 0x0d, Backspace = 0x0e, Tab = 0x0f,
	Q = 0x10, W = 0x11, E = 0x12, R = 0x13, T = 0x14, Y = 0x15, U = 0x16, I = 0x17, O = 0x18, P = 0x19, L_Bracket = 0x1a, R_Bracket = 0x1b, Enter = 0x1c, L_Ctrl = 0x1d, 
	A = 0x1e, S = 0x1f, D = 0x20, F = 0x21, G = 0x22, H = 0x23, J = 0x24, K = 0x25, L = 0x26, Colon = 0x27, Quote = 0x28, Wave = 0x29, L_Shift = 0x2a, Backslash = 0x2b, 
	Z = 0x2c, X = 0x2d, C = 0x2e, V = 0x2f, B = 0x30, N = 0x31, M = 0x32, Comma = 0x33, Dot = 0x34, Question = 0x35, R_Shift = 0x36, PrtSc = 0x37, L_Alt = 0x38, Space = 0x39, CapsLock = 0x3a, 
	F1 = 0x3b, F2 = 0x3c, F3 = 0x3d, F4 = 0x3e, F5 = 0x3f, F6 = 0x40, F7 = 0x41, F8 = 0x42, F9 = 0x43, F10 = 0x44, NumLock = 0x45, ScrollLock = 0x46
}

struct Input
{
	public InputType Type;
	public InputUnion Info;
}

[StructLayout(LayoutKind.Explicit)]
struct InputUnion
{
	[FieldOffset(0)]public MouseInput mi;
	[FieldOffset(0)]public KeyInput ki;
	[FieldOffset(0)]public HardwareInput hi;
}

[StructLayout(LayoutKind.Sequential)]
struct MouseInput
{
	public int X, Y;
	public uint MouseData;			// Flags包含滚轮时滚轮移动量
	public MouseEventFlag Flags;
	public uint Time = 0;
	public IntPtr ExtraInfo = 0;
	public MouseInput(int x, int y, uint mouseData, MouseEventFlag flags) => 
		(X, Y, MouseData, Flags) = (x, y, mouseData, flags);
}

[StructLayout(LayoutKind.Sequential)]
struct KeyInput
{
	public ushort WVirtualCode;
	public ScanCode WScan;
	public KeyEventFlag Flags;
	public uint Time = 0;
	public IntPtr ExtraInfo = 0;
	public KeyInput(ushort wvcode, ScanCode wscan, KeyEventFlag flags) =>
		(WVirtualCode, WScan, Flags) = (wvcode, wscan, flags);
}

[StructLayout(LayoutKind.Sequential)]
struct HardwareInput
{
	uint Message;
	ushort ParamL, ParamH;
	public HardwareInput(uint message, ushort paraml, ushort paramh) => 
		(Message, ParamL, ParamH) = (message, paraml, paramh);
}

static class WinInput
{
	public static int ScreenX = 1920, ScreenY = 1080;
	public static float Factor = 1.25f;
	public static Dictionary<int, ScanCode> mate_dict = new Dictionary<int, ScanCode>(){
		{1, ScanCode.K_1},
		{2, ScanCode.K_2},
		{3, ScanCode.K_3},
		{4, ScanCode.K_4}
	};


	[DllImport("user32.dll")]
	private static extern uint SendInput(uint count, Input[] list, int size);
	[DllImport("user32.dll")]
	private static extern IntPtr GetMessageExtraInfo();

	public static void KeyDown(ScanCode key, bool Shift = false, bool Ctrl = false, bool Alt = false)
	{
		LinkedList<Input> actions = new LinkedList<Input>();
		Input keyInput = new Input() {
			Type = InputType.Key,
			Info = new InputUnion() {
				ki = new KeyInput() {
					WVirtualCode = 0,
					WScan = key,
					Flags = KeyEventFlag.ScanCode,
					ExtraInfo = GetMessageExtraInfo()
				}}};
		actions.AddFirst(keyInput);
		if(Shift)
		{
			Input shiftInput = new Input() {
				Type = InputType.Key,
				Info = new InputUnion() {
					ki = new KeyInput() {
						WVirtualCode = 0,
						WScan = ScanCode.L_Shift,
						Flags = KeyEventFlag.ScanCode,
						ExtraInfo = GetMessageExtraInfo()
					}}};
			actions.AddFirst(shiftInput);
		}
		if(Ctrl)
		{
			Input CtrlInput = new Input() {
				Type = InputType.Key,
				Info = new InputUnion() {
					ki = new KeyInput() {
						WVirtualCode = 0,
						WScan = ScanCode.L_Ctrl,
						Flags = KeyEventFlag.ScanCode,
						ExtraInfo = GetMessageExtraInfo()
					}}};
			actions.AddFirst(CtrlInput);
		}
		if(Alt)
		{
			Input AltInput = new Input() {
				Type = InputType.Key,
				Info = new InputUnion() {
					ki = new KeyInput() {
						WVirtualCode = 0,
						WScan = ScanCode.L_Alt,
						Flags = KeyEventFlag.ScanCode,
						ExtraInfo = GetMessageExtraInfo()
					}}};
			actions.AddFirst(AltInput);
		}
		SendInput((uint)actions.Count, actions.ToArray(), Marshal.SizeOf(typeof(Input)));
	}

	public static void KeyUp(ScanCode key, bool Shift = false, bool Ctrl = false, bool Alt = false)
	{
		LinkedList<Input> actions = new LinkedList<Input>();
		Input keyInput = new Input() {
			Type = InputType.Key,
			Info = new InputUnion() {
				ki = new KeyInput() {
					WVirtualCode = 0,
					WScan = key,
					Flags = KeyEventFlag.KeyUp | KeyEventFlag.ScanCode,
					ExtraInfo = GetMessageExtraInfo()
				}}};
		actions.AddLast(keyInput);
		if(Shift)
		{
			Input shiftInput = new Input() {
				Type = InputType.Key,
				Info = new InputUnion() {
					ki = new KeyInput() {
						WVirtualCode = 0,
						WScan = ScanCode.L_Shift,
						Flags = KeyEventFlag.KeyUp | KeyEventFlag.ScanCode,
						ExtraInfo = GetMessageExtraInfo()
					}}};
			actions.AddLast(shiftInput);
		}
		if(Ctrl)
		{
			Input CtrlInput = new Input() {
				Type = InputType.Key,
				Info = new InputUnion() {
					ki = new KeyInput() {
						WVirtualCode = 0,
						WScan = ScanCode.L_Ctrl,
						Flags = KeyEventFlag.KeyUp | KeyEventFlag.ScanCode,
						ExtraInfo = GetMessageExtraInfo()
					}}};
			actions.AddLast(CtrlInput);
		}
		if(Alt)
		{
			Input AltInput = new Input() {
				Type = InputType.Key,
				Info = new InputUnion() {
					ki = new KeyInput() {
						WVirtualCode = 0,
						WScan = ScanCode.L_Alt,
						Flags = KeyEventFlag.KeyUp | KeyEventFlag.ScanCode,
						ExtraInfo = GetMessageExtraInfo()
					}}};
			actions.AddLast(AltInput);
		}
		SendInput((uint)actions.Count, actions.ToArray(), Marshal.SizeOf(typeof(Input)));
	}

	public static async Task KeyPress(ScanCode key, int duration = 50, bool Shift = false, bool Ctrl = false, bool Alt = false, CancellationToken? token = null)
	{
		KeyDown(key, Shift, Ctrl, Alt);
		Random random = new Random();
		duration = duration + random.Next(64) * duration / 256;
		var time = DateTime.Now;
		while((DateTime.Now - time).TotalMilliseconds < duration && (token?.IsCancellationRequested ?? true));
		KeyUp(key, Shift, Ctrl, Alt);
	}

	public static void MouseDown(bool isLeft, int x = 0, int y = 0, bool relative = true)
	{
		LinkedList<Input> actions = new LinkedList<Input>();
		Input mouseInput = new Input() {
			Type = InputType.Mouse,
			Info = new InputUnion() {
				mi = new MouseInput() {
					X = (int)(x * Factor * 65536 / ScreenX), Y = (int)(y * Factor * 65536 / ScreenY),
					MouseData = 0,
					Flags = isLeft ? MouseEventFlag.LeftDown : MouseEventFlag.RightDown,
					ExtraInfo = GetMessageExtraInfo()
				}}};
		if(x != 0 || y != 0)
			mouseInput.Info.mi.Flags = mouseInput.Info.mi.Flags | MouseEventFlag.Move;
		if(!relative)
			mouseInput.Info.mi.Flags = mouseInput.Info.mi.Flags | MouseEventFlag.Absolute;
		actions.AddFirst(mouseInput);
		SendInput((uint)actions.Count, actions.ToArray(), Marshal.SizeOf(typeof(Input)));
	}

	public static void MouseUp(bool isLeft, int x = 0, int y = 0, bool relative = true)
	{
		LinkedList<Input> actions = new LinkedList<Input>();
		Input mouseInput = new Input() {
			Type = InputType.Mouse,
			Info = new InputUnion() {
				mi = new MouseInput() {
					X = (int)(x * Factor * 65536 / ScreenX), Y = (int)(y * Factor * 65536 / ScreenY),
					MouseData = 0,
					Flags = isLeft ? MouseEventFlag.LeftUp : MouseEventFlag.RightUp,
					ExtraInfo = GetMessageExtraInfo()
				}}};
		if(x != 0 || y != 0)
			mouseInput.Info.mi.Flags = mouseInput.Info.mi.Flags | MouseEventFlag.Move;
		if(!relative)
			mouseInput.Info.mi.Flags = mouseInput.Info.mi.Flags | MouseEventFlag.Absolute;
		actions.AddFirst(mouseInput);
		SendInput((uint)actions.Count, actions.ToArray(), Marshal.SizeOf(typeof(Input)));
	}

	public static async Task MousePress(bool isLeft, int duration = 50, int x = 0, int y = 0, bool relative = true, CancellationToken? token = null)
	{
		MouseDown(isLeft, x, y, relative);
		Random random = new Random();
		duration = duration + random.Next(64) * duration / 256;
		var time = DateTime.Now;
		while((DateTime.Now - time).TotalMilliseconds < duration && (token?.IsCancellationRequested ?? true));
		MouseUp(isLeft, x, y, relative);
	}

	public static void MouseMove(int x, int y, bool accurate = false)
	{
		LinkedList<Input> actions = new LinkedList<Input>();
		(int _X, int _Y) = accurate ? ((int)(x * Factor / ScreenX), (int)(y * Factor / ScreenY))
			: ((int)(x * Factor * 65536 / ScreenX), (int)(y * Factor * 65536 / ScreenY));
		Input mouseInput = new Input() {
			Type = InputType.Mouse,
			Info = new InputUnion() {
				mi = new MouseInput() {
					X = _X, Y = _Y,
					MouseData = 0,
					Flags = MouseEventFlag.Move,
					ExtraInfo = GetMessageExtraInfo()
				}}};
		actions.AddFirst(mouseInput);
		SendInput((uint)actions.Count, actions.ToArray(), Marshal.SizeOf(typeof(Input)));
	}
}