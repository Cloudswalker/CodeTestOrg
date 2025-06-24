using LogComponent;
using LogComponent.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LogUsers
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Start");
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            using (var serviceProvider = serviceCollection.BuildServiceProvider())
            {
                var logger = serviceProvider.GetRequiredService<ILog>();

                for (int i = 0; i < 15; i++)
                {
                    logger.Write("Number with Flush: " + i.ToString());
                    Thread.Sleep(50);
                }

                logger.StopWithFlush();
            }

            Console.WriteLine("Finished part 1...");
            using (var serviceProvider = serviceCollection.BuildServiceProvider())
            {
                ILog logger2 = serviceProvider.GetRequiredService<ILog>();

                var loggingTask = Task.Run(() =>
                {
                    for (int i = 50; i > 0; i--)
                    {
                        logger2.Write("Number with No flush: " + i.ToString());
                        Thread.Sleep(20);
                    }
                });
                //wait for a moment to ensure some logs are written
                Task.Delay(300).Wait();
                logger2.StopWithoutFlush();
                //wait for the logging task to finish
                loggingTask.Wait();
                Console.WriteLine("Finished writing logs. Press Enter to exit.");
                Console.ReadLine();
            }
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            services.AddSingleton<IConfiguration>(configuration);
            services.AddSingleton<ILogWriter>(provider =>
            {
                var config = provider.GetRequiredService<IConfiguration>();
                var dir = config["Logging:Directory"] ?? "C:\\DefaultLogDir";
                return new FileLogWriter(dir);
            });
            services.AddTransient<ILog, AsyncLog>();
        }
    }
}
