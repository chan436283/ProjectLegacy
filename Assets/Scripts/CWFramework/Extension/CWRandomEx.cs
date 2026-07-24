using UnityEngine;

namespace CWFramework
{
    public static class CWRandomEx
    {
        private static readonly System.Random random;

        static CWRandomEx()
        {
            int seed = Application.version.GetHashCode(); // 빌드 버전을 시드로 사용
            random = new System.Random(seed);
        }

        public static bool GetBoolValue()
        {
            return random.Next(0, 2) == 1;
        }

        public static float GetFloatValue()
        {
            // 0 이상 1 미만
            return (float)random.NextDouble();
        }

        #region Vector

        public static Vector2 GetVector2Value(Vector2 size)
        {
            float x = size.x.IsZero() ? 0 : Range(-size.x, size.x);
            float y = size.y.IsZero() ? 0 : Range(-size.y, size.y);

            return new Vector2(x, y);
        }

        public static Vector3 GetVector3Value(Vector2 size)
        {
            float x = size.x.IsZero() ? 0 : Range(-size.x, size.x);
            float y = size.y.IsZero() ? 0 : Range(-size.y, size.y);

            return new Vector3(x, y, 0);
        }

        #endregion Vector

        public static int Range(int min, int max)
        {
            if (min >= max)
            {
                return min;
            }
            else
            {
                return random.Next(min, max);
            }
        }

        public static float Range(float min, float max)
        {
            // 범위가 유효하지 않거나 0인 경우 최소값만 반환합니다.
            // 위의 정수 오버로드의 동작을 반영합니다.
            if (min >= max || Mathf.Approximately(min, max))
            {
                return min;
            }
            else
            {
                float randomValue = GetFloatValue();
                return (randomValue * (max - min)) + min;
            }
        }
    }
}