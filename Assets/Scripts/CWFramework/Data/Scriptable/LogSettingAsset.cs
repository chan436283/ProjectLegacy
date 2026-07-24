using Sirenix.OdinInspector;
using UnityEngine;

#if UNITY_EDITOR

using UnityEditor;

#endif

namespace CWFramework
{
    [CreateAssetMenu(fileName = "LogSetting", menuName = "TeamSuneat/Scriptable/LogSetting")]
    public class LogSettingAsset : ScriptableObject
    {
        [SuffixLabel("애널리틱스")] public bool Analytics;

        [FoldoutGroup("[캐릭터]")][SuffixLabel("캐릭터")] public bool Character;
        [FoldoutGroup("[캐릭터]")][SuffixLabel("몬스터 캐릭터")] public bool Monster;
        [FoldoutGroup("[캐릭터]")][SuffixLabel("엘리트 캐릭터")] public bool Elite;
        [FoldoutGroup("[캐릭터]")][SuffixLabel("보스캐릭터")] public bool Boss;

        [FoldoutGroup("[캐릭터]")][SuffixLabel("캐릭터 생성")] public bool CharacterSpawn;
        [FoldoutGroup("[캐릭터]")][SuffixLabel("캐릭터 웨이브")] public bool CharacterWave;
        [FoldoutGroup("[캐릭터]")][SuffixLabel("캐릭터 능력")] public bool Ability;

        [FoldoutGroup("[캐릭터 물리]")][SuffixLabel("물리")] public bool Physics;
        [FoldoutGroup("[캐릭터 물리]")][SuffixLabel("강제이동")] public bool ForceVelocity;
        [FoldoutGroup("[캐릭터 물리]")][SuffixLabel("순간이동")] public bool Blink;

        [FoldoutGroup("[캐릭터 AI]")][SuffixLabel("AI")] public bool AI;
        [FoldoutGroup("[캐릭터 AI]")][SuffixLabel("AI 행동")] public bool AI_Action;
        [FoldoutGroup("[캐릭터 AI]")][SuffixLabel("AI 결정")] public bool AI_Decision;
        [FoldoutGroup("[캐릭터 AI]")][SuffixLabel("AI 상태전환")] public bool AI_Transition;

        [FoldoutGroup("[캐릭터 랜더러]")][SuffixLabel("애니메이션")] public bool Animation;
        [FoldoutGroup("[캐릭터 랜더러]")][SuffixLabel("랜더러")] public bool Renderer;

        [FoldoutGroup("[전투]")][SuffixLabel("공격")] public bool Attack;
        [FoldoutGroup("[전투]")][SuffixLabel("공격(목표 공격)")] public bool Attack_Target;
        [FoldoutGroup("[전투]")][SuffixLabel("공격(영역 공격)")] public bool Attack_Area;
        [FoldoutGroup("[전투]")][SuffixLabel("공격(발사체 생성)")] public bool Attack_Projectile;
        [FoldoutGroup("[전투]")][SuffixLabel("피해량 계산")] public bool Damage;
        [FoldoutGroup("[전투]")][SuffixLabel("패시브")] public bool Passive;
        [FoldoutGroup("[전투]")][SuffixLabel("패시브 발동")] public bool PassiveTrigger;
        [FoldoutGroup("[전투]")][SuffixLabel("버프")] public bool Buff;
        [FoldoutGroup("[전투]")][SuffixLabel("상태")] public bool State;
        [FoldoutGroup("[전투]")][SuffixLabel("능력치")] public bool Stat;
        [FoldoutGroup("[전투]")][SuffixLabel("전투자원")] public bool BattleResource;
        [FoldoutGroup("[전투]")][SuffixLabel("캐릭터 전투자원")] public bool Vital;
        [FoldoutGroup("[전투]")][SuffixLabel("발사체")] public bool Projectile;
        [FoldoutGroup("[전투]")][SuffixLabel("탐지")] public bool Detect;

        [FoldoutGroup("[기술]")][SuffixLabel("기술")] public bool Skill;
        [FoldoutGroup("[기술]")][SuffixLabel("기술 애니메이션")] public bool Skill_Animation;
        [FoldoutGroup("[기술]")][SuffixLabel("기술 버퍼")] public bool Skill_Buffer;
        [FoldoutGroup("[기술]")][SuffixLabel("기술 비용")] public bool Skill_Cost;
        [FoldoutGroup("[기술]")][SuffixLabel("기술 재사용 대기")] public bool Skill_Cooldown;
        [FoldoutGroup("[기술]")][SuffixLabel("기술 순서")] public bool Skill_Order;
        [FoldoutGroup("[기술]")][SuffixLabel("기술 시각 효과")] public bool Skill_VFX;

        [FoldoutGroup("[상호작용]")][SuffixLabel("상호작용")] public bool Interaciton;
        [FoldoutGroup("[상호작용]")][SuffixLabel("NPC")] public bool NPC;
        [FoldoutGroup("[상호작용]")][SuffixLabel("드랍 오브젝트")] public bool DropObject;
        [FoldoutGroup("[상호작용]")][SuffixLabel("포탈")] public bool Portal;
        [FoldoutGroup("[상호작용]")][SuffixLabel("영역 이벤트")] public bool AreaEvent;
        [FoldoutGroup("[상호작용]")][SuffixLabel("판매")] public bool Sell;

        [FoldoutGroup("[아이템]")][SuffixLabel("아이템")] public bool Item;
        [FoldoutGroup("[아이템]")][SuffixLabel("아이템 능력치")] public bool ItemStat;
        [FoldoutGroup("[아이템]")][SuffixLabel("아이템 드랍")] public bool ItemDrop;
        [FoldoutGroup("[아이템]")][SuffixLabel("아이템 착용")] public bool ItemEquip;
        [FoldoutGroup("[아이템]")][SuffixLabel("아이템 옵션")] public bool ItemOption;

        [FoldoutGroup("[아이템 종류]")][SuffixLabel("무기")] public bool Weapon;
        [FoldoutGroup("[아이템 종류]")][SuffixLabel("유물")] public bool Relic;
        [FoldoutGroup("[아이템 종류]")][SuffixLabel("물약")] public bool Potion;
        [FoldoutGroup("[아이템 종류]")][SuffixLabel("비약")] public bool Elixir;
        [FoldoutGroup("[아이템 종류]")][SuffixLabel("정수")] public bool Essence;
        [FoldoutGroup("[아이템 종류]")][SuffixLabel("요리 및 조리법")] public bool Food;
        [FoldoutGroup("[아이템 종류]")][SuffixLabel("영혼 보석")] public bool SoulGem;
        [FoldoutGroup("[아이템 종류]")][SuffixLabel("유물 조화 기도")] public bool Prayers;

        [FoldoutGroup("[해금]")][SuffixLabel("퀘스트")] public bool Quest;
        [FoldoutGroup("[해금]")][SuffixLabel("퀘스트 조건")] public bool QuestCondition;
        [FoldoutGroup("[해금]")][SuffixLabel("튜토리얼")] public bool Tutorial;
        [FoldoutGroup("[해금]")][SuffixLabel("난이도")] public bool Difficulty;

        [FoldoutGroup("[인게임]")][SuffixLabel("이펙트")] public bool Effect;
        [FoldoutGroup("[인게임]")][SuffixLabel("타임라인")] public bool Timeline;
        [FoldoutGroup("[인게임]")][SuffixLabel("포지션 그룹")] public bool PositionGroup;

        [FoldoutGroup("[데이터]")][SuffixLabel("데이터")] public bool Data;
        [FoldoutGroup("[데이터]")][SuffixLabel("Json 데이터")] public bool JsonData;
        [FoldoutGroup("[데이터]")][SuffixLabel("스크립터블 데이터")] public bool ScriptableData;
        [FoldoutGroup("[데이터]")][SuffixLabel("임시 데이터")] public bool GamePref;
        [FoldoutGroup("[데이터]")][SuffixLabel("리소스")] public bool Resource;

        [FoldoutGroup("[세이브 데이터]")][SuffixLabel("게임 데이터")] public bool GameData;
        [FoldoutGroup("[세이브 데이터]")][SuffixLabel("게임 데이터 - 기술")] public bool GameData_Skill;
        [FoldoutGroup("[세이브 데이터]")][SuffixLabel("게임 데이터 - 스테이지")] public bool GameData_Stage;
        [FoldoutGroup("[세이브 데이터]")][SuffixLabel("게임 데이터 - 유물")] public bool GameData_Relic;

        [FoldoutGroup("[세이브 데이터 - 인벤토리]")][SuffixLabel("재화")] public bool Currency;
        [FoldoutGroup("[세이브 데이터 - 인벤토리]")][SuffixLabel("소지품")] public bool Inventory;
        [FoldoutGroup("[세이브 데이터 - 인벤토리]")][SuffixLabel("인벤토리 이벤트 데이터")] public bool InventoryPopup_DataFlow;
        [FoldoutGroup("[세이브 데이터 - 인벤토리]")][SuffixLabel("인벤토리 슬롯 탐색")] public bool InventoryPopup_Search;
        [FoldoutGroup("[세이브 데이터 - 인벤토리]")][SuffixLabel("인벤토리 장착")] public bool InventoryPopup_Equip;
        [FoldoutGroup("[세이브 데이터 - 인벤토리]")][SuffixLabel("인벤토리 유물")] public bool InventoryPopup_Relic;
        [FoldoutGroup("[세이브 데이터 - 인벤토리]")][SuffixLabel("창고")] public bool Storage;

        [FoldoutGroup("[게임 설정]")][SuffixLabel("설정")] public bool Setting;
        [FoldoutGroup("[게임 설정]")][SuffixLabel("음향")] public bool Audio;
        [FoldoutGroup("[게임 설정]")][SuffixLabel("비디오")] public bool Video;
        [FoldoutGroup("[게임 설정]")][SuffixLabel("카메라")] public bool Camera;
        [FoldoutGroup("[게임 설정]")][SuffixLabel("입력")] public bool Input;
        [FoldoutGroup("[게임 설정]")][SuffixLabel("마우스 커서")] public bool Cursor;

        [FoldoutGroup("[지역]")][SuffixLabel("스테이지")] public bool Stage;
        [FoldoutGroup("[지역]")][SuffixLabel("스테이지 몬스터")] public bool Stage_Monster;
        [FoldoutGroup("[지역]")][SuffixLabel("스테이지 보상")] public bool Stage_Reward;

        [FoldoutGroup("[지역]")][SuffixLabel("타일")] public bool Tile;
        [FoldoutGroup("[지역]")][SuffixLabel("타일")] public bool Collision;
        [FoldoutGroup("[지역]")][SuffixLabel("월드맵 입력")] public bool WorldMapPopup_Input;
        [FoldoutGroup("[지역]")][SuffixLabel("월드맵 데이터")] public bool WorldMapPopup_Data;

        [FoldoutGroup("[글로벌]")][SuffixLabel("글로벌 이벤트")] public bool Global;
        [FoldoutGroup("[글로벌]")][SuffixLabel("스트링 텍스트")] public bool String;
        [FoldoutGroup("[글로벌]")][SuffixLabel("개발용")] public bool Develop;

        [FoldoutGroup("[UI]")] public bool UI;
        [FoldoutGroup("[UI]")][SuffixLabel("미니맵")] public bool UI_Minimap;
        [FoldoutGroup("[UI]")][SuffixLabel("버튼")] public bool UI_Button;
        [FoldoutGroup("[UI]")][SuffixLabel("게이지")] public bool UI_Gauge;
        [FoldoutGroup("[UI]")][SuffixLabel("인디케이터")] public bool UI_Indicator;
        [FoldoutGroup("[UI]")][SuffixLabel("팝업")] public bool UI_Popup;
        [FoldoutGroup("[UI]")][SuffixLabel("상세정보")] public bool UI_Details;
        [FoldoutGroup("[UI]")][SuffixLabel("소지품")] public bool UI_Inventory;
        [FoldoutGroup("[UI]")][SuffixLabel("유물")] public bool UI_Relic;
        [FoldoutGroup("[UI]")][SuffixLabel("대장장이")] public bool UI_Blacksmith;
        [FoldoutGroup("[UI]")][SuffixLabel("선택 이벤트")] public bool UI_SelectEvent;
        [FoldoutGroup("[UI]")][SuffixLabel("탭")] public bool UI_Tab;
        [FoldoutGroup("[UI]")][SuffixLabel("기술")] public bool UI_Skill;
        [FoldoutGroup("[UI]")][SuffixLabel("거래 - 데이터 경로")] public bool TradePopup_DataFlow;
        [FoldoutGroup("[UI]")][SuffixLabel("거래 - 슬롯 탐색")] public bool TradePopup_SearchSlot;
        [FoldoutGroup("[UI]")][SuffixLabel("룬 조합서")] public bool BookOfRunes;
        [FoldoutGroup("[UI]")][SuffixLabel("캐릭터 HUD")] public bool HUDCharacter;

        [FoldoutGroup("#Option")]
        [SuffixLabel("스트링 불러오기")]
        public bool LoadString;

        #region FOR EDITOR

        [FoldoutGroup("#Button")]
        [Button("Switch On All", ButtonSizes.Large)]
        public void ExternSwitchOnAll()
        {
            SwitchOnAll();
        }

        [FoldoutGroup("#Button")]
        [Button("Switch Off All", ButtonSizes.Large)]
        public void ExternSwitchOffAll()
        {
            SwitchOffAll();
        }

        #endregion FOR EDITOR

        private void SwitchOnAll()
        {
            Analytics = true;
            AreaEvent = true;
            Ability = true;

            AI = true;
            AI_Action = true;
            AI_Decision = true;
            AI_Transition = true;

            Animation = true;
            Renderer = true;

            Attack = true;
            Attack_Target = true;
            Attack_Area = true;
            Attack_Projectile = true;
            Audio = true;
            Video = true;

            Buff = true;

            Camera = true;
            Character = true;
            Monster = true;
            Elite = true;
            Boss = true;

            CharacterSpawn = true;
            CharacterWave = true;

            Damage = true;
            Detect = true;
            Data = true;
            GamePref = true;

            GameData = true;
            GameData_Skill = true;
            GameData_Stage = true;
            GameData_Relic = true;

            Quest = true;
            QuestCondition = true;
            Tutorial = true;
            Storage = true;
            Inventory = true;

            Develop = true;
            DropObject = true;
            Currency = true;

            ItemStat = true;
            ItemDrop = true;
            ItemEquip = true;

            Effect = true;
            ForceVelocity = true;
            Blink = true;
            Global = true;

            Item = true;
            ItemOption = true;

            Cursor = true;
            Input = true;
            Interaciton = true;
            JsonData = true;
            ScriptableData = true;

            NPC = true;

            Passive = true;
            PassiveTrigger = true;
            Portal = true;
            Potion = true;
            Food = true;
            Elixir = true;
            Essence = true;

            SoulGem = true;
            Prayers = true;

            Physics = true;
            Projectile = true;

            Relic = true;
            Resource = true;

            Setting = true;
            Sell = true;
            Skill = true;
            Skill_Animation = true;
            Skill_Buffer = true;
            Skill_Cost = true;
            Skill_Cooldown = true;
            Skill_Order = true;
            Skill_VFX = true;

            Stage = true;
            Stage_Monster = true;
            Stage_Reward = true;

            Difficulty = true;

            Stat = true;
            State = true;
            String = true;

            Tile = true;
            Collision = true;
            Timeline = true;
            PositionGroup = true;

            UI = true;
            UI_Minimap = true;
            UI_Button = true;
            UI_Gauge = true;
            UI_Indicator = true;
            UI_Popup = true;
            UI_Details = true;
            UI_Inventory = true;
            UI_Relic = true;
            UI_Blacksmith = true;
            UI_SelectEvent = true;
            UI_Tab = true;

            Vital = true;
            BattleResource = true;
            Weapon = true;

            InventoryPopup_DataFlow = true;
            InventoryPopup_Search = true;
            InventoryPopup_Equip = true;
            InventoryPopup_Relic = true;

            TradePopup_DataFlow = true;
            TradePopup_SearchSlot = true;

            BookOfRunes = true;

            UI_Skill = true;
            HUDCharacter = true;

            WorldMapPopup_Input = true;
            WorldMapPopup_Data = true;
        }

        private void SwitchOffAll()
        {
            Analytics = false;
            AreaEvent = false;
            Ability = false;

            AI = false;
            AI_Action = false;
            AI_Decision = false;
            AI_Transition = false;

            Animation = false;
            Renderer = false;

            Attack = false;
            Attack_Target = false;
            Attack_Area = false;
            Attack_Projectile = false;
            Audio = false;
            Video = false;

            Buff = false;

            Camera = false;
            Character = false;
            Monster = false;
            Elite = false;
            Boss = false;
            CharacterSpawn = false;
            CharacterWave = false;

            Damage = false;
            Detect = false;
            Data = false;

            GamePref = false;

            GameData = false;
            GameData_Skill = false;
            GameData_Stage = false;
            GameData_Relic = false;

            Quest = false;
            QuestCondition = false;
            Tutorial = false;
            Storage = false;
            Inventory = false;
            Difficulty = false;

            Develop = false;
            DropObject = false;
            Currency = false;

            ItemStat = false;
            ItemDrop = false;
            ItemEquip = false;

            Effect = false;
            ForceVelocity = false;
            Blink = false;
            Global = false;

            Item = false;
            ItemOption = false;

            Cursor = false;
            Input = false;
            Interaciton = false;
            JsonData = false;
            ScriptableData = false;

            NPC = false;

            Passive = false;
            PassiveTrigger = false;
            Portal = false;
            Potion = false;
            Food = false;
            Elixir = false;
            Essence = false;

            SoulGem = false;
            Prayers = false;

            Physics = false;
            Projectile = false;

            Relic = false;
            Resource = false;

            Setting = false;
            Sell = false;
            Skill = false;
            Skill_Animation = false;
            Skill_Buffer = false;
            Skill_Cost = false;
            Skill_Cooldown = false;
            Skill_Order = false;
            Skill_VFX = false;

            Stage = false;
            Stage_Monster = false;
            Stage_Reward = false;

            Stat = false;
            State = false;
            String = false;

            Tile = false;
            Collision = false;
            Timeline = false;
            PositionGroup = false;

            UI = false;
            UI_Minimap = false;
            UI_Button = false;
            UI_Gauge = false;
            UI_Indicator = false;
            UI_Popup = false;
            UI_Details = false;
            UI_Inventory = false;
            UI_Relic = false;
            UI_Blacksmith = false;
            UI_SelectEvent = false;
            UI_Tab = false;

            Vital = false;
            BattleResource = false;
            Weapon = false;

            InventoryPopup_DataFlow = false;
            InventoryPopup_Search = false;
            InventoryPopup_Equip = false;
            InventoryPopup_Relic = false;

            TradePopup_DataFlow = false;
            TradePopup_SearchSlot = false;

            BookOfRunes = false;

            UI_Skill = false;
            HUDCharacter = false;

            WorldMapPopup_Input = false;
            WorldMapPopup_Data = false;
        }

        public void OnLoadData()
        {
#if !UNITY_EDITOR
            SwitchOnAll();
#endif
        }

        public bool Find(LogTags logTag)
        {
            return logTag switch
            {
                LogTags.Analytics => Analytics,

                LogTags.AreaEvent => AreaEvent,
                LogTags.Ability => Ability,

                LogTags.AI => AI,
                LogTags.AI_Action => AI_Action,
                LogTags.AI_Decision => AI_Decision,
                LogTags.AI_Transition => AI_Transition,

                LogTags.Animation => Animation,
                LogTags.Renderer => Renderer,

                LogTags.Attack => Attack,
                LogTags.Attack_Target => Attack_Target,
                LogTags.Attack_Area => Attack_Area,
                LogTags.Attack_Projectile => Attack_Projectile,
                LogTags.Audio => Audio,
                LogTags.Video => Video,

                LogTags.Buff => Buff,

                LogTags.Camera => Camera,
                LogTags.Character => Character,
                LogTags.Monster => Monster,
                LogTags.Elite => Elite,
                LogTags.Boss => Boss,

                LogTags.CharacterSpawn => CharacterSpawn,
                LogTags.CharacterWave => CharacterWave,
                LogTags.Damage => Damage,
                LogTags.Detect => Detect,

                LogTags.Data => Data,
                LogTags.JsonData => JsonData,
                LogTags.ScriptableData => ScriptableData,
                LogTags.GamePref => GamePref,

                LogTags.GameData => GameData,
                LogTags.GameData_Skill => GameData_Skill,
                LogTags.GameData_Stage => GameData_Stage,
                LogTags.GameData_Relic => GameData_Relic,

                LogTags.Quest => Quest,
                LogTags.QuestCondition => QuestCondition,
                LogTags.Tutorial => Tutorial,
                LogTags.Storage => Storage,
                LogTags.Inventory => Inventory,
                LogTags.Difficulty => Difficulty,

                LogTags.Develop => Develop,
                LogTags.DropObject => DropObject,
                LogTags.Effect => Effect,
                LogTags.ForceVelocity => ForceVelocity,
                LogTags.Blink => Blink,
                LogTags.Global => Global,

                LogTags.Currency => Currency,
                LogTags.Item => Item,
                LogTags.ItemStat => ItemStat,
                LogTags.ItemDrop => ItemDrop,
                LogTags.ItemEquip => ItemEquip,
                LogTags.ItemOption => ItemOption,

                LogTags.Cursor => Cursor,
                LogTags.Input => Input,
                LogTags.Interaciton => Interaciton,

                LogTags.NPC => NPC,

                LogTags.Passive => Passive,
                LogTags.PassiveTrigger => PassiveTrigger,
                LogTags.Portal => Portal,
                LogTags.Potion => Potion,
                LogTags.Food => Food,
                LogTags.Elixir => Elixir,
                LogTags.Essence => Essence,
                LogTags.SoulGem => SoulGem,
                LogTags.Prayers => Prayers,

                LogTags.Physics => Physics,
                LogTags.Projectile => Projectile,

                LogTags.Relic => Relic,
                LogTags.Resource => Resource,

                LogTags.Setting => Setting,
                LogTags.Sell => Sell,
                LogTags.Skill => Skill,
                LogTags.Skill_Animation => Skill_Animation,
                LogTags.Skill_Buffer => Skill_Buffer,
                LogTags.Skill_Cost => Skill_Cost,
                LogTags.Skill_Cooldown => Skill_Cooldown,
                LogTags.Skill_Order => Skill_Order,
                LogTags.Skill_VFX => Skill_VFX,

                LogTags.Stage => Stage,
                LogTags.Stage_Monster => Stage_Monster,
                LogTags.Stage_Reward => Stage_Reward,

                LogTags.Stat => Stat,
                LogTags.State => State,
                LogTags.String => String,
                LogTags.Tile => Tile,
                LogTags.Collision => Collision,
                LogTags.Timeline => Timeline,
                LogTags.PositionGroup => PositionGroup,

                LogTags.UI => UI,
                LogTags.UI_Minimap => UI_Minimap,
                LogTags.UI_Button => UI_Button,
                LogTags.UI_Gauge => UI_Gauge,
                LogTags.UI_Indicator => UI_Indicator,
                LogTags.UI_Popup => UI_Popup,
                LogTags.UI_Details => UI_Details,
                LogTags.UI_Inventory => UI_Inventory,
                LogTags.UI_Relic => UI_Relic,
                LogTags.UI_Blacksmith => UI_Blacksmith,
                LogTags.UI_SelectEvent => UI_SelectEvent,
                LogTags.UI_Tab => UI_Tab,

                LogTags.Vital => Vital,
                LogTags.BattleResource => BattleResource,
                LogTags.Weapon => Weapon,

                LogTags.InventoryPopup_DataFlow => InventoryPopup_DataFlow,
                LogTags.InventoryPopup_Search => InventoryPopup_Search,
                LogTags.InventoryPopup_Equip => InventoryPopup_Equip,
                LogTags.InventoryPopup_Relic => InventoryPopup_Relic,

                LogTags.TradePopup_DataFlow => TradePopup_DataFlow,
                LogTags.TradePopup_SearchSlot => TradePopup_SearchSlot,

                LogTags.BookOfRunes => BookOfRunes,

                LogTags.UI_Skill => UI_Skill,
                LogTags.HUDCharacter => HUDCharacter,

                LogTags.WorldMapPopup_Input => WorldMapPopup_Input,
                LogTags.WorldMapPopup_Data => WorldMapPopup_Data,

                _ => false,
            };
        }

        public void SwitchOn(LogTags logTag)
        {
            switch (logTag)
            {
                case LogTags.Analytics: { Analytics = true; } break;
                case LogTags.AreaEvent: { AreaEvent = true; } break;
                case LogTags.Ability: { Ability = true; } break;

                case LogTags.AI: { AI = true; } break;
                case LogTags.AI_Action: { AI_Action = true; } break;
                case LogTags.AI_Decision: { AI_Decision = true; } break;
                case LogTags.AI_Transition: { AI_Transition = true; } break;

                case LogTags.Animation: { Animation = true; } break;
                case LogTags.Renderer: { Renderer = true; } break;

                case LogTags.Attack: { Attack = true; } break;
                case LogTags.Attack_Target: { Attack_Target = true; } break;
                case LogTags.Attack_Area: { Attack_Area = true; } break;
                case LogTags.Attack_Projectile: { Attack_Projectile = true; } break;
                case LogTags.Audio: { Audio = true; } break;
                case LogTags.Video: { Video = true; } break;

                case LogTags.Buff: { Buff = true; } break;

                case LogTags.Camera: { Camera = true; } break;
                case LogTags.Character: { Character = true; } break;
                case LogTags.Monster: { Monster = true; } break;
                case LogTags.Elite: { Elite = true; } break;
                case LogTags.Boss: { Boss = true; } break;

                case LogTags.CharacterSpawn: { CharacterSpawn = true; } break;
                case LogTags.CharacterWave: { CharacterWave = true; } break;

                case LogTags.Damage: { Damage = true; } break;
                case LogTags.Detect: { Detect = true; } break;

                case LogTags.Data: { Data = true; } break;
                case LogTags.JsonData: { JsonData = true; } break;
                case LogTags.ScriptableData: { ScriptableData = true; } break;
                case LogTags.GamePref: { GamePref = true; } break;

                case LogTags.GameData: { GameData = true; } break;
                case LogTags.GameData_Skill: { GameData_Skill = true; } break;
                case LogTags.GameData_Stage: { GameData_Stage = true; } break;
                case LogTags.GameData_Relic: { GameData_Relic = true; } break;

                case LogTags.Quest: { Quest = true; } break;
                case LogTags.QuestCondition: { QuestCondition = true; } break;
                case LogTags.Tutorial: { Tutorial = true; } break;
                case LogTags.Storage: { Storage = true; } break;
                case LogTags.Inventory: { Inventory = true; } break;
                case LogTags.Difficulty: { Difficulty = true; } break;

                case LogTags.Develop: { Develop = true; } break;
                case LogTags.DropObject: { DropObject = true; } break;

                case LogTags.Effect: { Effect = true; } break;
                case LogTags.ForceVelocity: { ForceVelocity = true; } break;
                case LogTags.Blink: { Blink = true; } break;

                case LogTags.Global: { Global = true; } break;

                case LogTags.Currency: { Currency = true; } break;
                case LogTags.Item: { Item = true; } break;
                case LogTags.ItemStat: { ItemStat = true; } break;
                case LogTags.ItemDrop: { ItemDrop = true; } break;
                case LogTags.ItemEquip: { ItemEquip = true; } break;
                case LogTags.ItemOption: { ItemOption = true; } break;

                case LogTags.Cursor: { Cursor = true; } break;
                case LogTags.Input: { Input = true; } break;
                case LogTags.Interaciton: { Interaciton = true; } break;

                case LogTags.NPC: { NPC = true; } break;

                case LogTags.Passive: { Passive = true; } break;
                case LogTags.PassiveTrigger: { PassiveTrigger = true; } break;
                case LogTags.Portal: { Portal = true; } break;
                case LogTags.Potion: { Potion = true; } break;
                case LogTags.Food: { Food = true; } break;
                case LogTags.Elixir: { Elixir = true; } break;
                case LogTags.Essence: { Essence = true; } break;

                case LogTags.SoulGem: { SoulGem = true; } break;
                case LogTags.Prayers: { Prayers = true; } break;

                case LogTags.Physics: { Physics = true; } break;
                case LogTags.Projectile: { Projectile = true; } break;

                case LogTags.Relic: { Relic = true; } break;
                case LogTags.Resource: { Resource = true; } break;

                case LogTags.Setting: { Setting = true; } break;
                case LogTags.Sell: { Sell = true; } break;

                case LogTags.Skill: { Skill = true; } break;
                case LogTags.Skill_Animation: { Skill_Animation = true; } break;
                case LogTags.Skill_Buffer: { Skill_Buffer = true; } break;
                case LogTags.Skill_Cost: { Skill_Cost = true; } break;
                case LogTags.Skill_Cooldown: { Skill_Cooldown = true; } break;
                case LogTags.Skill_Order: { Skill_Order = true; } break;
                case LogTags.Skill_VFX: { Skill_VFX = true; } break;

                case LogTags.Stage: { Stage = true; } break;
                case LogTags.Stage_Monster: { Stage_Monster = true; } break;
                case LogTags.Stage_Reward: { Stage_Reward = true; } break;

                case LogTags.Stat: { Stat = true; } break;
                case LogTags.State: { State = true; } break;
                case LogTags.String: { String = true; } break;

                case LogTags.Tile: { Tile = true; } break;
                case LogTags.Collision: { Collision = true; } break;
                case LogTags.Timeline: { Timeline = true; } break;
                case LogTags.PositionGroup: { PositionGroup = true; } break;

                case LogTags.UI: { UI = true; } break;
                case LogTags.UI_Minimap: { UI_Minimap = true; } break;
                case LogTags.UI_Button: { UI_Button = true; } break;
                case LogTags.UI_Gauge: { UI_Gauge = true; } break;
                case LogTags.UI_Indicator: { UI_Indicator = true; } break;
                case LogTags.UI_Popup: { UI_Popup = true; } break;
                case LogTags.UI_Details: { UI_Details = true; } break;
                case LogTags.UI_Inventory: { UI_Inventory = true; } break;
                case LogTags.UI_Relic: { UI_Relic = true; } break;
                case LogTags.UI_Blacksmith: { UI_Blacksmith = true; } break;
                case LogTags.UI_SelectEvent: { UI_SelectEvent = true; } break;
                case LogTags.UI_Tab: { UI_Tab = true; } break;

                case LogTags.Vital: { Vital = true; } break;
                case LogTags.BattleResource: { BattleResource = true; } break;
                case LogTags.Weapon: { Weapon = true; } break;

                case LogTags.InventoryPopup_DataFlow: { InventoryPopup_DataFlow = true; } break;
                case LogTags.InventoryPopup_Search: { InventoryPopup_Search = true; } break;
                case LogTags.InventoryPopup_Equip: { InventoryPopup_Equip = true; } break;
                case LogTags.InventoryPopup_Relic: { InventoryPopup_Relic = true; } break;

                case LogTags.TradePopup_DataFlow: { TradePopup_DataFlow = true; } break;
                case LogTags.TradePopup_SearchSlot: { TradePopup_SearchSlot = true; } break;

                case LogTags.BookOfRunes: { BookOfRunes = true; } break;

                case LogTags.UI_Skill: { UI_Skill = true; } break;
                case LogTags.HUDCharacter: { HUDCharacter = true; } break;

                case LogTags.WorldMapPopup_Input: { WorldMapPopup_Input = true; } break;
                case LogTags.WorldMapPopup_Data: { WorldMapPopup_Data = true; } break;
            }
            ;
        }

        public void SwitchOff(LogTags logTag)
        {
            switch (logTag)
            {
                case LogTags.Analytics: { Analytics = false; } break;
                case LogTags.AreaEvent: { AreaEvent = false; } break;
                case LogTags.Ability: { Ability = false; } break;

                case LogTags.AI: { AI = false; } break;
                case LogTags.AI_Action: { AI_Action = false; } break;
                case LogTags.AI_Decision: { AI_Decision = false; } break;
                case LogTags.AI_Transition: { AI_Transition = false; } break;

                case LogTags.Animation: { Animation = false; } break;
                case LogTags.Renderer: { Renderer = false; } break;

                case LogTags.Attack: { Attack = false; } break;
                case LogTags.Attack_Target: { Attack_Target = false; } break;
                case LogTags.Attack_Area: { Attack_Area = false; } break;
                case LogTags.Attack_Projectile: { Attack_Projectile = false; } break;
                case LogTags.Audio: { Audio = false; } break;
                case LogTags.Video: { Video = false; } break;

                case LogTags.Buff: { Buff = false; } break;

                case LogTags.Camera: { Camera = false; } break;
                case LogTags.Character: { Character = false; } break;
                case LogTags.Monster: { Monster = false; } break;
                case LogTags.Elite: { Elite = false; } break;
                case LogTags.Boss: { Boss = false; } break;
                case LogTags.CharacterSpawn: { CharacterSpawn = false; } break;
                case LogTags.CharacterWave: { CharacterWave = false; } break;

                case LogTags.Damage: { Damage = false; } break;
                case LogTags.Detect: { Detect = false; } break;

                case LogTags.Data: { Data = false; } break;
                case LogTags.JsonData: { JsonData = false; } break;
                case LogTags.ScriptableData: { ScriptableData = false; } break;
                case LogTags.GamePref: { GamePref = false; } break;

                case LogTags.GameData: { GameData = false; } break;
                case LogTags.GameData_Skill: { GameData_Skill = false; } break;
                case LogTags.GameData_Stage: { GameData_Stage = false; } break;
                case LogTags.GameData_Relic: { GameData_Relic = false; } break;

                case LogTags.Quest: { Quest = false; } break;
                case LogTags.QuestCondition: { QuestCondition = false; } break;
                case LogTags.Tutorial: { Tutorial = false; } break;
                case LogTags.Storage: { Storage = false; } break;
                case LogTags.Inventory: { Inventory = false; } break;

                case LogTags.Difficulty: { Difficulty = false; } break;

                case LogTags.Develop: { Develop = false; } break;
                case LogTags.DropObject: { DropObject = false; } break;
                case LogTags.Effect: { Effect = false; } break;
                case LogTags.ForceVelocity: { ForceVelocity = false; } break;
                case LogTags.Blink: { Blink = false; } break;

                case LogTags.Global: { Global = false; } break;

                case LogTags.Currency: { Currency = false; } break;
                case LogTags.Item: { Item = false; } break;
                case LogTags.ItemStat: { ItemStat = false; } break;
                case LogTags.ItemDrop: { ItemDrop = false; } break;
                case LogTags.ItemEquip: { ItemEquip = false; } break;
                case LogTags.ItemOption: { ItemOption = false; } break;

                case LogTags.Cursor: { Cursor = false; } break;
                case LogTags.Input: { Input = false; } break;
                case LogTags.Interaciton: { Interaciton = false; } break;

                case LogTags.NPC: { NPC = false; } break;

                case LogTags.Passive: { Passive = false; } break;
                case LogTags.PassiveTrigger: { PassiveTrigger = false; } break;
                case LogTags.Portal: { Portal = false; } break;
                case LogTags.Potion: { Potion = false; } break;
                case LogTags.Food: { Food = false; } break;
                case LogTags.Elixir: { Elixir = false; } break;
                case LogTags.Essence: { Essence = false; } break;

                case LogTags.SoulGem: { SoulGem = false; } break;
                case LogTags.Prayers: { Prayers = false; } break;

                case LogTags.Physics: { Physics = false; } break;
                case LogTags.Projectile: { Projectile = false; } break;

                case LogTags.Relic: { Relic = false; } break;
                case LogTags.Resource: { Resource = false; } break;

                case LogTags.Setting: { Setting = false; } break;
                case LogTags.Sell: { Sell = false; } break;

                case LogTags.Skill: { Skill = false; } break;
                case LogTags.Skill_Animation: { Skill_Animation = false; } break;
                case LogTags.Skill_Buffer: { Skill_Buffer = false; } break;
                case LogTags.Skill_Cost: { Skill_Cost = false; } break;
                case LogTags.Skill_Cooldown: { Skill_Cooldown = false; } break;
                case LogTags.Skill_Order: { Skill_Order = false; } break;
                case LogTags.Skill_VFX: { Skill_VFX = false; } break;

                case LogTags.Stage: { Stage = false; } break;
                case LogTags.Stage_Monster: { Stage_Monster = false; } break;
                case LogTags.Stage_Reward: { Stage_Reward = false; } break;

                case LogTags.Stat: { Stat = false; } break;
                case LogTags.State: { State = false; } break;
                case LogTags.String: { String = false; } break;

                case LogTags.Tile: { Tile = false; } break;
                case LogTags.Collision: { Collision = false; } break;
                case LogTags.Timeline: { Timeline = false; } break;
                case LogTags.PositionGroup: { PositionGroup = false; } break;

                case LogTags.UI: { UI = false; } break;
                case LogTags.UI_Minimap: { UI_Minimap = false; } break;
                case LogTags.UI_Button: { UI_Button = false; } break;
                case LogTags.UI_Gauge: { UI_Gauge = false; } break;
                case LogTags.UI_Indicator: { UI_Indicator = false; } break;
                case LogTags.UI_Popup: { UI_Popup = false; } break;
                case LogTags.UI_Details: { UI_Details = false; } break;
                case LogTags.UI_Inventory: { UI_Inventory = false; } break;
                case LogTags.UI_Relic: { UI_Relic = false; } break;
                case LogTags.UI_Blacksmith: { UI_Blacksmith = false; } break;
                case LogTags.UI_SelectEvent: { UI_SelectEvent = false; } break;
                case LogTags.UI_Tab: { UI_Tab = false; } break;

                case LogTags.Vital: { Vital = false; } break;
                case LogTags.BattleResource: { BattleResource = false; } break;
                case LogTags.Weapon: { Weapon = false; } break;

                case LogTags.InventoryPopup_DataFlow: { InventoryPopup_DataFlow = false; } break;
                case LogTags.InventoryPopup_Search: { InventoryPopup_Search = false; } break;
                case LogTags.InventoryPopup_Equip: { InventoryPopup_Equip = false; } break;
                case LogTags.InventoryPopup_Relic: { InventoryPopup_Relic = false; } break;

                case LogTags.TradePopup_DataFlow: { TradePopup_DataFlow = false; } break;
                case LogTags.TradePopup_SearchSlot: { TradePopup_SearchSlot = false; } break;

                case LogTags.BookOfRunes: { BookOfRunes = false; } break;

                case LogTags.UI_Skill: { UI_Skill = false; } break;

                case LogTags.HUDCharacter: { HUDCharacter = false; } break;

                case LogTags.WorldMapPopup_Input: { WorldMapPopup_Input = false; } break;
                case LogTags.WorldMapPopup_Data: { WorldMapPopup_Data = false; } break;
            }
            ;
        }

        public void Refresh()
        {
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
#endif
        }
    }
}