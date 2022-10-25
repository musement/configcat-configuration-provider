using ConfigCat.Client;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using ConfigCat.Client.Configuration;

namespace Musement.Extensions.Configuration.ConfigCat
{
    internal class ConfigCatConfigurationProvider : ConfigurationProvider, IDisposable
    {
        private HashSet<(string key, string value)> _lastValues = new();
        private readonly IConfigCatClient _client;
        private readonly Func<string, bool> _keyFilter;
        private readonly Func<string, string, string> _keyMapper;

        public ConfigCatConfigurationProvider(ConfigCatConfigurationProviderOptions options)
        {
            if (options.Configuration is null)
            {
                throw new InvalidOperationException("You must set ConfigCatClientOptions.ConfigurationBuilder.");
            }

            var config = new ConfigCatClientOptions();
            options.Configuration.Invoke(config);

            if (config.PollingMode.GetType() != typeof(AutoPoll))
            {
                throw new InvalidOperationException("Only AutoPoll configuration is allowed.");
            }

            ((AutoPoll)config.PollingMode).OnConfigurationChanged += (s, e) => Reload();

            if (config is null || string.IsNullOrWhiteSpace(config.SdkKey))
            {
                throw new InvalidOperationException("Missing ConfigCat SDK Key");
            }

            _client = options.CreateClient(config);
            _keyFilter = options.KeyFilter ?? (_ => true);
            _keyMapper = options.KeyMapper ??
                         ((key, value) => key.Replace("__", ConfigurationPath.KeyDelimiter,
                             StringComparison.InvariantCultureIgnoreCase));
        }

        private void Reload()
        {
            ReloadAsync()
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();
        }

        public override void Load()
        {
            InitialLoadAsync()
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();
        }

        private async Task ReloadAsync()
        {
            var oldValues = _lastValues;
            var newValues = await FetchLiveConfigurationAsync()
                .ConfigureAwait(false);

            if (oldValues.SetEquals(newValues))
            {
                return;
            }

            _lastValues = newValues;
            SetData(newValues, true);
        }

        private async Task InitialLoadAsync()
        {
            _lastValues = await FetchLiveConfigurationAsync()
                .ConfigureAwait(false);
            SetData(_lastValues, false);
        }

        private void SetData(HashSet<(string key, string value)> values, bool triggerReload)
        {
            Data = values.ToDictionary(
                x => x.key,
                x => x.value,
                StringComparer.OrdinalIgnoreCase
            );

            if (triggerReload)
            {
                OnReload();
            }
        }

        private async Task<HashSet<(string, string)>> FetchLiveConfigurationAsync()
        {
            var allKeys = await _client.GetAllKeysAsync().ConfigureAwait(false);
            var set = new HashSet<(string, string)>();

            foreach (var key in allKeys)
            {
                if (!_keyFilter.Invoke(key))
                {
                    continue;
                }

                var value = await _client.GetValueAsync<object?>(key, default);

                if (value is null)
                {
                    continue;
                }

                var valueString = value.ToString();
                if (valueString!.Equals("True", StringComparison.Ordinal) ||
                    valueString.Equals("False", StringComparison.Ordinal))
#pragma warning disable CA1308
                    // In order to not break compatibility we need to return "true" or "false" for boolean values
                    // default comes as "True" and "False"
                    valueString = valueString.ToLowerInvariant();
#pragma warning restore CA1308

                var mappedKey = _keyMapper.Invoke(key, valueString);
                set.Add((mappedKey, valueString));
            }

            return set;
        }

        public void Dispose()
        {
            _client.Dispose();
        }
    }
}
