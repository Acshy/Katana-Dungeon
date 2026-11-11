using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UI_PlayerBar : MonoBehaviour
{
    [SerializeField] private AnimationCurve scaleAnimationCurve;
    [Header("PlayerHp")]
    [SerializeField] private RectTransform hpBarGroup;
    [SerializeField] private Slider hpBar;
    [SerializeField] private Slider hpLerpBar;
    [SerializeField] private Text hpText;
    [SerializeField] private Image hpBarFill;
    [SerializeField] private float hpLerpSpeed;
    [Header("PlayerPower")]
    [SerializeField] private Slider powerBar;
    [SerializeField] private Text powerBarText;
    [SerializeField] private Image powerBarFill;
    private Color powerBarColor;

    [Header("PlayerExp")]
    [SerializeField] private Slider expBar;
    [SerializeField] private Text levelText;
    private int lastlevel;

    [Header("奇遇")]
    [SerializeField] private Slider fortunesBar;
    [SerializeField] private DOTweenAnimation fortunesBarAnimation;
    private void Awake()
    {
        EventManager.Inst.AddEvent(EventName.OnRoleInjured, OnPlayerInjured);
        EventManager.Inst.AddEvent(EventName.OnRoleSkillPowerChange, OnRoleSkillPowerChange);
        EventManager.Inst.AddEvent(EventName.NeedPower, PowerBarShake);
        // EventManager.Inst.AddEvent(EventName.UpdateExp, UpdateExpBar);
        EventManager.Inst.AddEvent(EventName.FortuneFull, PlayFortuneBarAnimation);
        EventManager.Inst.AddEvent(EventName.FortuneClear,ClearFortuneBar);
        powerBarColor = powerBarFill.color;
    }

    private void OnEnable()
    {
        StartCoroutine(init());
    }

    private void OnDestroy()
    {
        EventManager.Inst.RemoveEvent(EventName.OnRoleInjured, OnPlayerInjured);
        EventManager.Inst.RemoveEvent(EventName.OnRoleSkillPowerChange, OnRoleSkillPowerChange);
        EventManager.Inst.RemoveEvent(EventName.NeedPower, PowerBarShake);
        // EventManager.Inst.RemoveEvent(EventName.UpdateExp, UpdateExpBar);
        EventManager.Inst.RemoveEvent(EventName.FortuneFull,PlayFortuneBarAnimation);
        EventManager.Inst.RemoveEvent(EventName.FortuneClear,ClearFortuneBar);
    }

    public void PlayFortuneBarAnimation(string  arg,object obj)
    {
        fortunesBarAnimation.DOPlayAllById("1");
        
    }
    public void ClearFortuneBar(string arg, object obj)
    {
        fortunesBarAnimation.DORewindAllById("1");
    }
    private Tweener powerBarTweener;
    //    private Tweener powerShake;
    private void PowerBarShake(string arg1, object arg2)
    {
        if (powerBarTweener != null)
        {
            powerBarTweener.Kill(true);
            //            powerShake.Kill(true);
        }
        powerBar.transform.localScale = Vector3.one;
        //        powerBar.transform.localRotation = new Quaternion(0,0,0,0);
        powerBarTweener = powerBarFill.DOColor(new Color(1, 0, 0), 0.3f).SetEase(scaleAnimationCurve);
        //        powerBarTweener = powerBar.transform.DOScale(Vector3.one * 1.2f, 0.3f).SetEase(powerAnim);
        //        powerShake = powerBar.transform.DOShakeRotation(0.3f, 30);
    }

    private Tweener scaleTweener;
    private Tweener colorTweener;
    private void OnPlayerInjured(string arg1, object id)
    {
        if (BattleManager.Inst.CurrentPlayer == null)
        {
            return;
        }
        var roleid = ((RoleInjuredInfo)id).RoleId;
        var dmgValue = ((RoleInjuredInfo)id).Dmg.DmgValue;
        if (roleid != BattleManager.Inst.CurrentPlayer.TemporaryId)
        {
            return;
        }
        float ScalePara = (dmgValue / BattleManager.Inst.CurrentPlayer.MaxHp)<0.05f?1.1f:1.5f;

        hpBar.transform.localScale = Vector3.one;
        if (scaleTweener != null)
        {
            scaleTweener.Kill(true);
        }
        scaleTweener = hpBar.transform.DOScale(Vector3.one * ScalePara, 0.3f).SetEase(scaleAnimationCurve);

        hpBarFill.color = new Color(0.36f, 0.72f, 0.2f);
        if (colorTweener != null)
        {
            colorTweener.Kill(true);
        }
        colorTweener = hpBarFill.DOColor(new Color(1f, 0, 0), 0.3f).SetEase(scaleAnimationCurve);

    }

    public void SetSize()
    {

        int width = 280;
        int height = 50;
        float maxHp = BattleManager.Inst.CurrentPlayer.MaxHp;
        if (maxHp < 540)
        {
            width = (int)(maxHp * 0.6f);
        }
        else if (maxHp < 1000)
        {
            width = (int)(maxHp * 0.4) + 104;
        }
        else
        {
            width = (int)(maxHp * 0.08f) + 424;
        }

        hpBarGroup.sizeDelta = new Vector2(width, height);
    }
    void Update()
    {
        if (BattleManager.Inst.CurrentPlayer != null)
        {
            hpBar.value = BattleManager.Inst.CurrentPlayer.CurrentHp / BattleManager.Inst.CurrentPlayer.MaxHp;
            hpLerpBar.value = Mathf.Lerp(hpLerpBar.value, hpBar.value, hpLerpSpeed);
            hpText.text = Mathf.CeilToInt(BattleManager.Inst.CurrentPlayer.CurrentHp) + " / " + ((int)BattleManager.Inst.CurrentPlayer.MaxHp);
            //SetSize();
            if (powerBar.value < BattleManager.Inst.CurrentPlayer.RollNeedPower/BattleManager.Inst.CurrentPlayer.MaxPower)
                powerBarFill.color = new Color(0.8f, 0.2f, 0);
            else
                powerBarFill.color = powerBarColor;

            // fortunesBar.value = BattleManager.Inst.CurrentFortunes / (float) BattleManager.Inst.MaxFortunes;
        }
    }

    IEnumerator init()
    {
        while (BattleManager.Inst.CurrentPlayer == null)
        {
            yield return null;
        }

        OnRoleSkillPowerChange(null, null);
    }
    private void OnRoleSkillPowerChange(string arg1, object arg2)
    {
        if (BattleManager.Inst.CurrentPlayer == null)
        {
            return;
        }
        powerBar.value = BattleManager.Inst.CurrentPlayer.CurrentSkillPower / (float)BattleManager.Inst.CurrentPlayer.MaxSkillPower;
        powerBarText.text = $"{(int)BattleManager.Inst.CurrentPlayer.CurrentSkillPower}/{(int)BattleManager.Inst.CurrentPlayer.MaxSkillPower}";
    }
    // private void UpdateExpBar(string arg1, object arg2)
    // {
    //     // PlayerExperience playerExperience = BattleManager.Inst.CurrentPlayer.playerExperience;
    //     if (playerExperience == null)
    //         return;
    //
    //     bool isLevelUp = playerExperience.CurrentLevel > lastlevel;
    //     float percent = playerExperience.NextLevelPercent;
    //     if (isLevelUp)
    //     {
    //         lastlevel = playerExperience.CurrentLevel;
    //         levelText.text = "Lv " + lastlevel.ToString();
    //         expBar.transform.DOScale(Vector3.one * 1.1f, 0.3f).SetEase(scaleAnimationCurve).SetUpdate(true);
    //
    //         expBar.DOValue(1, 0.2f).SetUpdate(true).OnComplete(() =>
    //         {
    //             expBar.value = 0;
    //             expBar.DOValue(percent, 0.3f).SetUpdate(true);
    //
    //         });
    //     }
    //     else
    //     {
    //         expBar.DOValue(percent, 0.3f).SetUpdate(true);
    //     }
    // }
}
