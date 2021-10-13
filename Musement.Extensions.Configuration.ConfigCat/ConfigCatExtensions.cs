using Musement.Extensions.Configuration.ConfigCat;
using System;

namespace Microsoft.Extensions.Configuration
{
    public static class ConfigCatExtensions
    {
        public static IConfigurationBuilder AddConfigCat(this IConfigurationBuilder configurationBuilder,
            Action<ConfigCatConfigurationProviderOptions> optionsBuilder)
        {
            if (configurationBuilder is null)
            {
                throw new ArgumentNullException(nameof(configurationBuilder));
            }

            var options = new ConfigCatConfigurationProviderOptions();
            optionsBuilder?.Invoke(options);

            var source = new ConfigCatConfigurationSource(options);
            configurationBuilder.Add(source);

            return configurationBuilder;
        }
    }
}
