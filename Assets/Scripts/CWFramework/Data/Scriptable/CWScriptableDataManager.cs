using UnityEngine;

namespace CWFramework
{
    /// <summary> ������Ʈ���� ������ Scriptable Object�� �����մϴ�. </summary>
    public class CWScriptableDataManager : Singleton<CWScriptableDataManager>
    {
        private LogSettingAsset _logSetting;

        public virtual void Clear()
        {
            _logSetting = null;
        }

        public virtual bool CheckLoaded()
        {
            if (_logSetting == default) { return false; }

            return true;
        }

        public virtual void Load()
        {
            LoadLogSetting();
        }

        public virtual void OnLoadData()
        {
            _logSetting?.OnLoadData();
        }

        public bool LoadLogSetting()
        {
            string filePath = PathManager.FindAssetPath("LogSetting");
            if (string.IsNullOrEmpty(filePath))
            {
                return false;
            }

            LogSettingAsset asset = Resources.Load<LogSettingAsset>(filePath);
            if (asset != null)
            {
                _logSetting = asset;

#if !UNITY_EDITOR
                asset.ExternSwitchOffAll();
#endif
                return true;
            }

            return false;
        }

        public bool FindLog(LogTags tag)
        {
            if (_logSetting != null)
            {
                return _logSetting.Find(tag);
            }

            return false;
        }

        public LogSettingAsset GetLogSetting()
        {
            return _logSetting;
        }

        public bool CheckLogWithLoadString()
        {
            if (_logSetting != null)
            {
                return _logSetting.LoadString;
            }

            return false;
        }
    }
}