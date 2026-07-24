using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace CWFramework
{
    public class LogSystem : IDisposable
    {
        private string category;
        private bool useFileLog;
        private bool isInitialized = false;

        private StreamWriter file;
        protected int logCountToWrite = 0;

        private const long MAX_FILE_SIZE = 10 * 1024 * 1024; // 10MB
        private int _fileIndex = 0;
        private string _logPathBase;

        public LogSystem(string category, bool useFileLog)
        {
            this.category = category;
            this.useFileLog = useFileLog;
        }

        public void Initialize()
        {
            if (isInitialized) return;

            if (useFileLog)
            {
                try
                {
                    _fileIndex = 0;
                    _logPathBase = string.Format("{0}/{1}_{2}", Application.persistentDataPath, category, System.Diagnostics.Process.GetCurrentProcess().Id); 
                    
                    string logPath = GetIndexedLogPath();
                    Log("FileLog", "File Path:" + logPath, color: Color.green);
                    file = new StreamWriter(logPath, append: false, encoding: new UTF8Encoding(false))
                    {
                        AutoFlush = true
                    };
                }
                catch (FileNotFoundException e)
                {
                    Debug.LogError(e.Message);
                }
            }

            isInitialized = true;
        }

        public void Dispose()
        {
            CleanUp();
        }

        public void CleanUp()
        {
            if (null != file)
            {
                file.Close();
                file.Dispose();
                file = null;
            }

            logCountToWrite = 0;
        }

        private string ColorToString(Color color)
        {
            if (Color.black.Equals(color))
            {
                return "black";
            }
            else if (Color.blue.Equals(color))
            {
                return "blue";
            }
            else if (Color.cyan.Equals(color))
            {
                return "cyan";
            }
            else if (Color.grey.Equals(color))
            {
                return "grey";
            }
            else if (Color.green.Equals(color))
            {
                return "green";
            }
            else if (Color.magenta.Equals(color))
            {
                return "magenta";
            }
            else if (Color.red.Equals(color))
            {
                return "red";
            }
            else if (Color.yellow.Equals(color))
            {
                return "yellow";
            }
            else if (Color.white.Equals(color))
            {
                return "white";
            }
            else
            {
                return CWColorEx.ColorToHex(color);
            }
        }

        public void Log(string tag, string text, Color? color = null)
        {
            StringBuilder stringBuilder = new StringBuilder();

#if UNITY_EDITOR || DEVELOPMENT_BUILD || TS_DEVELOPMENT_BUILD || BETA_BUILD
            FileLog(tag, text);

            if (color.HasValue)
            {
                string colorString = ColorToString(color.Value);
                if (!string.IsNullOrEmpty(colorString))
                {
                    stringBuilder.Append("<color=");
                    stringBuilder.Append(colorString);
                    stringBuilder.Append(">(");
                    stringBuilder.Append(tag);
                    stringBuilder.Append(") ");
                    stringBuilder.Append(text);
                    stringBuilder.AppendLine("</color>");

                    try
                    {
                        UnityConsoleOutput(stringBuilder.ToString());
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(ex.ToString());
                    }
                    return;
                }
            }
#endif
            stringBuilder.Append("(");
            stringBuilder.Append(tag);
            stringBuilder.Append(") ");
            stringBuilder.AppendLine(text);
            try
            {
                UnityConsoleOutput(stringBuilder.ToString());
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.ToString());
            }
        }

        private readonly object _fileLock = new object();

        public void FileLog(string tag, string text)
        {
            if (!isInitialized)
            {
                Debug.LogWarning("LogSystem.Initialize()가 호출되지 않았습니다.");
                return;
            }

            if (file == null) return;

            string content = string.Empty;

            lock (_fileLock)
            {
                logCountToWrite += 1;

                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append($"({category}/{tag})({DateTime.Now.ToLongTimeString()}) ");
                stringBuilder.Append(text);

                try
                {
                    content = RemoveColorTag(stringBuilder.ToString());

                    if (string.IsNullOrEmpty(content))
                    {
                        Debug.LogWarning("FileLog: content is null or empty.");
                        return;
                    }

                    // 파일 크기 체크 및 롤링
                    if (file.BaseStream.Length > MAX_FILE_SIZE)
                    {
                        file.Flush();
                        file.Close();
                        file.Dispose();
                        _fileIndex++;
                        file = new StreamWriter(GetIndexedLogPath(), append: false, encoding: new UTF8Encoding(false))
                        {
                            AutoFlush = true
                        };
                        Log("FileLog", $"New Log File Created: {GetIndexedLogPath()}", color: Color.yellow);
                    }

                    file.WriteLine(content);

                    if (logCountToWrite >= 200)
                    {
                        file.Flush();
                        logCountToWrite = 0;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"FileLog Exception - content: {content}");
                    Debug.LogException(e);
                }
            }
        }

        private string GetIndexedLogPath()
        {
            return $"{_logPathBase}_{_fileIndex}.txt";
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        private void UnityConsoleOutput(string log)
        {
            Debug.Log(log);
        }

        public string RemoveColorTag(string input)
        {
            // <color=...>~</color> 태그 제거 (색상 형식 무관)
            string pattern = "<color=.*?>(.*?)</color>";
            return Regex.Replace(input, pattern, "$1");
        }
    }
}