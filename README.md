![Build status](https://github.com/musement/configcat-configuration-provider/actions/workflow/ci.yml/badge.svg)

This repository contains a provider for [Microsoft.Extensions.Configuration](https://www.nuget.org/packages/Microsoft.Extensions.Configuration/) that maps configuration stored in [ConfigCat](https://configcat.com/)

## Overview

Storing configuration and feature flags in environment variables or
`appsettings.json` may require a new deploy of the application to update
something in some cases. This package extends the default configuration
pipeline used in ASP.NET applications (or any other application that uses
Microsoft.Extensions.Configuration really) to take advantage of remote
configuration via ConfigCat.

The repo contains a sample application, but this is how an application will
look like:

```csharp
public class Program
{
    public static Task Main(string[] args)
    {
        Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostingContext, builder) =>
            {
                builder.AddConfigCat(configCatOptions =>
                {
                    configCatOptions.Configuration = c =>
                    {
                        c.SdkKey = YOUR_SDK_KEY;
                    };
                })
            })
            .Build()
            .Run();
    }
}
```

We recommend storing the SDK key in a secure location, like AWS SecretsManager
or Azure KeyVault.

## Configuration

You can use the lambda parameter in the `AddConfigCat` extension method to
customize the behavior of the application.

The first obvious part is the `Configuration` property, which is then passed
to the ConfigCat SDK internally. This is an `AutoPollConfiguration` because
the library will automatically update the configuration on a timer, which you
can configure from this object. You can get more info about this object in the
[official ConfigCat SDK docs](https://github.com/configcat/.net-sdk/).

The options object also lets you set a filter (`KeyFilter` ) for the keys you
want to import, this is just a lambda function that returns `true` or `false`
respectively if you want to import a configuration key or not.
The default always returns `true`.

The `KeyMapper` lambda lets you customize how the ConfigCat key is transformed
into a IConfiguration key. The default works like how environment variables
work with IConfiguration, with double underscores (`__`) being transformed in
section delimiters so that `FOO__BAR` gets mapped as `FOO:BAR`.

Last but not least, the `CreateClient` lambda gives you an opportunity to
override how the ConfigCat client is created.
The `Configuration` object from above is passed as the only parameter.

## Versioning

This library follows [Semantic Versioning](http://semver.org/spec/v2.0.0.html)
for the public releases.

## Releases
Packages are automatically built and published on nuget by a Github Actions
workflow triggered by tags.

## How to build

This project uses the default `dotnet` CLI.
