using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;

public class ChooseItemPanel : PanelBase
{
    [SerializeField]
    private ChooseItemPanel_ShowItem itemPanel;

    //[SerializeField]
    //private Text text;
    //[SlizeField]
    //private Text PanelName;


    private List<UI_ShopPanel_ShopItem> shopItemList = new List<UI_ShopPanel_ShopItem>();
    
    public void OnOpen()
    {
        DataManager.Inst.GetCpData(BattleManager.Inst.RuntimeData.CurrentChapterId).LoadChapterData(
            delegate(ChapterData data)
            {
                itemPanel.ShowItem(
                    data.ItemData.ArtifactPool,
                    ClosePanel
                    );
            });
    }


    public void ClosePanel()
    {
        UIManager.Inst.Close("ChooseItemPanel");
    }
    
}
