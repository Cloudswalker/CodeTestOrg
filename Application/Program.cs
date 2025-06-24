using LogComponent;

namespace LogUsers
{
    using LogComponent.Interfaces;
    using Microsoft.Extensions.DependencyInjection;
    using System.Threading;

    class Program
    {
        static void Main(string[] args)
        {
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
            using (var serviceProvider = serviceCollection.BuildServiceProvider())
            {
                ILog logger2 = serviceProvider.GetRequiredService<ILog>();

                for (int i = 50; i > 0; i--)
                {
                    logger2.Write("Number with No flush: " + i.ToString());
                    Thread.Sleep(20);
                }

                logger2.StopWithoutFlush();

                Console.ReadLine();
            }
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ILogWriter>(provider =>
                new FileLogWriter(@"C:\LogTest"));

            services.AddTransient<ILog, AsyncLog>();
        }
    }
}
