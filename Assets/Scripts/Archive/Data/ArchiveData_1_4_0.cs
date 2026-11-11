using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class ArchiveData_1_4_0 : IArchiveData
{
    public string Version => "1.4.0";
    
    [LabelText("临时数据"), Tooltip("记录上一场战斗产生的临时解锁和资源变化。")]
    public TemporaryData_1_4_0 TemporaryData;
    [LabelText("全局数据"), Tooltip("记录长期生效的资源、英雄成长、章节选择和各类解锁。")]
    public GlobalData_1_4_0 GlobalData;
    [LabelText("战斗续档"), Tooltip("记录离开战斗时保存的关卡、房间和玩家状态。")]
    public BattleArchiveData_1_4_0 BattleData;
    [LabelText("统计数据"), Tooltip("记录启动、游玩、击杀、章节和图鉴相关统计。")]
    public StatisticsData_1_4_0 StatisticsData;
    [LabelText("设置数据"), Tooltip("记录玩家的音效和背景音乐设置。")]
    public SettingArchiveData_1_4_0 SettingArchiveData;
    public void AddKillEnemyCount(int id,int count)
    {
        if (!StatisticsData.KillEnemys.ContainsKey(id))
        {
            StatisticsData.KillEnemys.Add(id, 0);
        }
        StatisticsData.KillEnemys[id] += count;
    }
    public int GetAtEnemyCount(int id)
    {
        return StatisticsData.KillEnemys[id];
    }


    
    // /// <summary>
    // /// 存档数据升级
    // /// </summary>
    // /// <param name="archiveData">1.3.11存档数据</param>
    // /// <returns>1.4.0存档数据</returns>
    // public static ArchiveData_1_4_0 ArchiveUpgrade(ArchiveData archiveData)
    // {
    //     ArchiveData_1_4_0 archiveData140 = new ArchiveData_1_4_0();
    //     //临时数据
    //     archiveData140.TemporaryData = new TemporaryData_1_4_0();
    //     archiveData140.TemporaryData.UnlockedItems.AddRange(archiveData.TemporaryData.UnlockedItems);
    //     archiveData140.TemporaryData.UnlockItems.AddRange(archiveData.TemporaryData.UnlockItems);
    //     archiveData140.TemporaryData.AddGoldValue = archiveData.TemporaryData.AddGoldValue;
    //     //全局数据
    //     archiveData140.GlobalData = new GlobalData_1_4_0();
    //     archiveData140.GlobalData.Gold = archiveData.Gold;
    //     archiveData140.GlobalData.Fire = 24;
    //     archiveData140.GlobalData.MaxFire = 12;
    //     archiveData140.GlobalData.FireRecoveryTime = -1;
    //     archiveData140.GlobalData.OneFireRecoveryNeedTime = 300;
    //     // foreach (KeyValuePair<int,int> keyValuePair in archiveData.SelectHeroSchoolId)
    //     // {
    //     //     archiveData140.GlobalData.SelectHeroSchoolId.Add(keyValuePair.Key,keyValuePair.Value);
    //     // }
    //     foreach (KeyValuePair<int,int> keyValuePair in archiveData.UnLockHeros)
    //     {
    //         archiveData140.GlobalData.UnLockHeros.Add(keyValuePair.Key);
    //     }
    //     // archiveData140.GlobalData.UnLockHeroSchools.AddRange(archiveData.UnLockHeroSchools);
    //     archiveData140.GlobalData.UnLockItems.AddRange(archiveData.UnLockItems);
    //     archiveData140.GlobalData.LastSelectHeroID = archiveData.LastSelectHeroID;
    //     archiveData140.GlobalData.ThroughTutorial = archiveData.ThroughTutorial;
    //     //战斗数据
    //     if (archiveData.BattleData == null)
    //     {
    //         archiveData140.BattleData = null;
    //     }
    //     else
    //     {
    //         archiveData140.BattleData = new BattleArchiveData_1_4_0();
    //         archiveData140.BattleData.ChapterData = new BattleArchiveChapterData_1_4_0();
    //         archiveData140.BattleData.ChapterData.CurrentCPId = archiveData.BattleData.ChapterData.CurrentCPId;
    //         archiveData140.BattleData.ChapterData.CurrentLevelIndex = archiveData.BattleData.ChapterData.CurrentLevelIndex;
    //         archiveData140.BattleData.ChapterData.CurrentRoomIndex = archiveData.BattleData.ChapterData.CurrentRoomIndex;
    //         archiveData140.BattleData.PlayerData = new BattleArchivePlayerData_1_4_0();
    //         archiveData140.BattleData.PlayerData.Items.AddRange(archiveData.BattleData.PlayerData.Items);
    //         archiveData140.BattleData.PlayerData.CurrentGold = archiveData.BattleData.PlayerData.CurrentGold;
    //         archiveData140.BattleData.PlayerData.CurrentHp = archiveData.BattleData.PlayerData.CurrentHp;
    //         archiveData140.BattleData.PlayerData.HeroId = archiveData.BattleData.PlayerData.HeroData.HeroID;
    //         archiveData140.BattleData.PlayerData.HeroLevel = archiveData.BattleData.PlayerData.HeroData.Level;
    //     }
    //     //统计数据
    //     archiveData140.StatisticsData = new StatisticsData_1_4_0();
    //     foreach (var value in archiveData.StatisticsData.KillEnemys)
    //     {
    //         archiveData140.StatisticsData.KillEnemys.Add(value.Value.Id,value.Value.Count);
    //     }
    //     foreach (KeyValuePair<string,int> keyValuePair in archiveData.TodayStartGameTimes)
    //     {
    //         archiveData140.StatisticsData.TodayStartGameTimes.Add(keyValuePair.Key,keyValuePair.Value);
    //     }
    //     foreach (KeyValuePair<int,ChapterClearanceData> keyValuePair in archiveData.ChapterClearanceDatas)
    //     {
    //         archiveData140.StatisticsData.ChapterClearanceDatas.Add(keyValuePair.Key,keyValuePair.Value);
    //     }
    //     
    //     archiveData140.StatisticsData.AppStartTimes = archiveData.AppStartTimes;
    //     archiveData140.StatisticsData.StartGameTimes = archiveData.StartGameTimes;
    //     archiveData140.StatisticsData.GameTime = archiveData.GameTime;
    //     
    //     return archiveData140;
    // }
    //
    
    public static void NewArchiveData(Action<ArchiveData_1_4_0> callBack)
    {
        
        
        DataManager.Inst.PeekDefaultUnLockItems(delegate(List<int> ints)
        {
            ArchiveData_1_4_0 archiveData140 = new ArchiveData_1_4_0();
            
            //临时数据
            archiveData140.TemporaryData = new TemporaryData_1_4_0();
            archiveData140.TemporaryData.UnlockedItems.Clear();
            archiveData140.TemporaryData.UnlockItems.Clear();
            archiveData140.TemporaryData.AddDiamondValue = 0;
            archiveData140.TemporaryData.AddSoulValue = 0;
            //全局数据
            archiveData140.GlobalData = new GlobalData_1_4_0();
            archiveData140.GlobalData.Diamond = 0;
            archiveData140.GlobalData.Soul = 0;
            archiveData140.GlobalData.Exp = 0;
            archiveData140.GlobalData.Fire = 12;
            archiveData140.GlobalData.MaxFire = 6;
            archiveData140.GlobalData.FireRecoveryTime = -1;
            archiveData140.GlobalData.OneFireRecoveryNeedTime = 7200;
            archiveData140.GlobalData.UnLockHeros.Add(1101);
            // ReSharper disable once PossibleInvalidOperationException
            archiveData140.GlobalData.HeroUpgradeDatas.Add(1101,new HeroUpgradeInfo());
            archiveData140.GlobalData.HeroUpgradeDatas[1101].SetHeroUpgradeData(DataManager.Inst.GetHeroUpgradeData(1101,1,0).Value);
            archiveData140.GlobalData.LastSelectHeroID = 1101;
            archiveData140.GlobalData.LastSelectChapterId = 1;
            archiveData140.GlobalData.ThroughTutorial = false;
            archiveData140.GlobalData.UnLockItems = ints;
            //战斗数据
            archiveData140.BattleData = null;
            //统计数据
            archiveData140.StatisticsData = new StatisticsData_1_4_0();
            archiveData140.StatisticsData.KillEnemys.Clear();
            archiveData140.StatisticsData.TodayStartGameTimes.Clear();
            archiveData140.StatisticsData.AppStartTimes = 0;
            archiveData140.StatisticsData.StartGameTimes = 0;
            archiveData140.StatisticsData.GameTime = 0;
            archiveData140.StatisticsData.ChapterClearanceDatas.Clear();
            // archiveData140.StatisticsData.ChapterClearanceDatas.Add(0,new ChapterClearanceData(){Count = 1}); 跳过新手章节
            archiveData140.SettingArchiveData = new SettingArchiveData_1_4_0();
            archiveData140.SettingArchiveData.Sound = true;
            archiveData140.SettingArchiveData.Bgm = true;
            
            callBack?.Invoke(archiveData140);
        });
    }
}

[Serializable]
public class GlobalData_1_4_0
{
    //火种恢复剩余时间
    public long FireTimeLeft
    {
        get
        {
            if (FireRecoveryTime == -1)
            {
                return -1;
            }

            // OneFireRecoveryNeedTime = 5;//TEST
            
            long currentTime = DateTime.Now.Ticks / 10000000;
            long elapsedTime = currentTime - FireRecoveryTime;
            if (elapsedTime>OneFireRecoveryNeedTime)
            {
                var fireCount = Math.Floor((float)elapsedTime/OneFireRecoveryNeedTime);
                Fire += (int)fireCount;
                elapsedTime %= OneFireRecoveryNeedTime;
                if (Fire >= MaxFire)
                {
                    Fire = MaxFire;
                    FireRecoveryTime = -1;
                }
                else
                {
                    FireRecoveryTime = DateTime.Now.Ticks / 10000000;
                }
            }
            return OneFireRecoveryNeedTime - elapsedTime;
        }
    }

    public int CurrentLevel
    {
        get
        {
            int level = 0;
            int currentExp = Exp;
            var expLevelData = DataManager.Inst.ExpLevelData;
            while (expLevelData.ContainsKey(level))
            {
                if (currentExp >= expLevelData[level].UpgradeNeedExp)
                {
                    currentExp -= expLevelData[level].UpgradeNeedExp;
                    level++; 
                }
                else
                {
                    break;
                }
            }
            return level;
        }
    }
    public float CurrentLevelExpPercentage
    {
        get
        {
            float value;
            int level = 0;
            int currentExp = Exp;
            var expLevelData = DataManager.Inst.ExpLevelData;
            while (expLevelData.ContainsKey(level) && currentExp >= expLevelData[level].UpgradeNeedExp)
            {
                currentExp -= expLevelData[level].UpgradeNeedExp;
                level++;
            }

            if (expLevelData.ContainsKey(level))
            {
                value = (float) currentExp / (float) expLevelData[level].UpgradeNeedExp;
            }
            else
            {
                value = 1;
            }

            return value;
        }
    }

    //经验值
    [LabelText("累计经验"), Tooltip("账号累计经验值，用于计算账号等级和当前等级经验进度。")]
    public int Exp;
    //魂
    [LabelText("魂"), Tooltip("账号持有的魂，可用于英雄培养等长期成长。")]
    public int Soul;
    //钻石
    [LabelText("钻石"), Tooltip("账号持有的钻石，用于游戏内资源消耗。")]
    public int Diamond;
    
    //火种
    [LabelText("当前火种"), Tooltip("当前可用于开始游戏的火种数量。")]
    public int Fire;
    //最大火种
    [LabelText("火种上限"), Tooltip("火种恢复所使用的上限值。")]
    public int MaxFire;
    //火种开始恢复时间,-1为满了没有在恢复
    [LabelText("火种恢复起始时间"), Tooltip("Unix 秒时间戳；-1 表示当前没有进行火种恢复。")]
    public long FireRecoveryTime;
    //一个火种恢复所需要的时间 - 秒
    [LabelText("恢复一个火种所需秒数"), Tooltip("火种每恢复一个所需的时间，单位为秒。")]
    public int OneFireRecoveryNeedTime;
    
    //解锁的英雄
    [LabelText("已解锁英雄 ID"), Tooltip("已经永久解锁的英雄 ID 列表。可在“英雄预览”页查看对应名称和预制体。")]
    public List<int> UnLockHeros = new List<int>();
    //已经解锁的物品
    [LabelText("已解锁物品 ID"), Tooltip("已经永久解锁、可进入物品池的物品 ID 列表。可在“物品预览”页查看对应配置。")]
    public List<int> UnLockItems = new List<int>();
    
    // key = heroId
    [LabelText("英雄培养数据"), Tooltip("以英雄 ID 为键，保存各英雄当前的等级、品质和已投入经验。")]
    public Dictionary<int,HeroUpgradeInfo> HeroUpgradeDatas = new Dictionary<int, HeroUpgradeInfo>();
    
    //最后一次选择的英雄ID
    [LabelText("上次选择英雄 ID"), Tooltip("下次进入主界面时默认选中的英雄 ID。")]
    public int LastSelectHeroID ;
    [LabelText("上次选择章节 ID"), Tooltip("下次开始游戏时默认选择的章节 ID。")]
    public int LastSelectChapterId;
    [LabelText("已完成新手教程"), Tooltip("标记玩家是否完成新手教程；关闭后启动流程可能重新进入教程。")]
    public bool ThroughTutorial;



    public int UnlockRuneCount
    {
        get
        {
            int count = 0;
            foreach (var runeItem in UnlockRuneItem)
            {
                count+=runeItem.Value;
            }
            return count;
        }
    }

    //Key = itemId,Value = Count
    [LabelText("已解锁符文"), Tooltip("以符文物品 ID 为键，记录每种符文的拥有数量。")]
    public Dictionary<int,int> UnlockRuneItem = new Dictionary<int, int>();
    
    [LabelText("已完成任务 ID"), Tooltip("已完成并领取奖励的任务 ID 列表。")]
    public List<int> MissionCompletedIdList = new List<int>();

    [LabelText("已领取章节魂奖励 ID"), Tooltip("已经领取过章节免费魂奖励的章节 ID 列表。")]
    public List<int> ReceiveChapterFreeSoul = new List<int>();
    
    [LabelText("已领取怪物图鉴奖励 ID"), Tooltip("已经领取过击杀奖励的怪物 ID 列表。")]
    public List<int> ReceiveEnemyIllustrated = new List<int>();
    [LabelText("已领取物品图鉴奖励 ID"), Tooltip("已经领取过图鉴奖励的物品 ID 列表。")]
    public List<int> ReceiveItemIllustrated = new List<int>();
    [LabelText("已领取流派图鉴奖励 ID"), Tooltip("已经领取过图鉴奖励的物品流派 ID 列表。")]
    public List<int> ReceiveItemSchoolIllustrated = new List<int>();
    //HeroId,LevelList
    [LabelText("已领取英雄技能奖励"), Tooltip("按英雄 ID、品质等级和英雄等级记录已领取的技能奖励。")]
    public Dictionary<int,Dictionary<int,List<int>>> ReceiveHeroSkill = new Dictionary<int, Dictionary<int,List<int>>>();
    
    [LabelText("已播放解锁系统动画"), Tooltip("记录已播放过解锁提示动画的系统，避免重复展示。")]
    public List<UnlockSystemType> PlayedUnlockSystemAnimaton = new List<UnlockSystemType>();
}

[Serializable]
public class HeroUpgradeInfo
{
    [LabelText("已投入经验"), Tooltip("当前保存在该英雄培养数据中的经验值。")]
    public int UseingExp;
    [LabelText("英雄 ID"), Tooltip("此培养数据所属的英雄 ID。")]
    public int HeroId;
    [LabelText("品质等级"), Tooltip("英雄当前品质等级，影响可用技能和成长上限。")]
    public int ColorLevel;
    [LabelText("英雄等级"), Tooltip("英雄当前培养等级。")]
    public int Level;

    public void SetHeroUpgradeData(HeroUpgradeData value)
    {
        HeroId = value.HeroId;
        ColorLevel = value.ColorLevel;
        Level = value.Level;
    }
    public HeroUpgradeData GetHeroUpgradeData()
    {
        return DataManager.Inst.GetHeroUpgradeData(HeroId, ColorLevel, Level).Value;
    }
}

public enum UnlockSystemType
{
    Rune,
    Event,
    Hero,
}

[Serializable]
public class TemporaryData_1_4_0
{
    //上一场战斗中解锁的Item
    [LabelText("本局新解锁物品 ID"), Tooltip("上一场战斗中新解锁、尚待结算展示的物品 ID。")]
    public List<int> UnlockItems = new List<int>();
    //上一场战斗中添加的
    [LabelText("本局获得钻石"), Tooltip("上一场战斗结算时新增的钻石数量。")]
    public int AddDiamondValue;
    //上一场战斗中添加的
    [LabelText("本局获得魂"), Tooltip("上一场战斗结算时新增的魂数量。")]
    public int AddSoulValue;
    //上一场战斗中添加的
    [LabelText("本局获得经验"), Tooltip("上一场战斗结算时新增的经验数量。")]
    public int AddExpValue;
    //已经解锁的物品 - 每次战斗之前缓存一份，防止随机出战斗过程中解锁的Item
    [LabelText("本局可用物品 ID"), Tooltip("本局开始时复制的已解锁物品列表，战斗中用它决定可用物品池。")]
    public List<int> UnlockedItems = new List<int>();
}


//统计数据
[Serializable]
public class StatisticsData_1_4_0
{
    //App启动次数
    [LabelText("应用启动次数"), Tooltip("存档记录的游戏启动总次数。")]
    public int AppStartTimes;
    //开始游戏次数
    [LabelText("开始游戏次数"), Tooltip("累计开始战斗的总次数。")]
    public int StartGameTimes;
    [LabelText("累计游戏时长"), Tooltip("累计游戏时长，单位为秒。")]
    public float GameTime;
    [LabelText("每日开始次数"), Tooltip("以日期字符串为键，记录每天开始战斗的次数。")]
    public Dictionary<string,int> TodayStartGameTimes = new Dictionary<string, int>();
    /// <summary>
    /// 击杀多怪物数量，Key：ID，Value：Count
    /// </summary>
    [LabelText("怪物击杀数"), Tooltip("以怪物 ID 为键，记录对应怪物的累计击杀数量。")]
    public Dictionary<int, int> KillEnemys = new Dictionary<int, int>();
    //章节通关信息
    [LabelText("章节通关记录"), Tooltip("以章节 ID 为键，记录最高进度、通关次数和到达层数。")]
    public Dictionary<int,ChapterClearanceData> ChapterClearanceDatas = new Dictionary<int, ChapterClearanceData>();
    //获得过的道具
    [LabelText("获得过的物品 ID"), Tooltip("记录玩家在战斗中获得过的物品 ID，用于物品图鉴统计。")]
    public List<int> GetItemList = new List<int>();
    [LabelText("解锁过的物品流派 ID"), Tooltip("记录已经解锁的物品流派 ID。")]
    public List<int> UnlockItemSchool= new List<int>();
    /// <summary>
    /// 击杀的怪物数量总和
    /// </summary>
    public int AllKillEnemyCount
    {
        get
        {
            int count = 0;
            foreach (var enemyCount in KillEnemys)
            {
                count += enemyCount.Value;
            }
            return count;
        }
    }
}

public class SettingArchiveData_1_4_0
{
    [LabelText("音效开关"), Tooltip("是否播放游戏音效。")]
    public bool Sound;
    [LabelText("背景音乐开关"), Tooltip("是否播放背景音乐。")]
    public bool Bgm;
}


#region BattleArchiveData

[Serializable]
public class BattleArchiveData_1_4_0:IArchiveBattleData
{
    [LabelText("战斗 GUID"), Tooltip("当前续档战斗的唯一标识。")]
    public Guid Guid;
    [LabelText("章节与房间"), Tooltip("续档时所在的章节、关卡和房间索引。")]
    public BattleArchiveChapterData_1_4_0 ChapterData;
    [LabelText("玩家状态"), Tooltip("续档时玩家携带的英雄、生命值、金币和物品。")]
    public BattleArchivePlayerData_1_4_0 PlayerData;
}
[Serializable]
public class BattleArchiveChapterData_1_4_0
{
    [LabelText("章节 ID"), Tooltip("续档战斗所在的章节 ID。")]
    public int CurrentCPId;
    [LabelText("关卡索引"), Tooltip("续档战斗所在关卡在章节中的索引。")]
    public int CurrentLevelIndex;
    [LabelText("房间索引"), Tooltip("续档战斗所在房间在关卡中的索引。")]
    public int CurrentRoomIndex;
}
[Serializable]
public class BattleArchivePlayerData_1_4_0
{
    [LabelText("当前金币"), Tooltip("续档时玩家在本局持有的金币。")]
    public int CurrentGold;
    [LabelText("英雄 ID"), Tooltip("续档时玩家使用的英雄 ID。")]
    public int HeroId;
    [LabelText("当前生命值"), Tooltip("续档时玩家剩余生命值。")]
    public float CurrentHp;
    [LabelText("携带物品 ID"), Tooltip("续档时玩家持有的物品 ID 列表。")]
    public List<int> Items = new List<int>();
}

#endregion
