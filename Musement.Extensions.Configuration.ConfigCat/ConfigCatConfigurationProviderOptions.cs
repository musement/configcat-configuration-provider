using ConfigCat.Client;
using System;

namespace Musement.Extensions.Configuration.ConfigCat
{
    public class ConfigCatConfigurationProviderOptions
    {
        public Action<AutoPollConfiguration> Configuration { get; set; } =
            _ => throw new InvalidOperationException("You must set ConfigCatConfigurationProviderOptions.ConfigurationBuilder.");

        public Func<string, bool>? KeyFilter { get; set; }
        public Func<string, string, string>? KeyMapper { get; set; }
        public Func<AutoPollConfiguration, IConfigCatClient> CreateClient { get; set; } = c => new ConfigCatClient(c);
    }
}
