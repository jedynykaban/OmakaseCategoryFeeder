namespace Signia.OmakaseCategoryFeeder.Diagnostic
{
    public enum ConfigurationSource
    {
        NotSet = 0,
        Offline,
        Online,
    }

    public enum ProcessSteps
    {
        Start,
        End
    }

    public enum LogLevels
    {
        Trace = 0,
        Debug = 1,
        Info = 2,
        Warning = 3,
        Error = 4,
        Fatal = 5
    }
}
