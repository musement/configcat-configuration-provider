using ConfigCat.Client;
using System;
using ConfigCat.Client.Configuration;

namespace Musement.Extensions.Configuration.ConfigCat
{
    public class ConfigCatConfigurationProviderOptions
    {
        public Func<ConfigCatClientOptions, ConfigCatClientOptions>? Configuration { get; set; }

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
