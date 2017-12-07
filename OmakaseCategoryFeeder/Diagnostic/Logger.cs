using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NLog.Config;
using NLog.Targets;
using NLogger = NLog.Logger;
using NLogLevel = NLog.LogLevel;
using NLogManager = NLog.LogManager;

namespace Signia.OmakaseCategoryFeeder.Diagnostic
{
    //NLog implementation of logging interface
    public class Logger : ILogger
    {
        private static ILogger _defaultLogger;
        //defult logger (to have static reference to logger)
        public static ILogger DefaultLogger => _defaultLogger ?? (_defaultLogger = new Logger(GetDefaultLoggerConfig()));


        public static LoggerConfig GetDefaultLoggerConfig()
        {
            string appVersion;
            try
            {
                appVersion = typeof (Logger).Assembly.GetName().Version.ToString();
            }
            catch
            {
                appVersion = new Version(0, 0, 0, 0).ToString();
            }
            return new LoggerConfig
            {
                AppName = "OmakaseFeeder",
                OperationMode = ConfigurationSource.Offline,
                AppVersion = appVersion
            };
        }

        private NLogger _logger;

        public Logger(LoggerConfig cfg)
        {
            switch (cfg.OperationMode)
            {
                case ConfigurationSource.Offline: InitializeOfflineLogger(cfg.AppName, cfg.AppVersion); break;
                case ConfigurationSource.Online: InitializeOnlineLogger(cfg.AppName, cfg.AppVersion, cfg.ConnStr); break;
                default: throw new NotImplementedException(cfg.OperationMode.ToString());
            }
        }

        /// <summary>
        /// non-database logger (just console & file)
        /// </summary>
        private void InitializeOfflineLogger(string appName, string appVersion)
        {
            _logger = InitializeOfflineLogger($"{appName}_{appVersion}", NLogLevel.Info, NLogLevel.Info);
            if (_defaultLogger == null)
                _defaultLogger = this;
        }

        private NLogger InitializeOfflineLogger(string loggerName, NLogLevel consoleLogLevel, NLogLevel fileLogLevel)
        {
            var config = NLogManager.Configuration ?? new LoggingConfiguration();
            var newRegistrationMsg = new StringBuilder();
#if DEBUG
            var target = config.FindTargetByName("debugConsoleTarget");
            if (target == null)
            {
                target = new ConsoleTarget
                {
                    Name = "debugConsoleTarget",
                    Layout =
                        "${longdate} | ${logger} | ${level}   ${message} ${exception:format=Message, Type, StackTrace:separator=\r\n}"
                };
                // LogLayout.DEFAULT_LAYOUT;
                config.AddTarget("debugConsoleTarget", target);
                newRegistrationMsg.AppendLine($"New file target {target.Name} created");
            }
            var isRuleRegistered =
                config.LoggingRules.Any(lr => lr.LoggerNamePattern == loggerName && lr.Targets.Any(t => t == target));
            if (!isRuleRegistered)
            {
                var debugRule = new LoggingRule(loggerName, consoleLogLevel, target);
                config.LoggingRules.Add(debugRule);
                newRegistrationMsg.AppendLine($"New logging rule with params: {loggerName}|{consoleLogLevel}|{target.Name} added to configuration");
            }

            target = config.FindTargetByName("debugFileTarget");
            if (target == null)
            {
                target = new FileTarget
                {
                    Name = "debugFileTarget",
                    FileName = $"{loggerName}_{DateTime.Now.ToString("yyyy-MM-dd")}.txt",
                    Layout =
                        "${longdate} | ${logger} | ${level}   ${message} ${exception:format=Message, Type, StackTrace:separator=\r\n}"
                };
                // LogLayout.DEFAULT_LAYOUT;
                config.AddTarget("debugFileTarget", target);
                newRegistrationMsg.AppendLine($"New file target {target.Name} created");
            }
            isRuleRegistered =
                config.LoggingRules.Any(lr => lr.LoggerNamePattern == loggerName && lr.Targets.Any(t => t == target));
            if (!isRuleRegistered)
            {
                var debugRule = new LoggingRule(loggerName, fileLogLevel, target);
                config.LoggingRules.Add(debugRule);
                newRegistrationMsg.AppendLine($"New logging rule with params: {loggerName}|{fileLogLevel}|{target.Name} added to configuration");
            }
#endif

            NLogManager.Configuration = config;
            var logger = NLogManager.GetLogger(loggerName);
            if (newRegistrationMsg.Length > 0)
                logger.Info($"New logger registrations: {newRegistrationMsg}");

            return logger;
        }

        private void InitializeOnlineLogger(string appName, string appVersion, string connStr)
        {
            var loggerName = $"SigniaReactor_{appName}_{appVersion}";

            var target = new DatabaseTarget
            {
                ConnectionString = connStr,
                Name = loggerName,
                CommandText = "insert into [LogTable]([time_stamp], [level], [machinename], [processid], [processname], [message], [exception_info])"
                            + " values(@time_stamp, @level, @machinename, @processid, @processname, @message, @exception_info);"
            };

            target.Parameters.Add(
                new DatabaseParameterInfo
                {
                    Name = "@time_stamp",
                    Layout = "${date}"
                });
            target.Parameters.Add(
                new DatabaseParameterInfo
                {
                    Name = "@level",
                    Layout = "${level}"
                });
            target.Parameters.Add(
                new DatabaseParameterInfo
                {
                    Name = "@machinename",
                    Layout = "${machinename}"
                });
            target.Parameters.Add(
                new DatabaseParameterInfo
                {
                    Name = "@processid",
                    Layout = "${processid}"
                });
            target.Parameters.Add(
                new DatabaseParameterInfo
                {
                    Name = "@processname",
                    Layout = "${processname}"
                });
            target.Parameters.Add(
                new DatabaseParameterInfo
                {
                    Name = "@message",
                    Layout = "${message}"
                });
            //moze stacktrace tylko w opisie wyjatku...
            //target.Parameters.Add(
            //    new DatabaseParameterInfo
            //    {
            //        Name = "@stacktrace",
            //        Layout = "${stacktrace}"
            //    });
            target.Parameters.Add(
                new DatabaseParameterInfo
                {
                    Name = "@exception_info",
                    Layout = "${exception:format=Message, Type, StackTrace:separator=\r\n}"
                });



#if DEBUG
            LogLevels severity = LogLevels.Trace;
#else
            LogLevels severity = LogLevels.Info;
#endif

            var debugRule = new LoggingRule(loggerName, NLogLevel.FromOrdinal((int)severity), target);

            var config = NLogManager.Configuration ?? new LoggingConfiguration();
            config.AddTarget("debugDB", target);
            config.LoggingRules.Add(debugRule);

            NLogManager.Configuration = config;
            _logger = NLogManager.GetLogger(loggerName);

            _logger.Trace("Online (Database) Logger initialized");

            _defaultLogger = this;
        }

        private static readonly ReaderWriterLockSlim InternalLock = new ReaderWriterLockSlim();
        private static readonly List<string> ConfiguredLoggers = new List<string>();
        private NLogger GetLogger(string loggerName, NLogLevel consoleLogLevel, NLogLevel fileLogLevel)
        {
            try
            {
                InternalLock.EnterUpgradeableReadLock();
                if (ConfiguredLoggers.Any(ln => ln == loggerName))
                    return NLogManager.GetLogger(loggerName);

                try
                {
                    InternalLock.EnterWriteLock();
                    var logger = InitializeOfflineLogger(loggerName, consoleLogLevel, fileLogLevel);
                    ConfiguredLoggers.Add(loggerName);
                    return logger;
                }
                finally
                {
                    if (InternalLock.IsWriteLockHeld)
                        InternalLock.ExitWriteLock();
                }
            }
            finally
            {
                if (InternalLock.IsUpgradeableReadLockHeld)
                    InternalLock.ExitUpgradeableReadLock();
            }
        }
        public void LogException(Exception ex, LogLevels severity, string message)
            => _logger?.Log(NLogLevel.FromOrdinal((int)severity), ex, message);

        public void LogException(Exception ex, string message)
            => _logger?.Log(NLogLevel.Fatal, ex, message);

        public void LogException(Exception ex)
            => _logger?.Log(NLogLevel.Fatal, ex);

        public void LogException(Exception ex, LogLevels severity)
            => _logger?.Log(NLogLevel.FromOrdinal((int)severity), ex);

        public void Log(LogLevels severity, string message)
            => _logger?.Log(NLogLevel.FromOrdinal((int)severity), message);

        public void PerformanceTrace(string processName, string step)
            => PerformanceTrace(processName, step, string.Empty);

        public void PerformanceTrace(string processName, string step, string additionalInfo)
            => Log(LogLevels.Trace, $"[{processName};{step}] {additionalInfo}");

        public void PerformanceTrace(string processName, ProcessSteps step)
            => Log(LogLevels.Trace, $"[{processName};{(step == ProcessSteps.Start ? "START" : "END")}]");

        //fsn-1138
        public void Log(string loggerName, LogLevels severity, string message)
        {
            var logger = GetLogger(loggerName, NLogLevel.Trace, NLogLevel.Trace);
            logger?.Log(NLogLevel.FromOrdinal((int)severity), message);
        }

        //fsn-1138
        public void LogException(string loggerName, Exception ex)
        {
            var logger = GetLogger(loggerName, NLogLevel.Trace, NLogLevel.Trace);
            logger?.Log(NLogLevel.Fatal, ex);
        }

        //fsn-1138
        public void LogException(string loggerName, Exception ex, string message)
        {
            var logger = GetLogger(loggerName, NLogLevel.Trace, NLogLevel.Trace);
            logger?.Log(NLogLevel.Fatal, ex, message);
        }

    }
}
