﻿using LogComponent;
using LogComponent.Interfaces;
using LogUnitTests;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

[TestFixture]
public class AsyncLogTests
{
    private ServiceProvider _provider;
    private const string LogDir = @"C:\LogTest";
    private FaultyLogWriter _faultyLogWriter;

    [SetUp]
    public void Setup()
    {
        if (Directory.Exists(LogDir))
            Directory.Delete(LogDir, true);

        var services = new ServiceCollection();

        // Register the writer to check the result
        _faultyLogWriter = new FaultyLogWriter();
        services.AddSingleton<ILogWriter>(_ => _faultyLogWriter);

        // DI-registration of logger with custom writer
        services.AddTransient<ILog, AsyncLog>();

        _provider = services.BuildServiceProvider();
    }

    [TearDown]
    public void TearDown()
    {
        _provider.Dispose();
    }

    [Test]
    public void WriteWritesLogLineToFile()
    {
        using var logger = new AsyncLog();

        string testMessage = "Test log message";
        logger.Write(testMessage);

        // Stop with Flush ensures all logs are written
        logger.StopWithFlush();
        logger.Dispose();
        // Wait for the file to be created
        var logFiles = Directory.GetFiles(LogDir);
        Assert.IsNotEmpty(logFiles, "No log files created");

        string content = File.ReadAllText(logFiles.First());

        Assert.That(content, Does.Contain(testMessage));
    }

    [Test]
    public void Write_CreatesNewFileAfterMidnight()
    {
        DateTime fakeNow = new(2025, 6, 24, 23, 59, 59);
        DateTime AdvanceOneSecond() => fakeNow = fakeNow.AddSeconds(1);

        var fileLogWriter = new FileLogWriter(LogDir, () => fakeNow);

        var lineBeforeMidnight = new LogLine(fakeNow, "Before midnight");
        var lineAfterMidnight = new LogLine(AdvanceOneSecond(), "After midnight");

        // Write lines
        fileLogWriter.WriteAsync(lineBeforeMidnight, CancellationToken.None).Wait();
        fileLogWriter.WriteAsync(lineAfterMidnight, CancellationToken.None).Wait();

        fileLogWriter.Dispose();

        var files = Directory.GetFiles(LogDir);
        Assert.That(files.Length, Is.EqualTo(2), "Expected 2 log files due to date change");
    }

    [Test]
    public void StopWithFlush_WaitsUntilAllLogsAreWritten()
    {
        using var logger = new AsyncLog();

        for (int i = 0; i < 10; i++)
        {
            logger.Write($"Log line {i}");
        }

        // Stop with Flush ensures all 10 logs are written
        logger.StopWithFlush();
        logger.Dispose();
        // After stopping, check that all logs are present
        var logFiles = Directory.GetFiles(LogDir);
        Assert.IsNotEmpty(logFiles, "No log files found");

        string allLogs = string.Join("\n", logFiles.Select(File.ReadAllText));
        for (int i = 0; i < 10; i++)
        {
            Assert.That(allLogs, Does.Contain($"Log line {i}"));
        }
    }

    [Test]
    public void StopWithoutFlush_StopsImmediatelyWithoutWritingAllLogs()
    {
        using var logger = new AsyncLog();


        var loggingTask = Task.Run(() =>
        {
            for (int i = 50; i > 0; i--)
            {
                logger.Write($"Log line {i}");
                Thread.Sleep(20);
            }
        });
        //Let's give some time it to write some logs
        Thread.Sleep(500);
        // Stop without Flush does not guarantee all logs are written
        logger.StopWithoutFlush();

        // Let's give some time for async writes to complete, but not wait for full flush
        loggingTask.Wait();
        logger.Dispose();

        var logFiles = Directory.GetFiles(LogDir);
        Assert.IsNotEmpty(logFiles, "No log files found");

        string allLogs = string.Join("\n", logFiles.Select(File.ReadAllText));

        // Check that at least some logs were written
        Assert.That(allLogs.Length, Is.GreaterThan(0));

        // Maybe some logs were not written, check that at least one log is present
        Assert.That(allLogs, Does.Contain("Log line"));
    }

    [Test]
    public void Write_IsThreadSafe_WhenCalledFromMultipleThreads()
    {
        using var logger = new AsyncLog();

        int totalMessages = 100;
        Parallel.For(0, totalMessages, i =>
        {
            logger.Write($"Parallel log {i}");
        });

        logger.StopWithFlush();
        logger.Dispose();

        var logFiles = Directory.GetFiles(LogDir);
        Assert.IsNotEmpty(logFiles, "No log files created");

        string allLogs = string.Join("\n", logFiles.Select(File.ReadAllText));

        for (int i = 0; i < totalMessages; i++)
        {
            Assert.That(allLogs, Does.Contain($"Parallel log {i}"), $"Missing: Parallel log {i}");
        }
    }

    [Test]
    public void AsyncLog_DoesNotCrash_WhenWriterThrows()
    {
        var logger = _provider.GetRequiredService<ILog>();

        logger.Write("This will fail internally");

        // Call stop to give a chance for internal write
        logger.StopWithFlush();
        logger.Dispose();

        // We expect no crash, so if we reach here, the test passes
        Assert.Pass("No crash occurred");
    }

    #region EdgeCases

    [Test]
    public void Write_AfterStopWithoutFlush_DoesNotWriteToFile()
    {
        var logger = new AsyncLog(new FileLogWriter(LogDir));
        logger.StopWithoutFlush();

        logger.Write("Should not be written");
        Thread.Sleep(200); // let it process any pending writes
        logger.Dispose();

        var files = Directory.GetFiles(LogDir);
        string contents = files.Any() ? File.ReadAllText(files.First()) : "";

        Assert.That(contents, Does.Not.Contain("Should not be written"));
    }

    [Test]
    public void VeryLongLogLine_IsWrittenCompletely()
    {
        var logger = new AsyncLog(new FileLogWriter(LogDir));

        string longText = new string('X', 10_000);
        logger.Write(longText);
        logger.StopWithFlush();

        logger.Dispose();
        var file = Directory.GetFiles(LogDir).First();
        string content = File.ReadAllText(file);

        Assert.That(content, Does.Contain(new string('X', 10_000)));
    }

    [Test]
    public void DoubleDispose_DoesNotThrow()
    {
        var log = new AsyncLog(new FileLogWriter(LogDir));

        log.Dispose();
        Assert.DoesNotThrow(() => log.Dispose());
    }
    #endregion
}
