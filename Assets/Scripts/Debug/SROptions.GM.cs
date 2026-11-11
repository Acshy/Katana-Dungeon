using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public partial class SROptions
{
    private int gmValue;
    private int gmChapterId = 1;
    private int gmHeroId = 1101;
    private int gmItemId;
    private int gmEnemyId;
    private int gmBattleChapterId = 1;
    private int gmBattleLevel = 1;
    private int gmBattleRoom = 1;
    private float gmTimeScale = 1f;
    private bool gmInvincible;

    [Category("GM - 全局存档")]
    [Description("钻石、魂、经验、火种等资源命令使用的数值。")]
    public int GMValue
    {
        get => gmValue;
        set => gmValue = value;
    }

    [Category("GM - 全局存档")]
    [Description("指定要开放到的章节 ID。")]
    public int GMChapterId
    {
        get => gmChapterId;
        set => gmChapterId = value;
    }

    [Category("GM - 全局存档")]
    [Description("英雄相关命令使用的英雄 ID。")]
    public int GMHeroId
    {
        get => gmHeroId;
        set => gmHeroId = value;
    }

    [Category("GM - 全局存档")]
    [Description("物品相关命令使用的物品 ID。")]
    public int GMItemId
    {
        get => gmItemId;
        set => gmItemId = value;
    }

    [Category("GM - 全局存档")]
    public void AddGM_Diamond()
    {
        if (!HasArchive()) return;
        ArchiveManager.Inst.ChangeDiamond(gmValue);
        ArchiveManager.Inst.SaveArchive();
        Debug.Log($"GM：钻石增加 {gmValue}");
    }

    [Category("GM - 全局存档")]
    public void SetGM_Diamond()
    {
        if (!HasArchive()) return;
        int delta = gmValue - ArchiveManager.Inst.ArchiveData.GlobalData.Diamond;
        ArchiveManager.Inst.ChangeDiamond(delta);
        ArchiveManager.Inst.SaveArchive();
        Debug.Log($"GM：钻石设置为 {gmValue}");
    }

    [Category("GM - 全局存档")]
    public void AddGM_Soul()
    {
        if (!HasArchive()) return;
        ArchiveManager.Inst.ChangeSoul(gmValue);
        ArchiveManager.Inst.SaveArchive();
        Debug.Log($"GM：魂增加 {gmValue}");
    }

    [Category("GM - 全局存档")]
    public void SetGM_Soul()
    {
        if (!HasArchive()) return;
        int delta = gmValue - ArchiveManager.Inst.ArchiveData.GlobalData.Soul;
        ArchiveManager.Inst.ChangeSoul(delta);
        ArchiveManager.Inst.SaveArchive();
        Debug.Log($"GM：魂设置为 {gmValue}");
    }

    [Category("GM - 全局存档")]
    public void AddGM_Exp()
    {
        if (!HasArchive()) return;
        ArchiveManager.Inst.ChangeExp(gmValue);
        ArchiveManager.Inst.SaveArchive();
        Debug.Log($"GM：经验增加 {gmValue}");
    }

    [Category("GM - 全局存档")]
    public void SetGM_Exp()
    {
        if (!HasArchive()) return;
        int delta = gmValue - ArchiveManager.Inst.ArchiveData.GlobalData.Exp;
        ArchiveManager.Inst.ChangeExp(delta);
        ArchiveManager.Inst.SaveArchive();
        Debug.Log($"GM：经验设置为 {gmValue}");
    }

    [Category("GM - 全局存档")]
    public void SetGM_Fire()
    {
        if (!HasArchive()) return;
        GlobalData_1_4_0 data = ArchiveManager.Inst.ArchiveData.GlobalData;
        data.Fire = Mathf.Clamp(gmValue, 0, Mathf.Max(0, data.MaxFire));
        data.FireRecoveryTime = data.Fire >= data.MaxFire ? -1 : DateTime.Now.Ticks / 10000000;
        EventManager.Inst.DistributeEvent(EventName.OnArchiveFireChange);
        ArchiveManager.Inst.SaveArchive();
        Debug.Log($"GM：火种设置为 {data.Fire}/{data.MaxFire}");
    }

    [Category("GM - 全局存档")]
    public void SetGM_MaxFire()
    {
        if (!HasArchive()) return;
        GlobalData_1_4_0 data = ArchiveManager.Inst.ArchiveData.GlobalData;
        data.MaxFire = Mathf.Max(1, gmValue);
        data.Fire = Mathf.Min(data.Fire, data.MaxFire);
        data.FireRecoveryTime = data.Fire >= data.MaxFire ? -1 : DateTime.Now.Ticks / 10000000;
        EventManager.Inst.DistributeEvent(EventName.OnArchiveFireChange);
        ArchiveManager.Inst.SaveArchive();
        Debug.Log($"GM：火种上限设置为 {data.MaxFire}");
    }

    [Category("GM - 全局存档")]
    [Description("将目标章节之前的章节标记为已通关，并选择目标章节。章节 ID 以项目章节表为准。")]
    public void SetGM_UnlockedChapter()
    {
        if (!HasArchive()) return;
        if (!DataManager.Inst.ChapterTableDatas.ContainsKey(gmChapterId))
        {
            Debug.LogError($"GM：章节 ID {gmChapterId} 不存在");
            return;
        }

        Dictionary<int, ChapterClearanceData> progress = ArchiveManager.Inst.ArchiveData.StatisticsData.ChapterClearanceDatas;
        if (!progress.ContainsKey(0))
            progress.Add(0, new ChapterClearanceData { Schedule = 100, Count = 1 });
        else
        {
            progress[0].Schedule = 100;
            progress[0].Count = Mathf.Max(1, progress[0].Count);
        }

        foreach (int chapterId in DataManager.Inst.ChapterTableDatas.Keys)
        {
            if (chapterId >= gmChapterId)
                continue;
            if (!progress.ContainsKey(chapterId))
                progress.Add(chapterId, new ChapterClearanceData { Schedule = 100, Count = 1 });
            else
                progress[chapterId].Schedule = 100;
        }

        ArchiveManager.Inst.ArchiveData.GlobalData.LastSelectChapterId = gmChapterId;
        ArchiveManager.Inst.SaveArchive();
        Debug.Log($"GM：已开放并选择章节 {gmChapterId}");
    }

    [Category("GM - 全局存档")]
    public void GM_UnlockAllChapters()
    {
        if (!HasArchive()) return;
        if (!ArchiveManager.Inst.ArchiveData.StatisticsData.ChapterClearanceDatas.ContainsKey(0))
            ArchiveManager.Inst.ArchiveData.StatisticsData.ChapterClearanceDatas.Add(
                0, new ChapterClearanceData { Schedule = 100, Count = 1 });
        else
        {
            ChapterClearanceData tutorial = ArchiveManager.Inst.ArchiveData.StatisticsData.ChapterClearanceDatas[0];
            tutorial.Schedule = 100;
            tutorial.Count = Mathf.Max(1, tutorial.Count);
        }
        foreach (int chapterId in DataManager.Inst.ChapterTableDatas.Keys)
        {
            if (!ArchiveManager.Inst.ArchiveData.StatisticsData.ChapterClearanceDatas.ContainsKey(chapterId))
                ArchiveManager.Inst.ArchiveData.StatisticsData.ChapterClearanceDatas.Add(
                    chapterId, new ChapterClearanceData { Schedule = 100, Count = 1 });
            else
            {
                ChapterClearanceData progress = ArchiveManager.Inst.ArchiveData.StatisticsData.ChapterClearanceDatas[chapterId];
                progress.Schedule = 100;
                progress.Count = Mathf.Max(1, progress.Count);
            }
        }
        ArchiveManager.Inst.SaveArchive();
        Debug.Log("GM：全部章节已解锁");
    }

    [Category("GM - 全局存档")]
    public void GM_UnlockAllHeroes()
    {
        if (!HasArchive()) return;
        GlobalData_1_4_0 data = ArchiveManager.Inst.ArchiveData.GlobalData;
        foreach (int heroId in DataManager.Inst.HeroDatas.Keys)
        {
            HeroUpgradeData? initial = DataManager.Inst.GetHeroUpgradeData(heroId, 1, 0);
            if (!initial.HasValue)
                continue;
            if (!data.UnLockHeros.Contains(heroId))
                data.UnLockHeros.Add(heroId);
            if (!data.HeroUpgradeDatas.ContainsKey(heroId))
            {
                HeroUpgradeInfo info = new HeroUpgradeInfo();
                info.SetHeroUpgradeData(initial.Value);
                data.HeroUpgradeDatas.Add(heroId, info);
            }
        }
        ArchiveManager.Inst.SaveArchive();
        Debug.Log("GM：全部可用英雄已解锁");
    }

    [Category("GM - 全局存档")]
    public void GM_UnlockAllItems()
    {
        if (!HasArchive()) return;
        List<int> unlockedItems = ArchiveManager.Inst.ArchiveData.GlobalData.UnLockItems;
        foreach (int itemId in DataManager.Inst.AllItemObj.Keys)
        {
            if (!unlockedItems.Contains(itemId))
                unlockedItems.Add(itemId);
        }
        ArchiveManager.Inst.SaveArchive();
        Debug.Log("GM：全部物品已解锁；下一局生效");
    }

    [Category("GM - 全局存档")]
    public void GM_UnlockHero()
    {
        if (!HasArchive()) return;
        if (!DataManager.Inst.HeroDatas.ContainsKey(gmHeroId))
        {
            Debug.LogError($"GM：英雄 ID {gmHeroId} 不存在");
            return;
        }

        GlobalData_1_4_0 data = ArchiveManager.Inst.ArchiveData.GlobalData;
        HeroUpgradeData? initial = null;
        if (!data.HeroUpgradeDatas.ContainsKey(gmHeroId))
        {
            initial = DataManager.Inst.GetHeroUpgradeData(gmHeroId, 1, 0);
            if (!initial.HasValue)
            {
                Debug.LogError($"GM：英雄 {gmHeroId} 缺少初始培養配置");
                return;
            }
        }

        if (!data.UnLockHeros.Contains(gmHeroId))
            data.UnLockHeros.Add(gmHeroId);
        if (!data.HeroUpgradeDatas.ContainsKey(gmHeroId))
        {
            HeroUpgradeInfo info = new HeroUpgradeInfo();
            info.SetHeroUpgradeData(initial.Value);
            data.HeroUpgradeDatas.Add(gmHeroId, info);
        }
        ArchiveManager.Inst.SaveArchive();
        Debug.Log($"GM：英雄 {gmHeroId} 已解锁");
    }

    [Category("GM - 全局存档")]
    public void GM_UnlockItem()
    {
        if (!HasArchive()) return;
        if (!DataManager.Inst.AllItemObj.ContainsKey(gmItemId))
        {
            Debug.LogError($"GM：物品 ID {gmItemId} 不存在");
            return;
        }
        ArchiveManager.Inst.UnLockItem(gmItemId, true);
        ArchiveManager.Inst.SaveArchive();
        Debug.Log($"GM：物品 {gmItemId} 已解锁；下一局生效");
    }

    [Category("GM - 全局存档")]
    [Description("将目标章节、关卡和房间写入战斗续档。关卡和房间按从 1 开始输入。")]
    public int GMBattleChapterId
    {
        get => gmBattleChapterId;
        set => gmBattleChapterId = value;
    }

    [Category("GM - 全局存档")]
    public int GMBattleLevel
    {
        get => gmBattleLevel;
        set => gmBattleLevel = value;
    }

    [Category("GM - 全局存档")]
    public int GMBattleRoom
    {
        get => gmBattleRoom;
        set => gmBattleRoom = value;
    }

    [Category("GM - 全局存档")]
    public void GM_SetBattleResumePosition()
    {
        if (!HasArchive()) return;
        if (!DataManager.Inst.ChapterTableDatas.TryGetValue(gmBattleChapterId, out ChapterTabData chapter) ||
            gmBattleLevel < 1 || gmBattleLevel > chapter.LevelCount || gmBattleRoom < 1)
        {
            Debug.LogError("GM：续档位置无效；章节必须存在，关卡需在章节范围内，关卡和房间从 1 开始");
            return;
        }

        if (BattleManager.Inst.RuntimeData is ChapterRulesRuntimeData currentRuntime &&
            currentRuntime.CurrentChapterId == gmBattleChapterId &&
            currentRuntime.LevelStructData is ChapterStructData currentChapter &&
            gmBattleLevel <= currentChapter.RoomList.Count &&
            gmBattleRoom > currentChapter.RoomList[gmBattleLevel - 1].Count)
        {
            Debug.LogError("GM：目标房间超过当前已生成关卡的房间数量");
            return;
        }

        BattleManager battle = BattleManager.Inst;
        BattleArchiveData_1_4_0 saved = ArchiveManager.Inst.ArchiveData.BattleData ?? new BattleArchiveData_1_4_0();
        saved.Guid = battle.RuntimeData == null ? Guid.NewGuid() : battle.RuntimeData.BattleGuid;
        saved.ChapterData = new BattleArchiveChapterData_1_4_0
        {
            CurrentCPId = gmBattleChapterId,
            CurrentLevelIndex = gmBattleLevel - 1,
            CurrentRoomIndex = gmBattleRoom - 1
        };
        saved.PlayerData = new BattleArchivePlayerData_1_4_0
        {
            CurrentGold = battle.CurrentPlayer == null ? 0 : battle.CurrentGold,
            HeroId = battle.CurrentPlayer == null ? ArchiveManager.Inst.ArchiveData.GlobalData.LastSelectHeroID : battle.CurrentPlayer.UniqueID,
            CurrentHp = battle.CurrentPlayer == null ? 1f : battle.CurrentPlayer.CurrentHp,
            Items = battle.CurrentPlayer == null
                ? new List<int>()
                : battle.CurrentPlayer.roleItemController.GetItemsId()
        };
        ArchiveManager.Inst.ArchiveData.BattleData = saved;
        ArchiveManager.Inst.ArchiveData.GlobalData.LastSelectChapterId = gmBattleChapterId;
        ArchiveManager.Inst.SaveArchive();
        Debug.Log($"GM：已将续档位置设为章节 {gmBattleChapterId}，第 {gmBattleLevel} 关，第 {gmBattleRoom} 房");
    }

    [Category("GM - 当前战局")]
    [Description("当前章节战斗要跳到的关卡，从 1 开始。")]
    public int GMJumpLevel
    {
        get => gmBattleLevel;
        set => gmBattleLevel = value;
    }

    [Category("GM - 当前战局")]
    public void GM_JumpToLevel()
    {
        BattleManager battle = BattleManager.Inst;
        if (battle == null || !battle.GameIsRuning || !(battle.BattleRules is SimilarChapterRules rules))
        {
            Debug.LogError("GM：请在章节或冒险战斗开始后使用跳关");
            return;
        }
        GameManager.Inst.StartCoroutine(rules.GMJumpToLevel(gmBattleLevel));
    }

    [Category("GM - 当前战局")]
    public void GM_GiveItemToPlayer()
    {
        if (!HasRunningPlayer()) return;
        if (!DataManager.Inst.AllItemObj.TryGetValue(gmItemId, out ItemScriptableObject itemConfig))
        {
            Debug.LogError($"GM：物品 ID {gmItemId} 不存在");
            return;
        }
        Item item = DataManager.Inst.ParsingItemObj(itemConfig);
        BattleManager.Inst.CurrentPlayer.roleItemController.AddItem(item, added =>
            Debug.Log(added ? $"GM：已给予物品 {item.Name}" : "GM：物品添加失败"));
    }

    [Category("GM - 当前战局")]
    public void GM_ReplaceCurrentHero()
    {
        if (!HasRunningPlayer()) return;
        if (!DataManager.Inst.HeroDatas.ContainsKey(gmHeroId))
        {
            Debug.LogError($"GM：英雄 ID {gmHeroId} 不存在");
            return;
        }
        GM_UnlockHero();
        if (!ArchiveManager.Inst.ArchiveData.GlobalData.HeroUpgradeDatas.ContainsKey(gmHeroId))
            return;
        if (BattleManager.Inst.BattleRules is SimilarChapterRules rules)
        {
            rules.GMReplaceCurrentHero(gmHeroId);
            if (gmInvincible)
                GameManager.Inst.StartCoroutine(ReapplyGMInvincible(gmHeroId));
        }
        else
            Debug.LogError("GM：当前战斗规则不支持替换英雄");
    }

    [Category("GM - 当前战局")]
    public void GM_HealOrResurrectPlayer()
    {
        if (!HasRunningPlayer()) return;
        PlayerController player = BattleManager.Inst.CurrentPlayer;
        if (player.IsDie)
            BattleManager.Inst.Resurrection(1f);
        else
            player.HpTreatment(new TreatmentData(player.MaxHp, player.TemporaryId));
        player.rolePower.AddPower(player.MaxPower);
        player.rolePower.AddSkillPower(player.MaxSkillPower);
        Debug.Log("GM：玩家生命、体力和技能能量已恢复");
    }

    [Category("GM - 当前战局")]
    [Description("开启后保持当前玩家无敌；重复设置不会叠加无敌计数。")]
    public bool GM_PlayerInvincible
    {
        get => gmInvincible;
        set
        {
            if (gmInvincible == value)
                return;
            gmInvincible = value;
            ApplyGMInvincibleToCurrentPlayer();
        }
    }

    private void ApplyGMInvincibleToCurrentPlayer()
    {
        if (BattleManager.Inst == null || BattleManager.Inst.CurrentPlayer == null)
            return;
        PlayerController player = BattleManager.Inst.CurrentPlayer;
        if (player.roleHealth.IsGod != gmInvincible)
            player.SetGod(gmInvincible);
    }

    private IEnumerator ReapplyGMInvincible(int heroId)
    {
        float elapsed = 0f;
        while ((BattleManager.Inst.CurrentPlayer == null || BattleManager.Inst.CurrentPlayer.UniqueID != heroId) && elapsed < 20f)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        ApplyGMInvincibleToCurrentPlayer();
    }

    [Category("GM - 当前战局")]
    public int GMBattleValue
    {
        get => gmValue;
        set => gmValue = value;
    }

    [Category("GM - 当前战局")]
    public void GM_AddBattleGold()
    {
        if (!HasRunningPlayer()) return;
        BattleManager.Inst.AddGold(gmValue);
        Debug.Log($"GM：本局金币增加 {gmValue}");
    }

    [Category("GM - 当前战局")]
    public void GM_SetBattleGold()
    {
        if (!HasRunningPlayer()) return;
        BattleRuntimeBaseData runtimeData = BattleManager.Inst.RuntimeData;
        int delta = gmValue - runtimeData.CurrentGold;
        runtimeData.SetGold(gmValue);
        EventManager.Inst.DistributeEvent(EventName.OnAddMoney, delta);
        Debug.Log($"GM：本局金币设置为 {gmValue}");
    }

    [Category("GM - 当前战局")]
    public void GM_ClearCurrentRoomEnemies()
    {
        if (!HasRunningPlayer()) return;
        List<RoleController> enemies = new List<RoleController>(BattleManager.Inst.EnemyTeam.Values);
        foreach (RoleController enemy in enemies)
        {
            if (enemy == null || enemy.IsDie) continue;
            enemy.HpInjured(new DamageInfo(enemy.TemporaryId, 999999, BattleManager.Inst.CurrentPlayer,
                BattleManager.Inst.CurrentPlayer.transform.position, DmgType.Other, false, false, 0, 0));
        }
        Debug.Log("GM：已击杀当前已生成的敌人");
    }

    [Category("GM - 当前战局")]
    public void GM_SpawnEnemy()
    {
        if (!HasRunningPlayer()) return;
        if (!DataManager.Inst.EnemyDatas.TryGetValue(gmEnemyId, out EnemyInfoData enemy))
        {
            Debug.LogError($"GM：怪物 ID {gmEnemyId} 不存在");
            return;
        }
        BattleTool.CreateEnemy(enemy, controller =>
        {
            if (controller != null)
                controller.transform.position = BattleManager.Inst.CurrentPlayer.transform.position + Vector3.right * 2f;
        });
    }

    [Category("GM - 当前战局")]
    [Description("怪物召唤命令使用的怪物 ID。")]
    public int GMEnemyId
    {
        get => gmEnemyId;
        set => gmEnemyId = value;
    }

    [Category("GM - 当前战局")]
    [Description("设置当前游戏速度，1 为正常速度。")]
    public float GM_TimeScale
    {
        get => gmTimeScale;
        set
        {
            gmTimeScale = Mathf.Clamp(value, 0.1f, 3f);
            if (TimeManager.Inst != null)
                TimeManager.Inst.SetTimeScale(gmTimeScale);
        }
    }

    [Category("GM - 当前战局")]
    public void GM_ResetTimeScale()
    {
        gmTimeScale = 1f;
        if (TimeManager.Inst != null)
            TimeManager.Inst.SetTimeScale(1f);
    }

    private bool HasArchive()
    {
        if (ArchiveManager.Inst?.ArchiveData != null)
            return true;
        Debug.LogError("GM：存档尚未初始化");
        return false;
    }

    private bool HasRunningPlayer()
    {
        if (BattleManager.Inst != null && BattleManager.Inst.GameIsRuning && BattleManager.Inst.CurrentPlayer != null)
            return true;
        Debug.LogError("GM：当前没有正在运行的玩家战局");
        return false;
    }
}
