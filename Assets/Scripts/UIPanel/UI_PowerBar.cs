using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UI_PowerBar : MonoBehaviour
{
    [SerializeField]
    private Slider powerBar;
    [SerializeField]
    private Vector2 offset;
    Image barFill;
    RectTransform rect;
    RoleController roleController;
    bool onFade=false;
    public void SetTarget(RoleController roleController_)
    {

        roleController = roleController_;
        rect = GetComponent<RectTransform>();

        rect.sizeDelta = new Vector2(80, 10);
        barFill = powerBar.fillRect.GetComponent<Image>();
        barFill.color = new Color(1, 1, 1);
        gameObject.SetActive(true);
        Update();
    }

    void Show()
    {
        if (barFill.color.a > 0.9f)
            return;
       
        onFade = true;
        barFill.transform.DOScale(1, 0.1f);
        barFill.DOFade(1, 0.2f).SetEase(Ease.InCubic).OnComplete(()=>onFade=false);
    }

    void Hide()
    {
        if (barFill.color.a < 0.1f)
            return;
       
        
        onFade = true;
        barFill.transform.DOScale(1.2f, 0.1f);
        barFill.DOFade(0, 0.1f).OnComplete(() => onFade = false);
        
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (roleController == null)
        {
            Destroy(gameObject);
        }
        float value= roleController.CurrentPower / roleController.RollNeedPower;
        powerBar.value = value*value;

        if ( !onFade)
        {
            if (value > 0.96f)
            {
                Hide();
            }
            else
            {
                Show();
            }

        }
        
        if (Camera.main != null)
        {
            Vector3 targetPos;
            if (roleController==null || roleController.transform ==null)
            {
                return;
            }
            if (roleController.roleNode.Head != null )
            {
                targetPos = roleController.roleNode.Head.position;
            }
            else
            {
                targetPos = roleController.transform.position;
            }
            transform.position = Camera.main.WorldToScreenPoint(targetPos) + new Vector3(offset.x, offset.y, 0);
        }
       

    }
}
