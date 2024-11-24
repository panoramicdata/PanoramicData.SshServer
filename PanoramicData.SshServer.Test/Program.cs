using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PanoramicData.SshServer;
using PanoramicData.SshServer.Interfaces;
using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace ExampleApp;

partial class Program
{
	static async Task Main()
	{
		var cancellationTokenSource = new CancellationTokenSource();
		var services = new ServiceCollection();

		// Determine where we will look for the appsettings.json file in debug mode
		var appSettingsPath = "../../../appsettings.json";
		var fileInfo = new FileInfo(Path.Combine(Directory.GetCurrentDirectory(), appSettingsPath));
		var fileExists = fileInfo.Exists;


		// Build configuration
		var configuration = new ConfigurationBuilder()
			.SetBasePath(Directory.GetCurrentDirectory())
#if DEBUG
			.AddJsonFile(fileInfo.FullName, optional: false, reloadOnChange: true)
#else
			.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
#endif
			.Build();

		// Register configuration
		services
			.Configure<ExampleSshApplicationConfig>(configuration.GetSection("Configuration"));

		// Register services
		services
			.AddSingleton<IHostedService, SshServer>()
			.AddSingleton(new StartingInfo(IPAddress.Any, 1022, "SSH-2.0-ExampleApp"))
			.AddSingleton<ISshApplication, ExampleSshApplication>()
			.AddSingleton<IKeyManager, ExampleKeyManager>()
			.AddLogging(configure => configure.AddConsole());

		var serviceProvider = services.BuildServiceProvider();

		// Get the logger
		var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

		// Listen for shutdown requests (e.g., Ctrl+C)
		Console.CancelKeyPress += (sender, e) =>
		{
			logger.LogInformation("Exiting...");
			e.Cancel = true; // Prevents the process from terminating immediately
			cancellationTokenSource.Cancel(); // Trigger cancellation
		};

		logger.LogInformation("Starting SSH server...");

		await serviceProvider
			.GetRequiredService<IHostedService>()
			.StartAsync(cancellationTokenSource.Token);

		logger.LogInformation("SSH server started. Press Ctrl+C to exit.");

		// Wait for the cancellation token to be triggered
		try
		{
			await Task.Delay(Timeout.Infinite, cancellationTokenSource.Token);
		}
		catch (TaskCanceledException)
		{
			logger.LogInformation("Stopping SSH server...");
		}

		await serviceProvider
			.GetRequiredService<IHostedService>()
			.StopAsync(CancellationToken.None);

		logger.LogInformation("SSH server stopped.");
	}
}
