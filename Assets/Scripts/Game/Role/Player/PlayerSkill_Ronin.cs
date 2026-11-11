using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkill_Ronin : PlayerSkill
{
    public ParticleSystem FinishParticles;
    public AudioClip FinishSFX;
    public ParticleSystem OnHitParticles;
    public FeedBackObject OnHitFeedback;
    public FeedBackObject OnHitFeedback_Boss;

    public float MaxComboTime;
    private float comboTimer;
    public float MaxSkillComboTime;
    private float skillComboTimer;

    private List<RoleController> enemyForzen=new List<RoleController>();
    void FreezeTarget(float time)
    {
        List<RoleController> enemyList = BattleTool.GetEnemysInDistance(roleController, 2.5f);
        foreach (var enemyTarget in enemyList)
        {
            if (!enemyTarget)
                return;
            if(enemyForzen.Contains((enemyTarget)))
                return;
            enemyForzen.Add(enemyTarget);

            // enemyTarget.SetDizziness(true);
            enemyTarget.SetFreeze(true);
            enemyTarget.Animator.speed = 0;
            //调用反馈
            GameObject par = Instantiate(OnHitParticles.gameObject, enemyTarget.transform.position + Vector3.up, Quaternion.identity);

            float freezeTime = time;
            if (enemyTarget.roleTeamType == RoleTeamType.EliteEnemy ||
            enemyTarget.roleTeamType == RoleTeamType.Enemy_Boss)
            {
                freezeTime *= 0.5f;
                if (OnHitFeedback_Boss)
                {
                    FeedbackManager.Inst.UseFeedBack(enemyTarget, OnHitFeedback_Boss);
                }
            }
            else
            {
                FeedbackManager.Inst.UseFeedBack(enemyTarget, OnHitFeedback);
            }
            StartCoroutine(UnFreezeTarget(enemyTarget, freezeTime));
        }
    }
    IEnumerator UnFreezeTarget(RoleController enemyTarget, float time)
    {
        yield return new WaitForSeconds(time);

        // enemyTarget.SetDizziness(false);
        enemyTarget.SetFreeze(false);
        enemyTarget.Animator.speed = 1;
        GameObject par = Instantiate(FinishParticles.gameObject, enemyTarget.transform.position, Quaternion.identity);
        FinishSFX.Play();

        //这里变成暴击屏幕会白一下
        var dmg = new DamageInfo(enemyTarget.TemporaryId, roleController.AttackPower * 2f, roleController,
            roleController.transform.position, DmgType.Physical);
        enemyTarget.HpInjured(dmg);
        enemyForzen.Remove(enemyTarget);
        EventManager.Inst.DistributeEvent(EventName.OnRoleAttack, roleController.TemporaryId);
        EventManager.Inst.DistributeEvent(EventName.OnPlayerAttackHitEnemy, dmg);
    }

    void AttackCombo()
    {
        ((PlayerKatanaAttack)roleController.roleAttack).AccumlateAttack();
    }

    void SetCollisionWithEnemy(bool activeClollision)
    {
        
        roleController.gameObject.layer = LayerMask.NameToLayer("PlayerRoll");
    }

    protected override void AnimEvent(GameObject go, string eventName)
    {
        if (go != roleController.Animator.gameObject)
        {
            return;
        }

        if (eventName.Contains("Katana_FreezeAttackTargets_"))
        {
            eventName = eventName.Replace("Katana_FreezeAttackTargets_", "");
            float time = float.Parse(eventName);
            FreezeTarget(time);
        }
        else if (eventName.Contains("Katana_SecondBlink"))
        {
            if(roleColorLevel>=2)
                AttackCombo();
        }
        else if (eventName.Contains("Katana_SecondDamage"))
        {
            EventManager.Inst.DistributeEvent("DoBlinkDamage",roleController.AttackPower*2);
        }
    }
}

