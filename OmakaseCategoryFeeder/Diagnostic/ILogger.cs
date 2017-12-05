using System;

namespace Signia.OmakaseCategoryFeeder.Diagnostic
{
    public interface ILogger
    {
        void Log(string loggerName, LogLevels severity, string message);
        void LogException(string loggerName, Exception ex);
        void LogException(string loggerName, Exception ex, string message);
        void Log(LogLevels severity, string message);
        void LogException(Exception ex);
        void LogException(Exception ex, LogLevels severity);
        void LogException(Exception ex, LogLevels severity, string message);
        void LogException(Exception ex, string message);
        void PerformanceTrace(string processName, ProcessSteps step);
        void PerformanceTrace(string processName, string step);
        void PerformanceTrace(string processName, string step, string additionalInfo);
    }
}
