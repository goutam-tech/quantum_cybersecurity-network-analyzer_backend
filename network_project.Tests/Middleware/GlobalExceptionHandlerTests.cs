using System.Net;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using network_project.Middleware;

namespace network_project.Tests.Middleware;

public class GlobalExceptionHandlerTests
{
    [Fact]
    public async Task InvokeAsync_Should_Call_Next_When_No_Exception()
    {
        var context = new DefaultHttpContext();

        var logger = new Mock<ILogger<GlobalExceptionHandler>>();

        var middleware = new GlobalExceptionHandler(
            async (innerHttpContext) =>
            {
                await Task.CompletedTask;
            },
            logger.Object);

        await middleware.InvokeAsync(context);

        Assert.Equal(200, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_Should_Handle_Exception_And_Return_500()
    {
        var context = new DefaultHttpContext();

        context.Response.Body = new MemoryStream();

        var logger = new Mock<ILogger<GlobalExceptionHandler>>();

        var middleware = new GlobalExceptionHandler(
            (innerHttpContext) =>
            {
                throw new Exception("Test exception");
            },
            logger.Object);

        await middleware.InvokeAsync(context);

        context.Response.Body.Seek(0, SeekOrigin.Begin);

        var responseBody = await new StreamReader(context.Response.Body)
            .ReadToEndAsync();

        Assert.Equal((int)HttpStatusCode.InternalServerError,
            context.Response.StatusCode);

        Assert.Equal("application/json",
            context.Response.ContentType);

        Assert.Contains("An unexpected error occurred", responseBody);

        Assert.Contains("Test exception", responseBody);
    }

    [Fact]
    public async Task InvokeAsync_Should_Log_Error_When_Exception_Occurs()
    {
        var context = new DefaultHttpContext();

        context.Response.Body = new MemoryStream();

        var logger = new Mock<ILogger<GlobalExceptionHandler>>();

        var middleware = new GlobalExceptionHandler(
            (innerHttpContext) =>
            {
                throw new Exception("Logging exception");
            },
            logger.Object);

        await middleware.InvokeAsync(context);

        logger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}