using System;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
public class EnemyController : RoleController
{
    public override bool IsAcceptInput { get => true; }
    public override bool IsCanMove => !IsFastMoving && !IsDmging && !IsDie && !IsDizziness;//&& !IsAttacking;

    [LabelText("死亡之后需要停止的粒子")]
    public List<ParticleSystem> ToBeDestroidPar;
    [FormerlySerializedAs("TimeType")] public RoleTeamType TeamType = RoleTeamType.Enemy;
    //public ParticleSystem[] ProtectedParticles;


    public override void Init()
    {
        
        if (isInit)
        {
            return;
        }
        base.Init();
        if (UniqueID <= 0)
        {
            UniqueID = 1;
        }
        var enemyInfo = DataManager.Inst.GetEnemyInfo(UniqueID);
        switch (enemyInfo.EnemyType)
        {
            case EnemyType.Lv1:
            case EnemyType.Lv2:
            case EnemyType.Lv3:
                TeamType = RoleTeamType.Enemy;
                break;
            case EnemyType.Elite:
                TeamType = RoleTeamType.EliteEnemy;
                break;
            case EnemyType.Boss:
                TeamType = RoleTeamType.Enemy_Boss;
                break;
        }
        this.roleTeamType = TeamType;
        roleHealth.Init(roleHealth.IniHp);
        BattleManager.Inst.RoleRegistered(this);
    }

    public override void OnDeadEvent()
    {
        base.OnDeadEvent();
        
        if (ToBeDestroidPar != null)
        for (int i=0;i<ToBeDestroidPar.Count;i++)
        {
            if(ToBeDestroidPar[i]!=null)
                ToBeDestroidPar[i].Stop();
        }

        StartCoroutine(waitUnRegistered());
    }

    IEnumerator waitUnRegistered()
    {
        yield return null;
        // BattleManager.Inst.RoleUnRegistered(this);
    }
    public float GetRotateSpeed()
    {
        if ((roleMove as EnemyMove) == null)
            return -1;
        else
            return (roleMove as EnemyMove).RotateSpeed;
    }
    
    
    

    protected override void Update()
    {
        base.Update();
        if (!BattleManager.Inst.GameIsRuning)
            return;
        
        if (_isExitFloor)
        {
            _exitFloorTimer += Time.deltaTime;
            if (_exitFloorTimer > 2f)
            {
                DamageInfo dmg = new DamageInfo(
                    this.TemporaryId,
                    999999,
                    this,
                    new Vector3(transform.position.x,0,transform.position.z),
                    DmgType.Physical,
                    false,
                    false,
                    0,
                    0,
                    false,
                    this,
                    null,
                    null,
                    false
                );
                HpInjured(dmg);
                _exitFloorTimer = 0;
            }
        }
        else
        {
            _exitFloorTimer = 0;
        }
    }
    
     
}
