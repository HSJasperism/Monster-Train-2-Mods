using BepInEx.Logging;
using System;

namespace CardChanges
{
    public static class Logging
    {
        private static string _Name = CardChanges.GUID;
        public static string Name
        {
            get => _Name;
            set
            {
                Logger.Sources.Remove(_LogSource);
                _LogSource = null;
                _Name = value;
            }
        }

        private static ManualLogSource _LogSource;
        public static ManualLogSource LogSource
        {
            get
            {
                if (_LogSource is null)
                {
                    _LogSource = new ManualLogSource(Name);
                    Logger.Sources.Add(_LogSource);
                }
                return _LogSource;
            }
        }

        public static void Log(LogLevel level, string message) => LogSource.Log(level, message);
        public static void Log(string message, LogLevel level = LogLevel.Debug) => Log(level, message);
        public static void Log(Exception error) => Log(LogLevel.Error, $"{error.Source}\n{error.Message}");

        public static void LogInfo(string info) => Log(LogLevel.Info, info);

        public static void LogWarning(string warning) => Log(LogLevel.Warning, warning);

        public static void LogError(Exception error) => Log(error);
        public static void LogError(string error) => Log(LogLevel.Error, error);
    }
}
