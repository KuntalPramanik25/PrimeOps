namespace PrimeOps.LOGGER.Entities
{
    public enum LogAssembly
    {
        API,
        BLL,
        DAL,
        JOBS,
        PYTHON,
        COMMON
    }

    public enum LogLevel
    {
        Info,
        Warning,
        Error
    }

    public enum DiagnosticInfoLevel
    {
        Never,
        ErrorOnly,
        Always
    }
}
