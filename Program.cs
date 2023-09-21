using GenshinAuto.Operation;
using GenshinAuto.Windows;

DateTime tick = DateTime.Now;
TimeSpan span = new TimeSpan(0, 0, 0, 2, 000);

var tasks = new List<Task>();
var source = new CancellationTokenSource();

string t1 = "Please enter the options below:\n" + 
	"1: Bluetooth Operation\n" + 
	"0: Exit";
string t2 = "Task is running, press q to quit.\n";
bool running = true;
bool started = false;
while(running)
{
	try
	{
		if(!started)
		{
			Console.WriteLine(t1);			
			int i = int.Parse(Console.ReadLine() ?? String.Empty);
			switch (i)
			{
				case 1:
					tasks.Add(BluetoothOperation.Run(source.Token));
					Console.Write(t2);
					started = true;
					break;
				case 0:
					running = false;
					break;
				default:
					throw new Exception("Unknown option.");
			}
		}
		else
		{
			var i = Console.ReadLine() ?? String.Empty;
			if(i.StartsWith('q'))
			{
				source.Cancel();
				Task.WaitAll(tasks.ToArray());
				running = false;
			}
		}
	}
	catch(Exception e)
	{
		Console.WriteLine($"[Error]{e.Message}");
	}
}