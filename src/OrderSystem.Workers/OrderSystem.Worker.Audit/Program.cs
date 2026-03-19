using OrderSystem.Worker.Audit;
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
        services.AddHostedService<OrderAuditWorker>();
    })
    .Build()
    .RunAsync(cancellationToken);