using OrderSystem.Worker.Processor;
using Serilog;

var cancellationToken = new CancellationTokenSource().Token;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/processor.log", rollingInterval: RollingInterval.Day)
    .Enrich.FromLogContext()
    .CreateLogger();

await Host.CreateDefaultBuilder(args)
    .UseSerilog()
    .ConfigureServices(services =>
    {
        services.AddHostedService<OrderProcessorWorker>();
    })
    .Build()
    .RunAsync(cancellationToken);
