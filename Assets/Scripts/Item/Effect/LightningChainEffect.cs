using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningChainEffect : ItemEffect
{
    private int catapultTimes;
    private float distance;
    private int attackPower;
    private float CatapultAttackPowerPercentage;

    private int attackTimes;
    List<string> attackList = new List<string>();

    public bool isSurroundObj;
    public LightningChainEffect(int catapultTimes, float distance, int attackPower,float  CatapultAttackPowerPercentage,bool isSurroundObj = false)
    {
        this.catapultTimes = catapultTimes;
        this.distance = distance;
        this.CatapultAttackPowerPercentage = CatapultAttackPowerPercentage;
        this.attackPower = attackPower;
        this.isSurroundObj = isSurroundObj;
    }

    private UVChainLightning prefab;
    
    public override void Awake(RoleItemController rpe)
    {
        base.Awake(rpe);
        attackPower += (int)(roleController.OriginalAttackPower * CatapultAttackPowerPercentage);
        //GameObject fx = ResourcesManager.Inst.GetAsset<GameObject>("Assets/Arts/Vfxs/LightingChain/ChainLightning.prefab");
        prefab = (Resources.Load("ChainLightning") as GameObject).GetComponent<UVChainLightning>();
    }

    public override void Destroy(RoleItemController rpe)
    {
        base.Destroy(rpe);
        
    }

    private Coroutine _coroutine;

    public override void TriggerEffect(ItemEffectTriggerValue? value)
    {
        base.TriggerEffect(value);
        if (value.HasValue && string.IsNullOrEmpty(value.Value.TargetId))
        {
            Trigger(value.Value.TargetId);
        }
        else
        {
            RoleController target = BattleTool.FindNearestEnemy(roleController, distance);
            if (target)
            {
                Trigger(target.TemporaryId);
            }
        }
    }

    void Trigger(string targetId)
    {
        if (BattleManager.Inst.EnemyTeam.ContainsKey(targetId))
        {
            var enemy = BattleManager.Inst.EnemyTeam[targetId];
            if (enemy != null && !enemy.IsDie)
            {
                attackList.Clear();
                attackTimes = 0;

                bool isOk = false;
                if (isSurroundObj)
                {
                    isOk = true;
                }
                else
                {
                    foreach (KeyValuePair<string, RoleController> enemyTeam in BattleManager.Inst.EnemyTeam)
                    {
                        if (attackList.Contains(enemyTeam.Value.TemporaryId))
                        {
                            continue;
                        }

                        if (enemyTeam.Value.TemporaryId == enemy.TemporaryId)
                        {
                            isOk = true;
                            break;
                            continue;
                        }
                        float dis = (enemyTeam.Value.transform.position - enemy.transform.position).magnitude;
                        if (dis <= distance)
                        {
                            isOk = true;
                            break;
                        }
                    }
                }

                if (isOk)
                {
                    // if (_coroutine != null)
                    // {
                    //     GameManager.Inst.StopCoroutine(_coroutine);
                    // }
                    HitTarget(enemy);
                    
                    _coroutine = GameManager.Inst.StartCoroutine(LightningChainAttack(enemy));
                }
            }
        } 
    }

    IEnumerator LightningChainAttack(RoleController hitTarget)
    {
        //        yield return new WaitForSeconds(0.3f);
        
        List<Transform> hitList = new List<Transform>();
        List<UVChainLightning> lineList = new List<UVChainLightning>();
        
        hitList.Add(BattleManager.Inst.CurrentPlayer.transform);
        hitList.Add(hitTarget.transform);
        if (!isSurroundObj)
        {
            lineList.Add(GameObject.Instantiate(prefab));
            lineList[lineList.Count-1].SetTarget(BattleManager.Inst.CurrentPlayer.transform,hitTarget.transform);
        }
        
        bool isAttackEnemy = false;
        
        while (attackTimes < catapultTimes)
        {
            yield return new WaitForSeconds(0.3f);
            isAttackEnemy = false;
            foreach (KeyValuePair<string, RoleController> enemy in BattleManager.Inst.EnemyTeam)
            {
                if (attackList.Contains(enemy.Value.TemporaryId))
                {
                    continue;
                }

                if (enemy.Value.IsDie)
                {
                    continue;
                }
                
                
                float dis = (enemy.Value.transform.position - hitList[hitList.Count-1].position).magnitude;
                if (dis <= distance)
                {
                    isAttackEnemy = true;
                    hitList.Add(enemy.Value.transform);
                    HitTarget(enemy.Value);
                    lineList.Add(GameObject.Instantiate(prefab));
                    lineList[lineList.Count-1].SetTarget(hitList[hitList.Count-2],hitList[hitList.Count-1]);
                    break;
                }
            }

            if (!isAttackEnemy)
            {
                break;
            }
        }
        yield return new WaitForSeconds(0.3f);
        
        //回收闪电链
        for (int i = 0; i < lineList.Count; i++)
        {
            GameObject.Destroy(lineList[i].gameObject);
        }
    }

    void HitTarget(RoleController role)
    { 

        DamageInfo dmg = new DamageInfo(role.TemporaryId,attackPower, roleController, roleController.transform.position, DmgType.Thunder,
                   false, false,0,0);
        role.HpInjured(dmg);
        attackList.Add(role.TemporaryId);
        attackTimes++;
        EventManager.Inst.DistributeEvent(EventName.OnLightningChainHitRole,role.TemporaryId);
    }
 
}
