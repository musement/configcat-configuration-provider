using Microsoft.Extensions.Configuration;
using Moq;
using Musement.Extensions.Configuration.ConfigCat;
using System;
using Xunit;

namespace ConfigCatProvider.Tests
{
    public class ConfigCatExtensionsTests
    {
        private Mock<IConfigurationBuilder> BuilderMock { get; }
        
        public ConfigCatExtensionsTests()
        {
            BuilderMock = new();
        }

        [Fact]
        public void ConfigCatConfigurationSourceCanBeAddedViaExtensionMethod()
        {
            BuilderMock.Setup(m => m.Add(It.IsAny<IConfigurationSource>()));

            ConfigCatExtensions.AddConfigCat(BuilderMock.Object, _ => { });

            BuilderMock.Verify(m => m.Add(It.IsAny<ConfigCatConfigurationSource>()));
        }

        [Fact]
        public void ExtensionThrowsIfBuilderIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => ConfigCatExtensions.AddConfigCat(null!, _ => { }));
        }
    }
}
