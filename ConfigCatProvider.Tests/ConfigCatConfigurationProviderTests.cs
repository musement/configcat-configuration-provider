using ConfigCat.Client;
using Microsoft.Extensions.Configuration;
using Moq;
using Musement.Extensions.Configuration.ConfigCat;
using System;
using System.Reflection;
using Xunit;

namespace ConfigCatProvider.Tests
{
    public class ConfigCatConfigurationProviderTests
    {
        private ConfigCatConfigurationProviderOptions Options { get; }
        public ConfigCatConfigurationProviderTests()
        {
            Options = new ConfigCatConfigurationProviderOptions
            {
                Configuration = c => c.SdkKey = "foobar"
            };
        }

        [Fact]
        public void BuildInvokesOptionsLambda()
        {
            var called = false;
            var options = new ConfigCatConfigurationProviderOptions
            {
                Configuration = c =>
                {
                    called = true;
                    c.SdkKey = "fake";
                }
            };

            using var sut = new ConfigCatConfigurationProvider(options);

            Assert.True(called);
        }

        [Fact]
        public void BuildThrowsIfSdkKeyIsMissing()
        {
            var options = new ConfigCatConfigurationProviderOptions
            {
                Configuration = _ => { }
            };

            Assert.Throws<InvalidOperationException>(() => new ConfigCatConfigurationProvider(options));
        }

        [Fact]
        public void DefaultMapperConvertsDoubleUnderscores()
        {
            var clientMock = new Mock<IConfigCatClient>();
            clientMock
                .Setup(c => c.GetAllKeysAsync())
                .ReturnsAsync(new[] { "foo__bar" });
            clientMock
                .Setup(c => c.GetValueAsync("foo__bar", It.IsAny<string?>(), It.IsAny<User>()))
                .ReturnsAsync("dummy");

            var options = new ConfigCatConfigurationProviderOptions
            {
                Configuration = c =>
                {
                    c.SdkKey = "fake";
                },
                CreateClient = _ => clientMock.Object
            };

            using var sut = new ConfigCatConfigurationProvider(options);

            sut.Load();

            Assert.True(sut.TryGet("foo:bar", out var value));
            Assert.Equal("dummy", value);
        }

        [Fact]
        public void CustomMapperIsApplied()
        {
            var clientMock = new Mock<IConfigCatClient>();
            clientMock
                .Setup(c => c.GetAllKeysAsync())
                .ReturnsAsync(new[] { "foo§§bar" });
            clientMock
                .Setup(c => c.GetValueAsync("foo§§bar", It.IsAny<string?>(), It.IsAny<User>()))
                .ReturnsAsync("dummy");

            var options = new ConfigCatConfigurationProviderOptions
            {
                Configuration = c =>
                {
                    c.SdkKey = "fake";
                },
                KeyMapper = (key, value) => key.Replace("§§", ConfigurationPath.KeyDelimiter, StringComparison.InvariantCultureIgnoreCase),
                CreateClient = _ => clientMock.Object
            };

            using var sut = new ConfigCatConfigurationProvider(options);

            sut.Load();

            Assert.True(sut.TryGet("foo:bar", out var value));
            Assert.Equal("dummy", value);
        }

        [Fact]
        public void ConfigCatRefreshTriggersReload()
        {
            var clientMock = new Mock<IConfigCatClient>();
            clientMock
                .Setup(c => c.GetAllKeysAsync())
                .ReturnsAsync(new[] { "foo__bar" });
            clientMock
                .Setup(c => c.GetValueAsync("foo__bar", It.IsAny<string?>(), It.IsAny<User>()))
                .ReturnsAsync("dummy");

            AutoPollConfiguration? config = null;

            var options = new ConfigCatConfigurationProviderOptions
            {
                Configuration = c =>
                {
                    config = c;
                    c.SdkKey = "fake";
                },
                CreateClient = _ => clientMock.Object
            };

            using var sut = new ConfigCatConfigurationProvider(options);

            Assert.NotNull(config);

            sut.Load();

            clientMock
                .Setup(c => c.GetValueAsync("foo__bar", It.IsAny<string?>(), It.IsAny<User>()))
                .ReturnsAsync("dummy2");

            // This is very risky
            var raiseConfigurationChangedMethod = config!
                .GetType()
                .GetMethod("RaiseOnConfigurationChanged", BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.NotNull(raiseConfigurationChangedMethod);

            raiseConfigurationChangedMethod!.Invoke(config, new object[]
            {
                clientMock.Object,
                OnConfigurationChangedEventArgs.Empty
            });

            Assert.True(sut.TryGet("foo:bar", out var value));
            Assert.Equal("dummy2", value);
        }

        [Fact]
        public void ValueDontChangeUponRefreshIfNotChangedUpstream()
        {
            var clientMock = new Mock<IConfigCatClient>();
            clientMock
                .Setup(c => c.GetAllKeysAsync())
                .ReturnsAsync(new[] { "foo__bar" });
            clientMock
                .Setup(c => c.GetValueAsync("foo__bar", It.IsAny<string?>(), It.IsAny<User>()))
                .ReturnsAsync("dummy");

            AutoPollConfiguration? config = null;

            var options = new ConfigCatConfigurationProviderOptions
            {
                Configuration = c =>
                {
                    config = c;
                    c.SdkKey = "fake";
                },
                CreateClient = _ => clientMock.Object
            };

            using var sut = new ConfigCatConfigurationProvider(options);

            Assert.NotNull(config);

            sut.Load();

            // This is very risky
            var raiseConfigurationChangedMethod = config!
                .GetType()
                .GetMethod("RaiseOnConfigurationChanged", BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.NotNull(raiseConfigurationChangedMethod);

            raiseConfigurationChangedMethod!.Invoke(config, new object[]
            {
                clientMock.Object,
                OnConfigurationChangedEventArgs.Empty
            });

            Assert.True(sut.TryGet("foo:bar", out var value));
            Assert.Equal("dummy", value);
        }

        [Fact]
        public void FilteredOutKeysAreNotFetched()
        {
            var clientMock = new Mock<IConfigCatClient>();
            clientMock
                .Setup(c => c.GetAllKeysAsync())
                .ReturnsAsync(new[] { "foo__bar", "foo__baz" });
            clientMock
                .Setup(c => c.GetValueAsync("foo__bar", It.IsAny<string?>(), It.IsAny<User>()))
                .ReturnsAsync("dummy")
                .Verifiable();

            clientMock
                .Setup(c => c.GetValueAsync("foo__baz", It.IsAny<string?>(), It.IsAny<User>()))
                .ReturnsAsync("dummy")
                .Verifiable();

            var options = new ConfigCatConfigurationProviderOptions
            {
                Configuration = c =>
                {
                    c.SdkKey = "fake";
                },
                KeyFilter = k => k.Contains("bar", StringComparison.InvariantCultureIgnoreCase),
                CreateClient = _ => clientMock.Object
            };

            using var sut = new ConfigCatConfigurationProvider(options);

            sut.Load();

            clientMock
                .Verify(c => c.GetValueAsync("foo__bar", It.IsAny<string?>(), It.IsAny<User>()), Times.Once);

            clientMock
                .Verify(c => c.GetValueAsync("foo__baz", It.IsAny<string?>(), It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public void FilteredOutKeysAreNotStored()
        {
            var clientMock = new Mock<IConfigCatClient>();
            clientMock
                .Setup(c => c.GetAllKeysAsync())
                .ReturnsAsync(new[] { "foo__bar", "foo__baz" });
            clientMock
                .Setup(c => c.GetValueAsync("foo__bar", It.IsAny<string?>(), It.IsAny<User>()))
                .ReturnsAsync("dummy");

            clientMock
                .Setup(c => c.GetValueAsync("foo__baz", It.IsAny<string?>(), It.IsAny<User>()))
                .ReturnsAsync("dummy2");

            var options = new ConfigCatConfigurationProviderOptions
            {
                Configuration = c =>
                {
                    c.SdkKey = "fake";
                },
                KeyFilter = k => k.Contains("bar", StringComparison.InvariantCultureIgnoreCase),
                CreateClient = _ => clientMock.Object
            };

            using var sut = new ConfigCatConfigurationProvider(options);

            sut.Load();

            Assert.True(sut.TryGet("Foo:Bar", out var fooBarValue));
            Assert.Equal("dummy", fooBarValue);
            Assert.False(sut.TryGet("Foo:Baz", out var fooBazValue));
        }

        [Fact]
        public void AllKeysAreFetchedIfNoCustomFilterIsSet()
        {
            var clientMock = new Mock<IConfigCatClient>();
            clientMock
                .Setup(c => c.GetAllKeysAsync())
                .ReturnsAsync(new[] { "foo__bar", "foo__baz" });
            clientMock
                .Setup(c => c.GetValueAsync("foo__bar", It.IsAny<string?>(), It.IsAny<User>()))
                .ReturnsAsync("dummy")
                .Verifiable();

            clientMock
                .Setup(c => c.GetValueAsync("foo__baz", It.IsAny<string?>(), It.IsAny<User>()))
                .ReturnsAsync("dummy")
                .Verifiable();

            var options = new ConfigCatConfigurationProviderOptions
            {
                Configuration = c =>
                {
                    c.SdkKey = "fake";
                },
                CreateClient = _ => clientMock.Object
            };

            using var sut = new ConfigCatConfigurationProvider(options);

            sut.Load();

            clientMock
                .Verify(c => c.GetValueAsync("foo__bar", It.IsAny<string?>(), It.IsAny<User>()), Times.Once);

            clientMock
                .Verify(c => c.GetValueAsync("foo__baz", It.IsAny<string?>(), It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public void NullValuesAreIgnored()
        {
            var clientMock = new Mock<IConfigCatClient>();
            clientMock
                .Setup(c => c.GetAllKeysAsync())
                .ReturnsAsync(new[] { "foo__bar", "foo__baz" });
            clientMock
                .Setup(c => c.GetValueAsync("foo__bar", It.IsAny<string?>(), It.IsAny<User>()))
                .ReturnsAsync((string?)null!);

            var options = new ConfigCatConfigurationProviderOptions
            {
                Configuration = c =>
                {
                    c.SdkKey = "fake";
                },
                CreateClient = _ => clientMock.Object
            };

            using var sut = new ConfigCatConfigurationProvider(options);

            sut.Load();

            Assert.False(sut.TryGet("Foo:Bar", out var _));
        }
    }
}
