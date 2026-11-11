using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public abstract class BaseRules : IBattleRules
{
    public bool IsLoadOver { get; protected set; }
    public abstract LevelStructType BattleRulesType { get; }
    public bool IsArchiveBattle { get; protected set; }
    public abstract RoomController CurrentRoom { get; }
    public abstract void StartGame();

    public abstract void LoadBattle(IBattleRulesData rulesData);

    public abstract void LoadBattle(IArchiveBattleData archiveBattleData);

    public abstract void EndBattle();

    public abstract void Update();

    
    protected BattleRuntimeBaseData RuntimeBaseData => BattleManager.Inst.RuntimeData;
    protected PlayerController CurrentPlayer => BattleManager.Inst.CurrentPlayer;
    protected CameraController CurrentCamera => BattleManager.Inst.CurrentCamera;
    protected HeroData CurrentHeroData => RuntimeBaseData.CurrentHeroData;
    
    //更新HeroData
    public virtual void UpdateHeroData()
    {
        
    }

    protected void CreateCamera(Action callBack)
    {
        if (CurrentCamera!=null)
        {
            GameObject.Destroy(CurrentCamera.gameObject);
        }
        BattleTool.CreatePlayerCamera(delegate(CameraController controller)
        {
            controller.SetTarget(CurrentPlayer.transform);
            controller.Distance = 40f;
            BattleManager.Inst.SetCamera(controller);
            callBack?.Invoke();
        });
    }
    protected void CreatePlayer(Action callback)
    {
        if (CurrentPlayer!=null)
        {
            GameObject.Destroy(CurrentPlayer.gameObject);
        }
        
        BattleTool.CreatePlayer(RuntimeBaseData.CurrentHeroData.HeroPrefabName,
            delegate(PlayerController controller)
            {
                GameObject startPoint = GameObject.Find("Entrance_Z");
                if (startPoint != null)
                {
                    controller.transform.position = startPoint.transform.position;// + new Vector3(0, 0, 0.5f);
                    controller.transform.rotation = Quaternion.identity;
                    
                }
                BattleManager.Inst.SetPlayer(controller);
                callback?.Invoke();
                
                controller.SetUniqueID(CurrentHeroData.HeroId);
                controller.GetComponent<PlayerSkill>()?.Init();
                controller.GetComponent<PlayerAppearance>()?.Init();
                
                //------------------------------------------------
                controller.Animator.transform.localPosition = new Vector3(0,15,0);
                ResourcesManager.Inst.GetAsset<GameObject>("Assets/AssetsPackage/CommonFX/VFX_PlayerLand.prefab", 
                    delegate(GameObject go)
                    {
                        controller.Animator.transform.DOLocalMove(Vector3.zero, 0.2f).OnComplete(() =>
                        {
                            var fxGo = GameObject.Instantiate(go);
                            fxGo.transform.position = controller.transform.position;
                            fxGo.gameObject.SetActive(true);
                            
                            
                            ResourcesManager.Inst.GetAsset<FeedBackObject>("Assets/AssetsPackage/ScriptObject_Combat/FeedBack/Feedback_Player_OnPlayerGround.asset", 
                                delegate(FeedBackObject data)
                                {
                                    FeedbackManager.Inst.UseFeedBack(controller,data);
                                });
                        }).SetDelay(0.3f).Delay();
                    });
                //------------------------------------------------
                
                EnvironmentManager.SetCurrentCharacterLight();
            });
        
        
    }

    protected void CreatePlayerAndCamera()
    {
        CreatePlayer(delegate
        {
            CreateCamera(delegate { IsLoadOver = true; });
        });
    }
    
    protected void PlayerAddGlobalRuneItem()
    {
        List<int> itemIds = new List<int>();
        foreach (var item in ArchiveManager.Inst.ArchiveData.GlobalData.UnlockRuneItem)
        {
            for (int i = 0; i < item.Value; i++)
            {
                itemIds.Add(item.Key);
            }
        }

        PlayerAddItem(itemIds);
    }
    
    protected void PlayerAddItem(List<int> itemIds)
    {
        foreach (int itemId in itemIds)
        {
            CurrentPlayer.roleItemController.AddItem(
                DataManager.Inst.ParsingItemObj(DataManager.Inst.GetItemScrObj(itemId)), isOk => { });
        }
    }

    protected void InitPlayerAttributes()
    {
        var heroUpgradeData = ArchiveManager.Inst.GetHeroUpgradeData(CurrentHeroData.HeroId).GetHeroUpgradeData();
        CurrentPlayer.InitAttacker(heroUpgradeData.AttackPower,heroUpgradeData.AoeMagnification);
        CurrentPlayer.InitMaxHp(heroUpgradeData.MaxHp);
    }

    protected void AddPlayerItem()
    {
        var heroUpgradeData = ArchiveManager.Inst.GetHeroUpgradeData(CurrentHeroData.HeroId).GetHeroUpgradeData();
        PlayerAddItem(heroUpgradeData.ItemIds);
    }
}
