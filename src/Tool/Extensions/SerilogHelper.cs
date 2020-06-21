using Serilog.Events;

namespace PackProject.Tool.Extensions
{
    public static class SerilogHelper
    {
        public static LogEventLevel GetLevel(string verbosity)
        {
            if (verbosity == null)
                return LogEventLevel.Warning;

            return verbosity.ToLowerInvariant() switch
            {
                "q" => LogEventLevel.Error,
                "quiet" => LogEventLevel.Error,
                "m" => LogEventLevel.Warning,
                "minimal" => LogEventLevel.Warning,
                "n" => LogEventLevel.Information,
                "normal" => LogEventLevel.Information,
                "d" => LogEventLevel.Debug,
                "detailed" => LogEventLevel.Debug,
                "diag" => LogEventLevel.Debug,
                "diagnostic" => LogEventLevel.Debug,
                _ => LogEventLevel.Warning
            };
        }
    }
}