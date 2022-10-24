using ConfigCat.Client;
using System;
using ConfigCat.Client.Configuration;

namespace Musement.Extensions.Configuration.ConfigCat
{
    public class ConfigCatConfigurationProviderOptions
    {
        public Action<ConfigCatClientOptions> Configuration { get; set; } =
            _ => throw new InvalidOperationException("You must set ConfigCatConfigurationProviderOptions.ConfigurationBuilder.");

        public Func<string, bool>? KeyFilter { get; set; }
        public Func<string, string, string>? KeyMapper { get; set; }

        public Func<ConfigCatClientOptions, IConfigCatClient> CreateClient { get; set; } = (c) =>
            new ConfigCatClient(
                opt =>
                {
                    opt.PollingMode = c.PollingMode;
                    opt.SdkKey = c.SdkKey;
                    opt.DataGovernance = c.DataGovernance;

                });
    }
}
