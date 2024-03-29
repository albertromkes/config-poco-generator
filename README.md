### Source Generator for generating POCO classes based on your appsettings.json files ###

[![NuGet Badge](https://buildstats.info/nuget/configgenerator)](https://www.nuget.org/packages/configgenerator/)

This Source Generator generates POCO's based on your appsettings.json files. It also supports multiple settings files: appsettings.json + appsettings.Development.json + appsettings.Production.json, etc.

1) Add the appsettings.json files to your `.csproj` like this: 

```
  <ItemGroup>
    <AdditionalFiles Include="appsettings.json" />
    <AdditionalFiles Include="appsettings.Development.json" />
  </ItemGroup>
```

You can add as many appsettings files as you want :)

2) The config classes are generated in the namespace `ApplicationConfig` (This will be configurable in the next release).
3) Register your config in the DI container: `builder.Services.Configure<ApplicationConfig.MyAppConfig>(builder.Configuration);`
4) Inject the config in your constructor: `IOptions<ApplicationConfig.MyAppConfig> options`

:warning: **You  may have to restart Visual Studio after installing this NuGet package!**

See this blogpost for more information:

https://albertromkes.com/2020/11/04/a-source-generator-for-your-appsettings-json/
