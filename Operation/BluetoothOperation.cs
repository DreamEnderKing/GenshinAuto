namespace GenshinAuto.Operation;

using System.IO.Ports;
using System.Text.RegularExpressions;
using GenshinAuto.Windows;

static class BluetoothOperation
{
	private static string port = "COM3";

	private static int x, y;
	private static bool z;
	private static bool old_z = false;
	private static bool e_pressed = false;
	private static bool mate_changed = false;
	private static DateTime mate_changed_time;
	private static double move_linear_factor = 50000;
	private static double move_limit_factor = 384;
	private static double move_pow_factor = 2;
	public static int mate = 1;

	private static DateTime lastRush;
	private static DateTime lastFlush;

	private static void MainProcess(CancellationToken token)
	{
		bool found = false;
		foreach(var p in SerialPort.GetPortNames())
			if(port == p)
			{
				found = true;
				break;
			}
		if(!found)
		{
			Console.WriteLine($"Port {port} not found!");
			return;
		}
		var Serial = new SerialPort(port, 9600);
		try
		{
			WinInput.KeyPress(WinInput.mate_dict[mate]).Wait();
			if(Serial.IsOpen)
				Serial.Close();
			Serial.Open();
			lastRush = DateTime.Now;
			lastFlush = DateTime.Now;
			Regex regex = new Regex(@"^\([0-9]+, [0-9]+\), [0-9]+");
			var tasks = new List<Task>();
			var cancelSource = new CancellationTokenSource();
			while(!token.IsCancellationRequested)
			{
				string message = Serial.ReadLine();
				if(!regex.Match(message).Success)
					continue;
				message = message.TrimEnd('\r', '\n');
				var sub_strs = message.Split(',');
				x = int.Parse(sub_strs[0].TrimStart('('));
				y = int.Parse(sub_strs[1].TrimStart().TrimEnd(')'));
				z = sub_strs[2].TrimStart() == "1";

				if((DateTime.Now - lastFlush).TotalMilliseconds > 1000)
				{
					cancelSource.Cancel();
					Task.WaitAll(tasks.ToArray());
					tasks.Clear();
					cancelSource = new CancellationTokenSource();
					lastFlush = DateTime.Now;
					Serial.ReadExisting();
					continue;
				}

				if(y > 768)
				{
					tasks.Add(WinInput.KeyPress(ScanCode.W, duration: 20, token: cancelSource.Token));
					WinInput.MouseMove((int)Math.Round(((x > 512) ? -1 : 1)*move_linear_factor * Math.Pow(Math.Abs((x - 512) / move_limit_factor), move_pow_factor)), 0, true);
					if(x > 384 && x < 640 && (DateTime.Now - lastRush).Seconds >= 4)
						WinInput.MousePress(false, duration: 10);
				}
				else if(y < 256)
				{
					tasks.Add(WinInput.KeyPress(ScanCode.S, duration: 20));
					// 左后摇触发q，右后摇触发e
					if(x < 256)
					{
						tasks.Add(WinInput.KeyPress(ScanCode.E, duration: 500, token: cancelSource.Token));
					}
					if(x > 768)
					{
						tasks.Add(WinInput.KeyPress(ScanCode.Q, duration: 20, token: cancelSource.Token));
					}
				}
				else if(x > 768 && !mate_changed)
				{
					mate_changed = true;
					mate_changed_time = DateTime.Now;
					mate = mate == 1 ? 4 : mate - 1;
					tasks.Add(WinInput.KeyPress(WinInput.mate_dict[mate], duration: 10));
				}
				else if(x < 256 && !mate_changed)
				{
					mate_changed = true;
					mate_changed_time = DateTime.Now;
					mate = mate == 4 ? 1 : mate + 1;
					tasks.Add(WinInput.KeyPress(WinInput.mate_dict[mate], duration: 10));
				}
				else
				{
					if((DateTime.Now - mate_changed_time).TotalMilliseconds > 1000)
					{
						mate_changed = false;
					}
				}

				if(z != old_z)
				{
					tasks.Add(WinInput.KeyPress(ScanCode.Space, duration: 10, token: cancelSource.Token));
					old_z = z;
				}
			}
		}
		catch(Exception e)
		{
			Console.WriteLine($"[Error]{e.Message}");
		}
		finally
		{
			if(Serial.IsOpen)
				Serial.Close();
		}
	}

	public static Task Run(CancellationToken token) => Task.Run(() => MainProcess(token));
}