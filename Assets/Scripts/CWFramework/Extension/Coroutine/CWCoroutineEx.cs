using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace CWFramework
{
    public static class CWCoroutineEx
    {
        public static IEnumerator NextFrame(UnityAction callback)
        {
            yield return null;

            if (callback != null)
            {
                callback();
            }
        }

        public static IEnumerator NextTimer(float duration, UnityAction callback)
        {
            if (false == Mathf.Approximately(duration, 0f))
                yield return new WaitForSeconds(duration);

            if (callback != null)
            {
                callback();
            }
        }

        public static IEnumerator NextRealTimer(float duration, UnityAction callback)
        {
            if (false == Mathf.Approximately(duration, 0f))
                yield return new WaitForSecondsRealtime(duration);

            if (callback != null)
            {
                callback();
            }
        }
    }
}