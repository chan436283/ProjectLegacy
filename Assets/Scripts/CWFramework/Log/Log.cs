using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace CWFramework
{
    public class Log
    {
        public static bool LevelProgress = true;
        public static bool LevelInfo = true;
        public static bool LevelWarning = true;
        public static bool LevelError = true;
        public static bool LevelExcept = true;
        public static bool LevelAsync = false;

        private static LogSystem logSystem = null;

        public static void Initialize()
        {
            if (logSystem == null)
            {
#if UNITY_EDITOR
                logSystem = new LogSystem("Editor", useFileLog: false);
#elif DEVELOPMENT_BUILD || TS_DEVELOPMENT_BUILD
                    logSystem = new LogSystem("Development", useFileLog: true);
#endif
            }

            logSystem?.Initialize();
        }

        private static bool IsInitialized => logSystem != null;

        private static void SafeLog(string tag, string content, Color? color)
        {
            if (!IsInitialized || logSystem == null)
            {
#if UNITY_EDITOR
                Initialize();
#else
                return;
#endif
            }

            logSystem.Log(tag, content, color);
        }

        private static Color? GetColor(Level logLevel)
        {
#if !UNITY_EDITOR
            return null;
#endif

            return logLevel switch
            {
                Level.Progress => CWColors.DarkOliveGreen,
                Level.Warning => CWColors.ActivateYellow,
                Level.Error => CWColors.CherryRed,
                Level.Except => CWColors.RoyalBlue,
                _ => null,
            };
        }

        private static readonly Dictionary<Level, Func<bool>> LevelChecks = new()
        {
            { Level.Progress, () => LevelProgress },
            { Level.Info, () => LevelInfo },
            { Level.Warning, () => LevelWarning },
            { Level.Error, () => LevelError },
            { Level.Except, () => LevelExcept }
        };

        private static bool IsLogLevelEnabled(Level level)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD || TS_DEVELOPMENT_BUILD
            return LevelChecks.TryGetValue(level, out var check) && check();
#else
            return false;
#endif
        }

        private static void ValidateArgs(string format, params object[] args)
        {
            if (string.IsNullOrEmpty(format))
            {
                throw new ArgumentException("format is null or empty. Format must be a valid string.");
            }

            if (args == null || args.Length == 0)
            {
                throw new ArgumentException($"args is null or empty. Format: {format}");
            }
        }

        private static string FormatMessage(string format, params object[] args)
        {
            StringBuilder sb = new StringBuilder();
            try
            {
                sb.AppendFormat(format, args);
            }
            catch (Exception e)
            {
                Log.Error($"{format}, {args.Length}, {args.GetString()}\n{e.ToString()}");
            }
            return sb.ToString();
        }

        #region 

        private static readonly object logLock = new();

        public static async Task LogAsync(string tag, string content, Color? color)
        {
            await Task.Run(() =>
            {
                LogSync(tag, content, color);
            });
        }

        private static void LogSync(string tag, string content, Color? color)
        {
            lock (logLock)
            {
                SafeLog(tag, content, color);
            }
        }

        #endregion

        private static async void LogMessage(string tag, string format, Level level, params object[] args)
        {
            if (false == IsLogLevelEnabled(level)) return; 

            ValidateArgs(format, args);
            string content = FormatMessage(format, args);
            if (LevelAsync)
            {
                await LogAsync(tag, content, GetColor(level));
            }
            else
            {
                LogSync(tag, content, GetColor(level));
            }
        }

        private static async void LogWithTag(LogTags tag, string format, Level level, params object[] args)
        {
            if (false == IsLogLevelEnabled(level)) return;
            if (false == CWScriptableDataManager.Instance.FindLog(tag)) return;

            ValidateArgs(format, args);
            string content = FormatMessage(format, args);
            if (LevelAsync)
            {
                await LogAsync(tag.ToString(), content, GetColor(level));
            }
            else
            {
                LogSync(tag.ToString(), content, GetColor(level));
            }
        }

        #region

        public static void Progress(string content)
        {
            if (false == IsLogLevelEnabled(Level.Progress)) return;

            SafeLog(LogLevels.Progress, content, color: GetColor(Level.Progress));
        }

        public static void Progress(string format, params object[] args)
        {
            LogMessage(LogLevels.Progress, format, Level.Progress, args);
        }

        public static void Progress(LogTags tag, string content)
        {
            if (false == IsLogLevelEnabled(Level.Progress)) return;
            if (false == CWScriptableDataManager.Instance.FindLog(tag)) return;

            SafeLog(tag.ToString(), content, color: GetColor(Level.Progress));
        }

        public static void Progress(LogTags tag, string format, params object[] args)
        {
            LogWithTag(tag, format, Level.Progress, args);
        }

        public static void Info(string content)
        {
            if (false == IsLogLevelEnabled(Level.Info)) return;

            SafeLog(LogLevels.Info, content, color: GetColor(Level.Info));
        }

        public static void Info(string format, params object[] args)
        {
            LogMessage(LogLevels.Info, format, Level.Info, args);
        }

        public static void Info(LogTags tag, string content)
        {
            if (false == IsLogLevelEnabled(Level.Info)) return;
            if (false == CWScriptableDataManager.Instance.FindLog(tag)) return;

            SafeLog(tag.ToString(), content, color: GetColor(Level.Info));
        }

        public static void Info(LogTags tag, string format, params object[] args)
        {
            LogWithTag(tag, format, Level.Info, args);
        }

        public static void Warning(string content)
        {
            if (false == IsLogLevelEnabled(Level.Warning)) return;

#if UNITY_EDITOR
            UnityEngine.Debug.LogWarningFormat("[Warning] {0}", content);
#else
            SafeLog(LogLevels.Warning, content, color: GetColor(Level.Warning));
#endif
        }

        public static void Warning(string format, params object[] args)
        {
            LogMessage(LogLevels.Warning, format, Level.Warning, args);
        }

        public static void Warning(LogTags tag, string content)
        {
            if (false == IsLogLevelEnabled(Level.Warning)) return;
            if (false == CWScriptableDataManager.Instance.FindLog(tag)) return;

#if UNITY_EDITOR
            Debug.LogWarningFormat("[{0}] {1}", tag, content);
#else
            SafeLog(tag.ToString(), content, color: GetColor(Level.Warning));
#endif
        }

        public static void Warning(LogTags tag, string format, params object[] args)
        {
            LogWithTag(tag, format, Level.Warning, args);
        }

        public static void Error(string content)
        {
            if (false == IsLogLevelEnabled(Level.Error)) return;

            SafeLog(LogLevels.Error, content, color: GetColor(Level.Error));
#if UNITY_EDITOR
            Debug.LogErrorFormat(content);
#endif
        }

        public static void Error(string format, params object[] args)
        {
            LogMessage(LogLevels.Error, format, Level.Error, args);
        }

        public static void Error(LogTags tag, string content)
        {
            if (false == IsLogLevelEnabled(Level.Error)) return;
            if (false == CWScriptableDataManager.Instance.FindLog(tag)) return;

            SafeLog(tag.ToString(), content, color: GetColor(Level.Error));
#if UNITY_EDITOR
            Debug.LogErrorFormat(content);
#endif
        }

        public static void Error(LogTags tag, string format, params object[] args)
        {
            LogWithTag(tag, format, Level.Error, args);
        }

        public static void Except(string content)
        {
            if (false == IsLogLevelEnabled(Level.Except)) return;

            SafeLog(LogLevels.Except, content, color: GetColor(Level.Except));
        }

        public static void Except(string format, params object[] args)
        {
            LogMessage(LogLevels.Except, format, Level.Except, args);
        }

        public static void Except(LogTags tag, string content)
        {
            if (false == IsLogLevelEnabled(Level.Except)) return;
            if (false == CWScriptableDataManager.Instance.FindLog(tag)) return;

            SafeLog(tag.ToString(), content, color: GetColor(Level.Except));
        }

        public static void Except(LogTags tag, string format, params object[] args)
        {
            LogWithTag(tag, format, Level.Except, args);
        }

        #endregion

        #region

        public enum Level
        {
            Off,
            Error,
            Warning,
            Info,
            Progress,
            Except,
        }

        private static class LogLevels
        {
            public const string Progress = "Progress";
            public const string Info = "Info";
            public const string Warning = "Warning";
            public const string Error = "Error";
            public const string Except = "Except";
        }

        public static void LoadLevel()
        {
#if UNITY_EDITOR
            LevelProgress = GamePrefs.GetBoolOrDefault(GamePrefTypes.LOG_LEVEL_PROCESS, true);
            LevelInfo = GamePrefs.GetBoolOrDefault(GamePrefTypes.LOG_LEVEL_INFO, true);
            LevelWarning = GamePrefs.GetBoolOrDefault(GamePrefTypes.LOG_LEVEL_WARNING, true);
            LevelError = GamePrefs.GetBoolOrDefault(GamePrefTypes.LOG_LEVEL_ERROR, true);
            LevelExcept = GamePrefs.GetBoolOrDefault(GamePrefTypes.LOG_LEVEL_EXCEPT, true);
            LevelAsync = GamePrefs.GetBoolOrDefault(GamePrefTypes.LOG_LEVEL_ASYNC, false);

#elif DEVELOPMENT_BUILD || TS_DEVELOPMENT_BUILD
            LevelProgress = GamePrefs.GetBoolOrDefault(GamePrefTypes.LOG_LEVEL_PROCESS, false);
            LevelInfo = GamePrefs.GetBoolOrDefault(GamePrefTypes.LOG_LEVEL_INFO, false);
            LevelWarning = GamePrefs.GetBoolOrDefault(GamePrefTypes.LOG_LEVEL_WARNING, true);
            LevelError = GamePrefs.GetBoolOrDefault(GamePrefTypes.LOG_LEVEL_ERROR, true);
            LevelExcept = GamePrefs.GetBoolOrDefault(GamePrefTypes.LOG_LEVEL_EXCEPT, true);
            LevelAsync = GamePrefs.GetBoolOrDefault(GamePrefTypes.LOG_LEVEL_ASYNC, false);

#else
            LevelProgress = GamePrefs.GetBoolOrDefault(GamePrefTypes.LOG_LEVEL_PROCESS, false);
            LevelInfo = GamePrefs.GetBoolOrDefault(GamePrefTypes.LOG_LEVEL_INFO, false);
            LevelWarning = GamePrefs.GetBoolOrDefault(GamePrefTypes.LOG_LEVEL_WARNING, false);
            LevelError = GamePrefs.GetBoolOrDefault(GamePrefTypes.LOG_LEVEL_ERROR, false);
            LevelExcept = GamePrefs.GetBoolOrDefault(GamePrefTypes.LOG_LEVEL_EXCEPT, false);
            LevelAsync = GamePrefs.GetBoolOrDefault(GamePrefTypes.LOG_LEVEL_ASYNC, false);

#endif
            Debug.LogFormat("�α� ������ �ҷ��ɴϴ�: Progress:{0}, Info:{1}, Warning:{2}, Error:{3}, Except:{4}, Async:{5}",
                                LevelProgress,
                                LevelInfo,
                                LevelWarning,
                                LevelError,
                                LevelExcept,
                                LevelAsync);
        }

        public static void SetLogLevelOff()
        {
            LevelProgress = false;
            LevelInfo = false;
            LevelWarning = false;
            LevelError = false;
            LevelExcept = false;

            SaveLogLevelSettings();
        }

        public static void SetLogLevelAll()
        {
            LevelProgress = true;
            LevelInfo = true;
            LevelWarning = true;
            LevelError = true;
            LevelExcept = true;

            SaveLogLevelSettings();
        }

        private static void SaveLogLevelSettings()
        {
            GamePrefs.SetBool(GamePrefTypes.LOG_LEVEL_PROCESS, LevelProgress);
            GamePrefs.SetBool(GamePrefTypes.LOG_LEVEL_INFO, LevelInfo);
            GamePrefs.SetBool(GamePrefTypes.LOG_LEVEL_WARNING, LevelWarning);
            GamePrefs.SetBool(GamePrefTypes.LOG_LEVEL_ERROR, LevelError);
            GamePrefs.SetBool(GamePrefTypes.LOG_LEVEL_EXCEPT, LevelExcept);

            Debug.Log("Log Levels Saved.");
        }

        //

        public static void SwitchLogLevelProgress()
        {
            LevelProgress = !LevelProgress;

            SetLogLevel(GamePrefTypes.LOG_LEVEL_PROCESS, LevelProgress);
        }

        public static void SwitchLogLevelInfo()
        {
            LevelInfo = !LevelInfo;

            SetLogLevel(GamePrefTypes.LOG_LEVEL_INFO, LevelInfo);
        }

        public static void SwitchLogLevelWarning()
        {
            LevelWarning = !LevelWarning;

            SetLogLevel(GamePrefTypes.LOG_LEVEL_WARNING, LevelWarning);
        }

        public static void SwitchLogLevelError()
        {
            LevelError = !LevelError;

            SetLogLevel(GamePrefTypes.LOG_LEVEL_ERROR, LevelError);
        }

        public static void SwitchLogLevelExcept()
        {
            LevelExcept = !LevelExcept;

            SetLogLevel(GamePrefTypes.LOG_LEVEL_EXCEPT, LevelExcept);
        }

        public static void SwitchLogLevelAsync()
        {
            LevelAsync = !LevelAsync;

            SetLogLevel(GamePrefTypes.LOG_LEVEL_ASYNC, LevelAsync);
        }

        private static void SetLogLevel(GamePrefTypes type, bool value)
        {
            GamePrefs.SetBool(type, value);

#if UNITY_EDITOR
            Debug.Log($"�α� ����({type})�� �����մϴ�: {value.ToBoolString()}");
#endif
        }

        #endregion �α� ����
    }
}