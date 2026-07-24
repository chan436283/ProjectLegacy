using CWFramework;
using UnityEngine;

public class SingletonMonoBehaviour<T> : CBehaviour where T : CBehaviour
{
    private static T _instance;
    private static readonly object _lock = new();
    private static bool _applicationIsQuitting = false;

    public static T Instance
    {
        get
        {
            if (_applicationIsQuitting)
            {
                LogWarningApplicationQuit();
                return null;
            }

            lock (_lock)
            {
                if (_instance != null)
                {
                    return _instance;
                }

                _instance = Object.FindFirstObjectByType<T>();

                if (_instance != null)
                {
                    if (Object.FindObjectsByType<T>(FindObjectsSortMode.None).Length > 1)
                    {
                        LogWarningMultipleInstances();
                    }
                }
                else
                {
                    CreateInstance();
                }

                return _instance;
            }
        }
    }

    public static void CreateInstance()
    {
        if (_instance != null)
        {
            LogUsingExistingInstance();
            return;
        }

        GameObject singletonGO = new($"@{typeof(T)}");
        _instance = singletonGO.AddComponent<T>();
        DontDestroyOnLoad(singletonGO);

        LogCreateSingleton(singletonGO);
    }

    protected override void OnAwake()
    {
        base.OnAwake();

        if (_instance == null)
        {
            _instance = this as T;
            DontDestroyOnLoad(gameObject);

            //Log.Progress($"싱글톤을 설정합니다: {this.GetHierarchyName()}");
        }
        else if (_instance != this)
        {
            Log.Warning($"중복된 싱글톤 인스턴스가 감지되어 파괴됩니다: {this.GetHierarchyName()}");
            Destroy(gameObject);
        }
    }

    protected override void OnApplicationQuitting()
    {
        base.OnApplicationQuitting();
        _applicationIsQuitting = true;
    }

    #region Logging

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    private static void LogCreateSingleton(GameObject go)
    {
        Log.Progress($"[Singleton] {go.GetHierarchyPath()} 생성됨. DontDestroyOnLoad 적용.");
    }

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    private static void LogUsingExistingInstance()
    {
        Log.Progress($"[Singleton] 이미 생성된 인스턴스를 사용합니다: {_instance.GetHierarchyPath()}");
    }

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    private static void LogWarningApplicationQuit()
    {
        Log.Warning($"[Singleton] 애플리케이션 종료 상태이므로 싱글톤 인스턴스를 반환하지 않습니다. Type: {typeof(T)}");
    }

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    private static void LogWarningMultipleInstances()
    {
        Log.Warning($"[Singleton] 여러 개의 인스턴스가 존재합니다. 싱글톤은 하나만 존재해야 합니다. Type: {typeof(T)}");
    }

    #endregion Logging
}
