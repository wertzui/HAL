using HAL.Common;
using HAL.Common.Converters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.Json;
using System.Threading.Tasks;

namespace HAL.Tests.Common.Converters;

[TestClass]
public class ResourceOfTJsonConverterFactoryTests
{
    [TestMethod]
    public async Task GetTypeInfo_should_be_thread_safe()
    {
        // Arrange
        var factory = new ResourceOfTJsonConverterFactory();
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        var type = typeof(Resource<JsonSerializerOptions>);
        var tcs = new TaskCompletionSource<bool>();

        var tasks = new Task[100];
        for (int i = 0; i < tasks.Length; i++)
        {
            tasks[i] = Task.Run(async () =>
            {
                await tcs.Task; // Wait until all tasks are ready
                var typeInfo = factory.GetTypeInfo(type, options);
                Assert.IsNotNull(typeInfo);
            }, TestContext.CancellationToken);
        }

        // Act
        tcs.SetResult(true); // Release all tasks
        await Task.WhenAll(tasks);
    }

    public TestContext TestContext { get; set; }
}
