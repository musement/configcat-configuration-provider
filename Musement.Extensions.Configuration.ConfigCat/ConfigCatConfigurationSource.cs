using Microsoft.Extensions.Configuration;

namespace Musement.Extensions.Configuration.ConfigCat
{
    internal class ConfigCatConfigurationSource : IConfigurationSource
    {
        private readonly ConfigCatConfigurationProviderOptions _options;

        public ConfigCatConfigurationSource(ConfigCatConfigurationProviderOptions? options)
        {
            _options = options ?? new ConfigCatConfigurationProviderOptions();
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new ConfigCatConfigurationProvider(_options);
        }
    }
}
