using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;
using ConfigCat.Client;
using ConfigCat.Client.Configuration;

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
                            var mappedKey = key.Replace("__", ConfigurationPath.KeyDelimiter, StringComparison.OrdinalIgnoreCase);
                            return "Cat:" + mappedKey;
                        };
                    });
                })
                .Build();

            var config = host.Services.GetRequiredService<IConfiguration>();

            do
            {
                foreach (var (key, value) in config.GetSection("Cat").AsEnumerable())
                {
                    Console.WriteLine($"{key}: {value}");
                }

                await Task.Delay(15_000);
            } while (true);
        }
    }
}
