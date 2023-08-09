using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting.Internal;
using System.Text;

namespace mParticleAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class LoadTestController : ControllerBase
{
    private static int _requestCount = 0;
    private static int _totalRequestCount = 0;
    private Random _random = new Random();
    //If not in development mode, the body content is not propogated via middleware
    private static bool isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";
    //Print raw post data to verify formatting etc
    private static bool printRaw = false;


    private readonly ILogger<LoadTestController> _logger;
    private readonly IMemoryCache _memoryCache;

    public LoadTestController(ILogger<LoadTestController> logger)
    {
        _logger = logger;
    }

    [HttpGet()]
    [ActionName("Clear")]
    [Route("Clear")]
    public Task<ObjectResult> Clear()
    {
        _totalRequestCount = 0;
        return Task.FromResult(StatusCode(StatusCodes.Status200OK, new TestDataResponse { Successful = true }));
    }

    [HttpGet(Name = "GetTest")]
    public async Task<ObjectResult> Get()
    {
        Interlocked.Increment(ref _requestCount);
        Interlocked.Increment(ref _totalRequestCount);

        Log($"Starting the process {_requestCount}");
        var delay = await TaskTest();
        Interlocked.Decrement(ref _requestCount);

        //Throw in some chaos engineering, let's fail a certain percentage
        int num = _random.Next(1, 50);
        if (num >= 40)
        {
            Log($"Returning the process as failure {_requestCount} delayed by {delay}");
            return StatusCode(StatusCodes.Status500InternalServerError, new TestDataResponse { Successful = false });
        }
        Log($"Returning the process {_requestCount} delayed by {delay} total count: {_totalRequestCount}");
        return StatusCode(StatusCodes.Status200OK, new TestDataResponse { Successful = true });
    }

    [HttpPost(Name = "PostTest")]
    public async Task<ObjectResult> Post(TestData data)
    {
        if (isDevelopment && printRaw)
        {
            Request.Body.Position = 0;
            using (var reader = new StreamReader(Request.Body, Encoding.UTF8))
            {
                var bodyContent = await reader.ReadToEndAsync();
                Log($"Raw Request body: {bodyContent} Parsed: Name: {data.Name} RequestsSent: {data.RequestsSent} Date: {data.Date}");
            }
        }

        Interlocked.Increment(ref _requestCount); 
        Interlocked.Increment(ref _totalRequestCount);
        
        //Log($"Starting the process {_requestCount}");
        var delay = await TaskTest();
        Interlocked.Decrement(ref _requestCount);

        //Throw in some chaos engineering, let's fail a certain percentage
        int num = _random.Next(1, 50);
        if (num >= 45)
        {
            Log($"Returning the process as failure {_requestCount} delayed by {delay}");
            return StatusCode(StatusCodes.Status500InternalServerError, new TestDataResponse { Successful = false });
        }
        //Throw in some chaos engineering, let's return an un-successful response
        num = _random.Next(1, 50);
        if (num >= 45)
        {
            Log($"Returning the process as 200 WITH Successful=false {_requestCount} delayed by {delay}");
            return StatusCode(StatusCodes.Status200OK, new TestDataResponse { Successful = false }); ;
        }
        Log($"Returning the process {_requestCount}  delayed by {delay} received count: {_totalRequestCount} sent count: {data.RequestsSent}");
        return StatusCode(StatusCodes.Status200OK, new TestDataResponse { Successful = true});
    }

    /// <summary>
    /// Delay the task to sumulate a slow server
    /// </summary>
    /// <returns></returns>
    private Task<int> TaskTest()
    {
        int num = _random.Next(1000, 2000);
        //For the top 10% lets add in a chance to really delay things
        if (num > 1900)
        {
            num = _random.Next(2000, 4000);
        }
        return Task.Run(async () => {
            await Task.Delay(num);
            return num;
        });
    }

    /// <summary>
    /// Centralize logging
    /// </summary>
    /// <param name="message"></param>
    private void Log(string message)
    {
        Console.WriteLine(message);
    }
}
