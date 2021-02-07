using LuminCommon.Defaults;
using LuminCommon.Properties;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LuminCommon.Configurator
{
    public class ConfigureService : BackgroundService, IConfigureService
    {
        private readonly FileInfo configFile;
        private readonly ILuminConfiguration config;
        private readonly ILogger<ConfigureService> logger;

        /// <summary>Initializes a new instance of the <see cref="ConfigureService" /> class.</summary>
        /// <param name="logger">The logger.</param>
        public ConfigureService(ILuminConfiguration luminConfig, ILogger<ConfigureService> logger)
        {
            this.logger = logger;
            config = luminConfig;
            logger.LogInformation("Loading Configuration File from: {0} ...", DefaultValues.UnixSavePath);
            configFile = new FileInfo(DefaultValues.UnixSavePath);
        }

        /// <summary>Reads and sets the lumin configuration.</summary>
        public async Task ReadLuminConfig()
        {
            if (!configFile.Exists)
            {
                logger.LogWarning(Resources.ConfigurationFileDoestNotExist, DefaultValues.UnixSavePath);
                return;
            }

            FileStream fileStream = new FileStream(configFile.FullName, FileMode.Open);
            Dictionary<string, string> configs = new Dictionary<string, string>();

            using (StreamReader reader = new StreamReader(fileStream))
            {
                string line;
                while ((line = await reader.ReadLineAsync().ConfigureAwait(false)) != null)
                {
                    line = line.Trim();

                    //Skip Comments
                    if (line.StartsWith("#"))
                        continue;

                    //Check Syntax contains an =
                    if (!line.Contains("="))
                    {
                        logger.LogWarning(Resources.CannotReadConfigurationLine, line);
                        continue;
                    }

                    var input = line.Split("=");

                    //Check value ob both sides of = is provided
                    if (input.Length != 2 || input.Where(s => s == null || s.Trim() == string.Empty).ToList().Count > 0)
                    {
                        logger.LogWarning(Resources.CannotReadConfigurationLine, line);
                        continue;
                    }

                    configs.Add(input[0].Trim(), input[1].Trim());
                }
            }

            foreach (var prop in typeof(LuminConfigs).GetProperties())
            {
                if (configs.ContainsKey(prop.Name))
                {
                    var configValue = configs[prop.Name];
                    if (prop != null && prop.CanWrite)
                    {
                        try
                        {
                            prop.SetValue(config, Convert.ChangeType(configValue, prop.PropertyType), null);
                            logger.LogDebug("Set Configuration Value: {0} = {1} ...", prop.Name, configValue);
                        }
                        catch (Exception ex)
                        {
                            logger.LogWarning("Cannot set Configuration Value: {0} to {1}. Using Default Value: {2}. Error Message: {3} ...",
                                prop.Name,
                                configValue,
                                prop.GetValue(config),
                                ex.Message);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// This method is called when the <see cref="T:Microsoft.Extensions.Hosting.IHostedService" /> starts. The implementation should return a task that represents
        /// the lifetime of the long running operation(s) being performed.
        /// </summary>
        /// <param name="stoppingToken">Triggered when <see cref="M:Microsoft.Extensions.Hosting.IHostedService.StopAsync(System.Threading.CancellationToken)" /> is called.</param>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await ReadLuminConfig().ConfigureAwait(false);      
        }
    }
}
