using System;

namespace CWFramework
{
    /// <summary>
    /// 무한 루프 검사 및 방지(에디터 전용)
    /// </summary>
    public static class InfiniteLoopDetector
    {
        private static string prevPoint = "";
        private static int detectionCount = 0;
        private const int DetectionThreshold = 1000;

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void Run(
            [System.Runtime.CompilerServices.CallerMemberName] string mn = "",
            [System.Runtime.CompilerServices.CallerFilePath] string fp = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int ln = 0
        )
        {
            string currentPoint = $"{fp}:{ln}, {mn}()";

            if (prevPoint == currentPoint)
            {
                detectionCount += 1;
            }
            else
            {
                detectionCount = 0;
            }

            if (detectionCount > DetectionThreshold)
            {
                throw new Exception($"Infinite Loop Detected: \n{currentPoint}\n\n");
            }

            prevPoint = currentPoint;
        }

        /// <summary>
        /// 독립적인 작업의 시작 시 내부 상태를 초기화하기 위한 함수
        /// </summary>
        public static void Reset()
        {
            detectionCount = 0;
            prevPoint = "";
        }

#if UNITY_EDITOR

        [UnityEditor.InitializeOnLoadMethod]
        private static void Init()
        {
            UnityEditor.EditorApplication.update += () =>
            {
                detectionCount = 0;
            };
        }

#endif
    }
}