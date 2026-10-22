using Android.App;
using Android.Runtime;

namespace CMMS_QRCode_Scanner;

[Application]
public class MainApplication : MauiApplication
{
	public MainApplication(IntPtr handle, JniHandleOwnership ownership)
		: base(handle, ownership)
	{
	}

	public override void OnCreate()
	{
		base.OnCreate();

		AndroidEnvironment.UnhandledExceptionRaiser += (sender, args) =>
		{
			System.Diagnostics.Debug.WriteLine($"[CRASH AndroidEnvironment] {args.Exception}");
		};

		AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
		{
			System.Diagnostics.Debug.WriteLine($"[CRASH AppDomain] {args.ExceptionObject}");
		};

		TaskScheduler.UnobservedTaskException += (sender, args) =>
		{
			System.Diagnostics.Debug.WriteLine($"[CRASH TaskScheduler] {args.Exception}");
			args.SetObserved();
		};
	}

	protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
