using ConfigCat.Client;
using System;
using ConfigCat.Client.Configuration;

namespace Musement.Extensions.Configuration.ConfigCat
{
    public class ConfigCatConfigurationProviderOptions
    {
        public ConfigCatConfigurationProviderOptions()
        {
            CreateClient = ClientBuilder;
        }

        public Action<ConfigCatClientOptions> Configuration { get; set; } =
            _ => throw new InvalidOperationException("You must set ConfigCatConfigurationProviderOptions.ConfigurationBuilder.");

        public Func<string, bool>? KeyFilter { get; set; }
        public Func<string, string, string>? KeyMapper { get; set; }
        public string? SdkKey { get; set; }
        public Func<ConfigCatClientOptions, IConfigCatClient> CreateClient { get; set; } =
            _ => throw new InvalidOperationException();

        private IConfigCatClient ClientBuilder(ConfigCatClientOptions arg)
        {
            if (SdkKey is null)
            {
                throw new InvalidOperationException("Missing SDK Key in ConfigCatConfigurationProvider");
            }

            return ConfigCatClient.Get(SdkKey, opt =>
            {
                opt.PollingMode = arg.PollingMode;
                opt.DataGovernance = arg.DataGovernance;
            });
        }
    }
}
