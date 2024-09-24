namespace uav.logic.Configuration;

using Microsoft.Extensions.Configuration;
using log4net;
using System.IO;
using System;

public static class Config
{
  private static IConfigurationRoot? _configuration;
  private static readonly ILog logger = LogManager.GetLogger(typeof(Config));

  public static void LoadConfigs<T>(string env) where T : class
  {
    logger.Info($"Loading configs for {env}");

    var builder = new ConfigurationBuilder()
      .SetBasePath(Directory.GetCurrentDirectory())
//      .AddJsonFile("appsettings.json", reloadOnChange: true, optional: false)
//      .AddJsonFile($"appsettings.{env}.json", reloadOnChange: true, optional: true)
      .AddUserSecrets<T>();

    _configuration = builder
      .Build();
  }

  public static string GetConfig(string key)
  {
    return _configuration?[key] ?? throw new Exception($"Config key {key} not found");
  }

  public static int GetConfigInt(string key)
  {
    return _configuration?.GetValue<int>(key) ?? throw new Exception($"Config key {key} not found");
  }

  public static string? GetConfigWithDefault(string key, string? defaultValue = null)
  {
    return _configuration?[key] ?? defaultValue;
  }
}