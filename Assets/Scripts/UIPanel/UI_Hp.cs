using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
public class UI_Hp : MonoBehaviour
{
    [SerializeField]
    private Slider slider;
    [SerializeField]
    private Vector2 offset;
    [SerializeField]
    private Slider LerpSlider;
    public float LerpMul = 0.03f;
    float StartLerpDelay = 0.1f;
    bool isDelaying = false;
    float tiemr = -1;
    RectTransform rect;
    private RoleController roleController;
    private float maxHp;

    CanvasGroup canvasGroup;



    public void SetTarget(RoleController roleController_)
    {

        roleController = roleController_;
        maxHp = roleController.MaxHp;
        rect = GetComponent<RectTransform>();
        /*if (roleController.tag == "Player")
        {
            Destroy(gameObject);
        }*/
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 1;

        SetSize(roleController);
        SetColor(roleController);

        EventManager.Inst.AddEvent(EventName.OnRoleInjured, OnRoleInjured);

        Update();

        gameObject.SetActive(true);
        //canvasGroup.alpha = 0;
        
        if (roleController.roleTeamType != RoleTeamType.Player)
        {
            canvasGroup.alpha = 0;
        }
        
    }
    protected virtual void OnRoleInjured(string arg1, object arg2)
    {
        var data = (RoleInjuredInfo)arg2;
        if (data.RoleId != roleController.TemporaryId)
        {
            return;
        }

        if (data.RoleId != BattleManager.Inst.CurrentPlayer.TemporaryId)
        {
            ShowHpBar();   
        }       

    }
    void SetColor(RoleController roleController)
    {
        if (roleController.roleTeamType == RoleTeamType.Enemy ||
       roleController.roleTeamType == RoleTeamType.Enemy_Boss ||
       roleController.roleTeamType == RoleTeamType.EliteEnemy)
        {
            slider.fillRect.GetComponent<Image>().color = Color.red;
        }
        else if (roleController.roleTeamType == RoleTeamType.Player)
        {
            // canvasGroup.alpha = 0; 
            slider.fillRect.GetComponent<Image>().color = new Color(0.3f, 0.7f, 0.2f);
        }

    }
    public void SetSize(RoleController roleController)
    {

        int width = 100;
        int height = 36;
        if (roleController.roleTeamType == RoleTeamType.Enemy ||
        roleController.roleTeamType == RoleTeamType.Enemy_Boss||
        roleController.roleTeamType == RoleTeamType.EliteEnemy)
        {
            if (maxHp < 100)
            {
                width = 36;
            }
            else if (maxHp < 900)
            {
                width = 120;
            }
            else if (maxHp < 2000)
            {
                width = 160;
            }
            else
            {
                height = 36;
                width = 240;
            }
        }
        else if (roleController.roleTeamType == RoleTeamType.Player)
        {
            height = 36;
            width = (int)(roleController.MaxHp/5);
            width = Mathf.Clamp(width,100,160);
        }
        rect.sizeDelta = new Vector2(width, height);
    }

    public void PlayerHpBarSetSize()
    {
        if (roleController == null)
            return;
        if (roleController.roleTeamType != RoleTeamType.Player)
            return;

        maxHp = roleController.MaxHp;

        int width = 160;
        int height = 36;
       

        DOTween.To(() => rect.sizeDelta, x => rect.sizeDelta = x, new Vector2(width, height), 1);

    }

    public void ShowHpBar()
    {
        if (canvasGroup == null)
        {
            return;
        }
        canvasGroup.alpha = 1; 
        if (DelayHideHpBarCoroutine != null)
        {
            StopCoroutine(DelayHideHpBarCoroutine);
        }
        DelayHideHpBarCoroutine = StartCoroutine(DelayHideHpBar(2.5f));
    }

    Coroutine DelayHideHpBarCoroutine;
    IEnumerator DelayHideHpBar(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        canvasGroup.DOFade(0,0.1f);
        DelayHideHpBarCoroutine = null;
    }

    public void FadeOff(float fadeTime)
    {
        transform.DOScale(transform.localScale * 3, fadeTime);
        canvasGroup.DOFade(0, fadeTime);
    }
    void Update()
    {
        if (roleController != null)
        {
            if (Camera.main != null)
            {
                Vector3 targetPos;
                if (roleController.roleNode.Head != null)
                {
                    targetPos = roleController.roleNode.Head.position;
                }
                else
                {
                    targetPos = roleController.transform.position;
                }
                transform.position = Camera.main.WorldToScreenPoint(targetPos) + new Vector3(offset.x, offset.y, 0);
            }

            //Lerp Slider
            slider.value = roleController.CurrentHp / roleController.MaxHp;
            if (LerpSlider.value > slider.value)
            {
                if (Time.time - tiemr > StartLerpDelay)
                {
                    LerpSlider.value -= Time.deltaTime * LerpMul;
                }
            }
            else
            {
                tiemr = Time.time;
                LerpSlider.value = slider.value;
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
