using System.Linq;
using UnityEngine;

namespace CWFramework
{
    public enum GamePrefTypes
    {
        // Log
        LOG_LEVEL_PROCESS,
        LOG_LEVEL_INFO,
        LOG_LEVEL_WARNING,
        LOG_LEVEL_ERROR,
        LOG_LEVEL_EXCEPT,
        LOG_LEVEL_ASYNC,

        // Option-Language
        OPTION_LANGUAGE,

        // Option-Audio
        OPTION_MASTER_VOLUME,
        OPTION_MUSIC_VOLUME,
        OPTION_AMBIENCE_VOLUME,
        OPTION_SFX_VOLUME,
        OPTION_MUTE_MUSIC,
        OPTION_MUTE_SFX,
        OPTION_STOP_HAPTIC,

        // Option-Video
        OPTION_USE_FULLSCREEN,
        OPTION_USE_BORDERLESS,
        OPTION_USE_VSYNC,
        OPTION_USE_PARTICLE_EFFECT,
        OPTION_USE_FRAME_RATE_CAP,
        OPTION_RESOLUTION_X,
        OPTION_RESOLUTION_Y,
        OPTION_RESOLUTION_REFRESH_RATE,

        // Option-Play
        OPTION_CAMERA_SHAKE,
        OPTION_VIBRATION,
        OPTION_SHOW_MONSTER_LIFE_TEXT,

        OPTION_SHOW_ITEM_OPTION_RANGE,
        OPTION_SHOW_ITEM_OPTION_COMPARE,

        OPTION_USE_TUTORIAL,
        OPTION_USE_MONSTER_GAUGE,
        OPTION_USE_DAMAGE_TEXT,
        OPTION_USE_STATE_EFFECT_TEXT,

        // Option-Play (Developer)
        OPTION_SHOW_MONSTER_ATTACK_COOLDOWN,
        OPTION_SHOW_INVULNERABLE_RENDERER,
        OPTION_SHOW_ALL_BUFF_ICONS,
        OPTION_SHOW_STATUS_CALCULATIONS,
        OPTION_IS_HIDE_USERINTERFACE,
        OPTION_IS_HIDE_NPC_MARKER,

        // Option-Control

        KEYBOARD_MOVEUP,
        KEYBOARD_MOVEDOWN,
        KEYBOARD_MOVELEFT,
        KEYBOARD_MOVERIGHT,
        KEYBOARD_JUMP,
        KEYBOARD_ATTACK,
        KEYBOARD_SUBATTACK,
        KEYBOARD_CAST1,
        KEYBOARD_CAST2,
        KEYBOARD_CAST3,
        KEYBOARD_CAST4,
        KEYBOARD_POTION1,
        KEYBOARD_INTERACT,
        KEYBOARD_ORDERINTERACT,
        KEYBOARD_POPUPSKILL,
        KEYBOARD_POPUPINVENTORY,
        KEYBOARD_POPUPITEM,
        KEYBOARD_COMPARE,
        KEYBOARD_SYNERGY,
        KEYBOARD_KEYBINDING,
        KEYBOARD_WORLDDIFFICULTY,

        // Cheat
        GAME_CHEAT_INFINITY_DAMAGE,
        GAME_CHEAT_PERCENT_DAMAGE,
        GAME_CHEAT_ONE_DAMAGE_ATTACK,

        GAME_CHEAT_CRITICAL_TYPE,
        GAME_CHEAT_PASSIVE_TRIGGER_CHANCE_TYPE,

        GAME_CHEAT_NOT_COST_RESOURCE,
        GAME_CHEAT_NO_COOLDOWN_TIME,
        GAME_CHEAT_RECEIVE_DAMAGE_ONLY_ONE,
        GAME_CHEAT_NOT_DEAD,
        GAME_CHEAT_NOT_CROWD_CONTROL,
        GAME_CHEAT_DONT_DROP_ITEM,
        GAME_CHEAT_FULL_POTIONS,
        GAME_CHEAT_ITEM_OPTION_MAX_STAT,
        GAME_CHEAT_CUSTOM_RELIC_GRADE,
        GAME_CHEAT_MONSTER_SKILL_INDEX,

        // Stage
        VISIBLE_CAMERA_COLLISION_CULLING_MASK,

        // Early Access
        EARLY_ACCESS_ENTER_NOTICE,

        // Blacksmith
        BLACKSMITH_ITEM_ENHANCE,
        BLACKSMITH_ITEM_TRANSCEND,
    }

    public static class GamePrefs
    {
        private static string GAME_NAME
        {
            get
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD || TS_DEVELOPMENT_BUILD || BETA_BUILD
                return "DEV_DRAGON_IS_DEAD_";
#else
                return "DRAGON_IS_DEAD_";
#endif
            }
        }

        public static bool HasKey(string type)
        {
            string key = GAME_NAME + type.ToUpperString();
            if (false == string.IsNullOrEmpty(key))
            {
                return PlayerPrefs.HasKey(key);
            }

            return false;
        }

        public static bool HasKey(GamePrefTypes type)
        {
            string key = GAME_NAME + type.ToUpperString();
            if (false == string.IsNullOrEmpty(key))
            {
                return PlayerPrefs.HasKey(key);
            }

            return false;
        }

        public static bool GetBool(GamePrefTypes type)
        {
            string key = GAME_NAME + type.ToUpperString();

            if (false == string.IsNullOrEmpty(key))
            {
                if (PlayerPrefs.HasKey(key))
                {
                    return PlayerPrefs.GetInt(key) == 1;
                }
            }

            return false;
        }

        public static bool GetBoolOrDefault(GamePrefTypes type, bool defaultValue)
        {
            string key = GAME_NAME + type.ToUpperString();

            if (false == string.IsNullOrEmpty(key))
            {
                if (PlayerPrefs.HasKey(key))
                {
                    return PlayerPrefs.GetInt(key) == 1;
                }
                else
                {
                    return defaultValue;
                }
            }

            return false;
        }

        public static int GetInt(GamePrefTypes type, int defaultValue = 0)
        {
            string key = GAME_NAME + type.ToUpperString();

            if (false == string.IsNullOrEmpty(key))
            {
                if (PlayerPrefs.HasKey(key))
                {
                    return PlayerPrefs.GetInt(key);
                }
            }

            return defaultValue;
        }

        public static float GetFloat(GamePrefTypes type)
        {
            string key = GAME_NAME + type.ToUpperString();

            if (false == string.IsNullOrEmpty(key))
            {
                if (PlayerPrefs.HasKey(key))
                {
                    return PlayerPrefs.GetFloat(key);
                }
            }

            return 0;
        }

        public static string GetString(GamePrefTypes type)
        {
            string key = GAME_NAME + type.ToUpperString();

            if (false == string.IsNullOrEmpty(key))
            {
                if (PlayerPrefs.HasKey(key))
                {
                    return PlayerPrefs.GetString(key);
                }
            }

            return string.Empty;
        }

        public static string GetString(string type)
        {
            string key = GAME_NAME + type.ToUpperString();

            if (false == string.IsNullOrEmpty(key))
            {
                if (PlayerPrefs.HasKey(key))
                {
                    return PlayerPrefs.GetString(key);
                }
            }

            return string.Empty;
        }

        public static void SetString(string type, string value)
        {
            string key = GAME_NAME + type.ToUpperString();

            if (false == string.IsNullOrEmpty(key))
            {
                Log.Info(LogTags.GamePref, $"Set String. key:({key}), value:({value}).");

                PlayerPrefs.SetString(key, value);
                PlayerPrefs.Save();
            }
        }

        public static void SetString(GamePrefTypes type, string value)
        {
            string key = GAME_NAME + type.ToUpperString();

            if (false == string.IsNullOrEmpty(key))
            {
                Log.Info(LogTags.GamePref, $"Set String. key:({key}), value:({value}).");

                PlayerPrefs.SetString(key, value);
                PlayerPrefs.Save();
            }
        }

        public static void SetBool(GamePrefTypes type, bool value)
        {
            string key = GAME_NAME + type.ToUpperString();

            if (false == string.IsNullOrEmpty(key))
            {
                Log.Info(LogTags.GamePref, $"Set Int. key:({key}), value:({value.ToBoolString()}).");

                int valueToInt = value ? 1 : 0;
                if (PlayerPrefs.GetInt(key) != valueToInt)
                {
                    PlayerPrefs.SetInt(key, valueToInt);
                    PlayerPrefs.Save();
                }
            }
        }

        public static void SetInt(GamePrefTypes type, int value)
        {
            string key = GAME_NAME + type.ToUpperString();

            if (false == string.IsNullOrEmpty(key))
            {
                Log.Info(LogTags.GamePref, $"Set Int. key:({key}), value:({value}).");

                PlayerPrefs.SetInt(key, value);
                PlayerPrefs.Save();
            }
        }

        public static void SetFloat(GamePrefTypes type, float value)
        {
            string key = GAME_NAME + type.ToUpperString();

            if (false == string.IsNullOrEmpty(key))
            {
                Log.Info(LogTags.GamePref, $"Set Float. key:({key}), value:({value}).");

                PlayerPrefs.SetFloat(key, value);
                PlayerPrefs.Save();
            }
        }

        //
        public static void ClearOnEntryPoint()
        {
            Delete(GamePrefTypes.EARLY_ACCESS_ENTER_NOTICE);
        }

        public static void Clear()
        {
            GamePrefTypes[] types = CWEnumEx.GetValues<GamePrefTypes>();
            for (int i = 0; i < types.Length; i++)
            {
                Delete(types[i]);
            }

            PlayerPrefs.DeleteAll();
        }

        public static void Delete(GamePrefTypes type)
        {
            string key = GAME_NAME + type.ToUpperString();

            if (false == string.IsNullOrEmpty(key))
            {
                if (PlayerPrefs.HasKey(key))
                {
                    PlayerPrefs.DeleteKey(key);
                }
            }
        }

        public static void Delete(string type)
        {
            string key = GAME_NAME + type.ToUpperString();

            if (false == string.IsNullOrEmpty(key))
            {
                if (PlayerPrefs.HasKey(key))
                {
                    PlayerPrefs.DeleteKey(key);
                }
            }
        }
    }
}