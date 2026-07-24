namespace CWFramework
{
    public enum LogTags
    {
        None,

        Analytics,

        #region Character

        /// <summary> 캐릭터 </summary>
        Character,

        /// <summary> 몬스터 캐릭터 </summary>
        Monster,

        /// <summary> 엘리트 캐릭터 </summary>
        Elite,

        /// <summary> 보스 캐릭터 </summary>
        Boss,

        /// <summary> 캐릭터 능력 </summary>
        Ability,

        /// <summary> 탐지 </summary>
        Detect,

        /// <summary> 물리 </summary>
        Physics,

        /// <summary> 강제이동 </summary>
        ForceVelocity,

        /// <summary> 순간이동 </summary>
        Blink,

        /// <summary> 캐릭터 생성 </summary>
        CharacterSpawn,

        /// <summary> 캐릭터 웨이브 </summary>
        CharacterWave,

        #endregion Character

        #region Character-AI

        /// <summary> AI </summary>
        AI,

        /// <summary> AI 행동 </summary>
        AI_Action,

        /// <summary> AI 결정 </summary>
        AI_Decision,

        /// <summary> AI 상태전환 </summary>
        AI_Transition,

        #endregion Character-AI

        #region Character-Battle

        /// <summary> 공격 </summary>
        Attack,

        /// <summary> 목표 공격 </summary>
        Attack_Target,

        /// <summary> 영역 공격 </summary>
        Attack_Area,

        /// <summary> 발사체 생성 공격 </summary>
        Attack_Projectile,

        /// <summary> 전투 자원(생명력/자원/보호막) </summary>
        BattleResource,

        /// <summary> 버프 </summary>
        Buff,

        /// <summary> 피해량 계산 </summary>
        Damage,

        /// <summary> 이펙트 </summary>
        Effect,

        /// <summary> 패시브 </summary>
        Passive,

        /// <summary> 패시브 발동 </summary>
        PassiveTrigger,

        /// <summary> 발사체 </summary>
        Projectile,

        /// <summary> 상태 </summary>
        State,

        /// <summary> 능력치 </summary>
        Stat,

        /// <summary> 캐릭터 전투 자원 </summary>
        Vital,

        #endregion Character-Battle

        #region Character-Skill

        Skill, // 기술
        Skill_Animation,    // 기술 애니메이션
        Skill_Buffer,       // 기술 버퍼
        Skill_Cost,         // 기술 비용
        Skill_Cooldown,     // 기술 재사용 대기시간
        Skill_Order,        // 기술 순서
        Skill_VFX,          // 기술 시각 효과

        #endregion Character-Skill

        #region Character-Data

        /// <summary> 데이터 </summary>
        Data,

        /// <summary> 임시 데이터 </summary>
        GamePref,

        /// <summary> Json 데이터 </summary>
        JsonData,

        /// <summary> 리소스 </summary>
        Resource,

        /// <summary> 스크립터블 데이터 </summary>
        ScriptableData,

        #endregion Character-Data

        #region Game-Data

        /// <summary> 게임 데이터 </summary>
        GameData,

        /// <summary> 게임 데이터 : 기술 </summary>
        GameData_Skill,

        /// <summary> 게임 데이터 : 스테이지 </summary>
        GameData_Stage,

        /// <summary> 게임 데이터 : 스테이지 </summary>
        GameData_Relic,

        /// <summary> 퀘스트 </summary>
        Quest,

        /// <summary> 퀘스트 조건 </summary>
        QuestCondition,

        /// <summary> 튜토리얼 </summary>
        Tutorial,

        /// <summary> 소지품 </summary>
        Inventory,

        /// <summary> 창고 </summary>
        Storage,

        /// <summary> 세계 난이도 </summary>
        Difficulty,

        #endregion Game-Data

        #region Develop

        /// <summary> 개발용 </summary>
        Develop,

        #endregion Develop

        #region Interaction

        /// <summary> 영역 이벤트 </summary>
        AreaEvent,

        /// <summary> 드랍 오브젝트 </summary>
        DropObject,

        /// <summary> 상호작용 </summary>
        Interaciton,

        /// <summary> NPC </summary>
        NPC,

        #endregion Interaction

        #region Item

        /// <summary> 재화 </summary>
        Currency,

        /// <summary> 정수 </summary>
        Essence,

        /// <summary> 음식 </summary>
        Food,

        /// <summary> 비약 </summary>
        Elixir,

        /// <summary> 아이템 </summary>
        Item,

        /// <summary> 아이템 능력치 </summary>
        ItemStat,

        /// <summary> 아이템 드랍 </summary>
        ItemDrop,

        /// <summary> 아이템 착용 </summary>
        ItemEquip,

        /// <summary> 아이템 옵션 </summary>
        ItemOption,

        /// <summary> 조리법 </summary>
        Recipe,

        /// <summary> 유물 </summary>
        Relic,

        /// <summary> 판매 </summary>
        Sell,

        /// <summary> 영혼 보석 </summary>
        SoulGem,

        /// <summary> 기도 </summary>
        Prayers,

        /// <summary> 무기 </summary>
        Weapon,

        /// <summary> 물약 </summary>
        Potion,

        #endregion Item

        #region MapObject

        /// <summary> 포탈 </summary>
        Portal,

        /// <summary> 포지션 그룹 </summary>
        PositionGroup,

        #endregion MapObject

        #region Renderer

        /// <summary> 애니메이션 </summary>
        Animation,

        /// <summary> 랜더러 </summary>
        Renderer,

        #endregion Renderer

        #region Setting

        /// <summary> 설정 </summary>
        Setting,

        /// <summary> 음향 </summary>
        Audio,
        Audio_BGM,
        Audio_AMB,
        Audio_SFX,

        Video,

        /// <summary> 카메라 </summary>
        Camera,

        /// <summary> 커서 </summary>
        Cursor,

        /// <summary> 글로벌 이벤트 </summary>
        Global,

        /// <summary> 입력 </summary>
        Input,

        #endregion Setting

        #region Stage

        Stage, // 스테이지
        Stage_Monster, // 스테이지 몬스터
        Stage_Reward, // 스테이지 보상

        Tile, // 타일
        Collision, // 충돌체

        #endregion Stage

        #region String

        /// <summary> 스트링 텍스트 </summary>
        String,

        #endregion String

        #region Timeline

        /// <summary> 타임라인 </summary>
        Timeline,

        #endregion Timeline

        #region UI

        /// <summary> UI </summary>
        UI,

        /// <summary> UI 버튼 </summary>
        UI_Button,

        /// <summary> UI 게이지 </summary>
        UI_Gauge,

        /// <summary> UI 미니맵 </summary>
        UI_Minimap,

        /// <summary> UI 인디케이터 </summary>
        UI_Indicator,

        /// <summary> 캐릭터 HUD </summary>
        HUDCharacter,

        /// <summary> 인벤토리 이벤트 데이터 </summary>
        InventoryPopup_DataFlow,

        /// <summary> 인벤토리 장착 </summary>
        InventoryPopup_Equip,

        /// <summary> 인벤토리 유물 </summary>
        InventoryPopup_Relic,

        /// <summary> 인벤토리 슬롯 탐색 </summary>
        InventoryPopup_Search,

        /// <summary> UI 인벤토리 </summary>
        UI_Inventory,

        /// <summary> UI 대장장이 </summary>
        UI_Blacksmith,

        /// <summary> UI 팝업 </summary>
        UI_Popup,

        /// <summary> UI 상세정보 </summary>
        UI_Details,

        /// <summary> UI 스킬 </summary>
        UI_Skill,

        /// <summary> UI 유물 </summary>
        UI_Relic,

        /// <summary> UI 선택 이벤트 </summary>
        UI_SelectEvent,

        /// <summary> UI 탭 </summary>
        UI_Tab,

        /// <summary> 룬 조합서 </summary>
        BookOfRunes,

        /// <summary> 거래 - 데이터 경로 </summary>
        TradePopup_DataFlow,

        /// <summary> 거래 - 슬롯 탐색 </summary>
        TradePopup_SearchSlot,

        /// <summary> 월드맵 데이터 </summary>
        WorldMapPopup_Data,

        /// <summary> 월드맵 입력 </summary>
        WorldMapPopup_Input,

        /// <summary> 전직 </summary>
        PromotionPopup,

        #endregion UI
    }
}