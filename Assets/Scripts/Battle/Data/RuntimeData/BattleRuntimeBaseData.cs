using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public abstract class BattleRuntimeBaseData: IBattleRuntimeData
{
    public abstract IBattleLevelStructData LevelStructData { get; }
    public abstract int Progress { get; }
    public abstract bool IsTutorial { get; }

    public abstract void SetLevelStructData(IBattleLevelStructData levelStructData);

    public abstract int CurrentChapterId { get; }

    public int ShopRefreshTimes;
    
    #region BattleGuid

    public Guid BattleGuid { get; private set; }
    public void SetBattleGuid(Guid guid)
    {
        BattleGuid = guid;
    }
    

    #endregion

    #region Hero相关信息

    //HeroId
    public int CurrentHeroId => _currentHeroId;
    private int _currentHeroId;


    //Hero对应等级的信息
    public HeroData CurrentHeroData => DataManager.Inst.GetHeroData(CurrentHeroId);

    public void SetHeroData(int id)
    {
        _currentHeroId = id;
    }

    #endregion

    #region 钱

    //钱
    public int CurrentGold => _currentGold;
    private int _currentGold;

    public void SetGold(int gold)
    {
        _currentGold = gold;
    }
    /// <summary>
    /// 添加金币
    /// </summary>
    public void AddGold(int value)
    {
        // if (value>0)
        // {
        //     //value = (int)(value * currentPlayer.AddGoldMagnification);
        //     //因为金币是小整数，所以用概率代替
        //     value = (int)(value + ((1+Random.value)<CurrentPlayer.AddGoldMagnification?1:0));
        // }
        
        _currentGold += value;
        if (CurrentGold < 0)
            _currentGold = 0;
        if (CurrentGold>999)
        {
            _currentGold = 999;
        }
        EventManager.Inst.DistributeEvent(EventName.OnAddMoney, value);
    }
    
    #endregion
    
    #region 击杀信息

    //已经击杀的小怪
    public int KillEnemyNumber => _killEnemyNumber;
    private int _killEnemyNumber;
    
    //已经击杀的精英怪
    public int KillSEnemyNumber => _killSEnemyNumber;
    private int _killSEnemyNumber;

    public void AddKillEnemyNumber(RoleTeamType roleTeamType)
    {
        switch (roleTeamType)
        {
            case RoleTeamType.Enemy:
                _killEnemyNumber++;
                break;
            case RoleTeamType.EliteEnemy:
                _killSEnemyNumber++;
                break;
        }
        
        // if (KillSEnemyNumber>1)
        // {
        //     _killSEnemyNumber = 1;
        //     Debug.LogError("检查Enemy计数器是否有错误。");
        //     Debug.LogError("错误怪物名字："+roleTeamType);
        // }
    }

    #endregion
    
    #region 增强怪物

    //当前强力怪的概率0-100
    private int _currentEnhancedEnemyProbability;

    public void ResetEnhancedEnemyProbability()
    {
        _currentEnhancedEnemyProbability = 0;
    }
    public virtual bool RandomEnhancedEnemy()
    {
        if (Random.Range(0, 100) < _currentEnhancedEnemyProbability)
        {
            _currentEnhancedEnemyProbability = 0;
            return true;
        }
        else
        {
            _currentEnhancedEnemyProbability += BattleManager.Inst.EnhancedEnemyAddProbability;
            return false;
        }
    }

    #endregion
    
    #region 房间模块

    //使用过的ConditionInfoId   房间模块
    public List<int> UsingConditionInfoIds = new List<int>();
    
    public void AddUsingConditionInfoId(int id)
    {
        UsingConditionInfoIds.Add(id);
    }

    public bool CheckContainsUsingConditionInfoId(int id)
    {
        return UsingConditionInfoIds.Contains(id);
    }

    #endregion
}