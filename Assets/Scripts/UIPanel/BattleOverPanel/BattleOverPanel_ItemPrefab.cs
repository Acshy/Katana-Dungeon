using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class BattleOverPanel_ItemPrefab : MonoBehaviour
{
    [SerializeField]
    private Image icon;

    private Item itemData;
    
    public void Init(Item item)
    {
        itemData = item;
        icon.sprite = item.Icon;
        transform.localScale = Vector3.zero;
        this.gameObject.SetActive(true);
    }

    public void Show(float value)
    {
        transform.DOScale(Vector3.one, 0.3f).SetDelay(value).Delay();
    }
    
    public void OnBtnClick()
    {
        if (itemData == null)
        {
            return;
        }
        var item = DataManager.Inst.GetItemScrObj(itemData.ID);
        UIManager.Inst.Open("InfoPanel",false,item);
    }
}
