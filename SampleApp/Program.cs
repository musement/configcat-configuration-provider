using ConfigCat.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;

namespace SampleApp
{
    sealed class Program
    {
        static async Task Main()
        {
            var host = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((context, builder) =>
                {
                    builder.AddUserSecrets<Program>();

                    builder.AddConfigCat(o =>
                    {
                        var conf = builder.Build();
                        o.SdkKey = conf["Secrets:ConfigCatKey"];
                        o.Configuration = c =>
                        {
                            c.DataGovernance = DataGovernance.EuOnly;
                        };
                        o.KeyMapper = (key, value) =>
                        {
#pragma warning disable CA1307 // Specify StringComparison for clarity REASON: support net48
                            var mappedKey = key.Replace("__", ConfigurationPath.KeyDelimiter);
#pragma warning restore CA1307 // Specify StringComparison for clarity
                            return "Cat:" + mappedKey;
                        };
                    });
                })
                .Build();

            var config = host.Services.GetRequiredService<IConfiguration>();

            do
            {
                foreach (var configSection in config.GetSection("Cat").AsEnumerable())
                {
                    Console.WriteLine($"{configSection.Key}: {configSection.Value}");
                }

                await Task.Delay(15_000);
            } while (true);
        }
    }
}
