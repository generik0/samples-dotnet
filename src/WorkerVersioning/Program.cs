using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Temporalio.Client;
using Temporalio.Extensions.Hosting;
using TemporalioSamples.WorkerVersioning;

var services = new ServiceCollection();
services.AddTemporalClient(
    opt =>
    {
        opt.TargetHost = "localhost:7233";
        opt.Namespace = "default";
        opt.LoggerFactory = LoggerFactory.Create(builder =>
            builder.AddSimpleConsole(options => options.TimestampFormat = "[HH:mm:ss] ")
                .SetMinimumLevel(LogLevel.Information));
    });

var taskQueue = $"worker-versioning-{Guid.NewGuid()}";

var builder = services.AddHostedTemporalWorker(taskQueue);
builder.AddWorkflow<MyWorkflowV1Dot1>()
    //.ConfigureOptions(options =>
    //{
    //    options.BuildId = "V1.1"; //version number
    //    options.UseWorkerVersioning = true;
    //})
    .AddScopedActivities<MyActivities>();

var sp = services.BuildServiceProvider();
var temporalClient = sp.GetRequiredService<ITemporalClient>();
try
{
    //await temporalClient.UpdateWorkerBuildIdCompatibilityAsync(taskQueue, new BuildIdOp.AddNewDefault("V1.1"));

    var id = Guid.NewGuid().ToString();
    var handle = await temporalClient.StartWorkflowAsync(
        (MyWorkflowV1Dot1 workflow) => workflow.RunAsync(id),
        new WorkflowOptions(id: id, taskQueue: taskQueue)
        {
            StartSignal = "Chad Rocks"
        });
    await handle.SignalAsync(workflow => workflow.ProceederAsync("V1.1"));
}
catch (Exception exception)
{
    Console.WriteLine(exception);
}

Console.ReadLine();