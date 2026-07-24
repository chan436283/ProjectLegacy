using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CWFramework;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

public class CScriptableObject : ScriptableObject
{
    [FoldoutGroup("#String", 2)]
    public string Name;

    public virtual void OnLoadData()
    {
#if UNITY_EDITOR
        Validate();
#endif
    }

    //-------------------------------------------------------------------------------------------------------------------

    [FoldoutGroup("#Button", true, 3)]
    [Button("갱신 및 저장", ButtonSizes.Large)]
    public virtual void Refresh()
    {
#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
#endif
    }

    public virtual bool RefreshWithoutSave()
    {
// #if UNITY_EDITOR
//         if (_hasChangedWhiteRefreshAll || GameDefine.SCRIPTABLE_OBJECT_FORCE_REFRESH_ALL)
//         {
//             EditorUtility.SetDirty(this);
//         }
// #endif

        return _hasChangedWhiteRefreshAll;
    }

    protected bool _hasChangedWhiteRefreshAll = false;

    protected void UpdateIfChanged<TEnum>(ref string target, TEnum newValue) where TEnum : Enum
    {
        string newString = newValue?.ToString();
        if (target != newString)
        {
            target = newString;
            _hasChangedWhiteRefreshAll = true;
        }
    }

    protected void UpdateIfChangedArray(ref string[] target, string[] newArray)
    {
        try
        {
            // 두 배열이 모두 null이면 업데이트할 필요가 없습니다.
            if (target == null && newArray == null)
            {
                return;
            }

            // 두 배열이 모두 null이 아니고 값이 동일하면 업데이트하지 않습니다.
            if (target != null && newArray != null && target.SequenceEqual(newArray))
            {
                return;
            }

            // 배열이 변경되었으므로 newArray로 업데이트하고 플래그를 설정합니다.
            target = newArray;
            _hasChangedWhiteRefreshAll = true;
        }
        catch (System.Exception ex)
        {
            Debug.Log(ex);
            return;
        }
    }

    [FoldoutGroup("#Button", true, 4)]
    [Button("에셋 이름 자동 설정", ButtonSizes.Large)]
    public virtual void Rename()
    {
        // 필요에 따라 기본 Rename 동작 정의 (현재는 빈 구현)
    }

    public void Rename(string title)
    {
#if UNITY_EDITOR
        string newName = $"{title}_{Name}";
        PerformRename(newName);
#endif
    }

    public void Rename(string title, string desc)
    {
#if UNITY_EDITOR
        string newName = $"{title}_{Name}_{desc}";
        PerformRename(newName);
#endif
    }

#if UNITY_EDITOR

    /// <summary>
    /// 에셋의 경로를 확인하고, 현재 파일 이름과 다르면 에셋 이름을 변경합니다.
    /// </summary>
    /// <param name="newName">새 에셋 이름 (확장자 제외)</param>
    protected void PerformRename(string newName)
    {
        string assetPath = UnityEditor.AssetDatabase.GetAssetPath(this);
        string currentFileName = Path.GetFileName(assetPath);
        string newFileName = newName + ".asset";

        if (newFileName != currentFileName)
        {
            UnityEditor.AssetDatabase.RenameAsset(assetPath, newName);
        }
    }

#endif

    //-------------------------------------------------------------------------------------------------------------------

    private void OnValidate()
    {
        Validate();
    }

    public virtual void Validate()
    {
    }

    //-------------------------------------------------------------------------------------------------------------------

    protected void OnRefreshAll()
    {
#if UNITY_EDITOR
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
#endif
    }

    //-------------------------------------------------------------------------------------------------------------------

#if UNITY_EDITOR

    /// <summary>
    /// 부모 폴더가 지정되지 않은 경우 기본 경로를 사용하여 에셋을 생성합니다.
    /// </summary>
    protected T CreateAsset<T>(string assetType, string assetName, bool isCreateAsset) where T : CScriptableObject
    {
        return CreateAssetInternal<T>(null, assetType, assetName, isCreateAsset);
    }

    /// <summary>
    /// 부모 폴더가 지정된 경우 해당 폴더 경로를 사용하여 에셋을 생성합니다.
    /// </summary>
    protected T CreateAsset<T>(string parentFolder, string assetType, string assetName, bool isCreateAsset) where T : CScriptableObject
    {
        return CreateAssetInternal<T>(parentFolder, assetType, assetName, isCreateAsset);
    }

    /// <summary>
    /// 에셋 생성의 공통 로직을 담당하는 내부 메서드입니다.
    /// </summary>
    private T CreateAssetInternal<T>(string parentFolder, string assetType, string assetName, bool isCreateAsset) where T : CScriptableObject
    {
        if (isCreateAsset)
        {
            // 부모 폴더가 없으면 기본 경로 사용, 있으면 해당 경로 사용
            string assetPath = string.IsNullOrEmpty(parentFolder)
                ? $"Assets/Resources/Scriptable/{assetType}/{assetType}_{assetName}.asset"
                : $"Assets/Resources/Scriptable/{parentFolder}/{assetType}/{assetType}_{assetName}.asset";

            T asset = ScriptableObject.CreateInstance<T>();
            UnityEditor.AssetDatabase.CreateAsset(asset, assetPath);
            UnityEditor.AssetDatabase.ImportAsset(assetPath);

            return asset;
        }
        else
        {
            return null;
        }
    }

#endif

    protected T CreateAssetInSameFolder<T>(string assetType, string assetName, bool isCreateAsset) where T : CScriptableObject
    {
        if (isCreateAsset)
        {
#if UNITY_EDITOR
            // 호출한 스크립터블 오브젝트의 에셋 경로를 가져옵니다.
            string callerAssetPath = UnityEditor.AssetDatabase.GetAssetPath(this);
            string targetFolder;

            if (string.IsNullOrEmpty(callerAssetPath))
            {
                // 만약 호출하는 에셋의 경로를 찾지 못했다면 기본 경로를 사용합니다.
                targetFolder = $"Assets/Resources/Scriptable/{assetType}";
            }
            else
            {
                // 에셋의 경로에서 폴더 경로만 추출합니다.
                targetFolder = Path.GetDirectoryName(callerAssetPath);
            }

            // 새 에셋의 경로를 targetFolder 내에 생성합니다.
            string assetPath = Path.Combine(targetFolder, $"{assetType}_{assetName}.asset");
            // Path.Combine은 OS에 따라 역슬래시를 사용할 수 있으므로 Unity 에셋 경로 형식에 맞게 슬래시로 변환합니다.
            assetPath = assetPath.Replace("\\", "/");

            // T 타입의 인스턴스를 생성하고 에셋으로 등록합니다.
            T asset = ScriptableObject.CreateInstance<T>();
            UnityEditor.AssetDatabase.CreateAsset(asset, assetPath);
            UnityEditor.AssetDatabase.ImportAsset(assetPath);

            return asset;
#endif
        }

        return null;
    }

    #region Color Utility

    /// <summary>
    /// 일반 값(T)이 기본 값(default)과 동일하면 DarkGray, 그렇지 않으면 GreenYellow를 반환합니다.
    /// (bool, int, enum 등 대부분의 타입에 적용 가능)
    /// </summary>
    protected Color GetFieldColor<T>(T value)
    {
        if (EqualityComparer<T>.Default.Equals(value, default(T)))
        {
            return CWColors.DarkGray;
        }
        else
        {
            return CWColors.GreenYellow;
        }
    }

    /// <summary>
    /// float 타입의 경우 IsZero 확장 메서드를 사용해 값이 0에 가까운지 판별합니다.
    /// </summary>
    protected Color GetFieldColor(float value)
    {
        if (value.IsZero())
        {
            return CWColors.DarkGray;
        }
        else
        {
            return CWColors.GreenYellow;
        }
    }

    #endregion Color Utility

    #region Inspector Color Methods
    protected Color GetBoolFieldColor(bool value)
    {
        return GetFieldColor(value);
    }

    protected Color GetFloatFieldColor(float value)
    {
        return GetFieldColor(value);
    }

    protected Color GetIntFieldColor(int value)
    {
        return GetFieldColor(value);
    }

    #endregion Inspector Color Methods
}

public static class XScriptableObjectValidator
{
    public static bool IsValid(this CScriptableObject scriptableObject)
    {
        if (scriptableObject == null)
        {
            return false;
        }

        if (string.IsNullOrEmpty(scriptableObject.Name))
        {
            return false;
        }

        return true;
    }
}
