using Microsoft.Extensions.Configuration;
using Moq;
using Musement.Extensions.Configuration.ConfigCat;
using Xunit;

namespace ConfigCatProvider.Tests
{
    public class ConfigCatConfigurationSourceTests
    {
        private ConfigCatConfigurationProviderOptions Options { get; }

        public ConfigCatConfigurationSourceTests()
        {
            Options = new ConfigCatConfigurationProviderOptions
            {
                Configuration = c =>
                {
                    c.SdkKey = "foobar";
                    return c;
                }
            };
        }

        [Fact]
        public void BuildCreatesAnIConfigurationProvider()
        {
            var builder = new Mock<IConfigurationBuilder>();
            var sut = new ConfigCatConfigurationSource(Options);

            var provider = sut.Build(builder.Object);

            Assert.NotNull(provider);
            Assert.IsType<ConfigCatConfigurationProvider>(provider);
        }
    }
}
