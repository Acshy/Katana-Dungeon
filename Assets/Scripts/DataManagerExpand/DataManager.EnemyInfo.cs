using System;
using System.Collections.Generic;
using FlexFramework.Excel;
using UnityEngine;

public partial class DataManager : TSingleton<DataManager>
{
    private string EnemyInfoPath = "Assets/AssetsPackage/Table/CSV_EnemyInfo.csv";
    
    public Dictionary<int, EnemyInfoData> EnemyDatas { get; private set; }
    void InitEnemyInfo()
    {
        initNumber++;
        EnemyDatas = new Dictionary<int, EnemyInfoData>();
        ResourcesManager.Inst.GetAsset<TextAsset>(EnemyInfoPath, delegate(TextAsset infoText)
        {
            var enemyData = Document.Load(infoText.text);
            for (int i = 1; i < enemyData.Count; i++)
            {
                EnemyInfoData infoData = new EnemyInfoData();
                infoData.EnemyID = enemyData[i][0].Convert<int>();
                infoData.EnemyName = enemyData[i][1].String;
                infoData.EnemyIcon = enemyData[i][2].String;
                infoData.EnemyPrefabName = enemyData[i][3].String;
                infoData.EnemyTalk = enemyData[i][4].String;
                infoData.EnemyType = (EnemyType)Enum.Parse(typeof(EnemyType),enemyData[i][5].String);
                infoData.EnemyWeight = enemyData[i][6].Convert<int>();
                infoData.ShowInAtlas = enemyData[i][7].Convert<string>().ToLower() == "true";
                EnemyDatas.Add(infoData.EnemyID, infoData);
            }

            initNumber--;
        });
    }
    public EnemyInfoData GetEnemyInfo(int id)
    {
        if (!EnemyDatas.ContainsKey(id))
        { 
            Debug.LogError("表里找不到怪物：ID="+id);
            return null;
        }

        return EnemyDatas[id];
    }
    public EnemyInfoData GetEnemyInfo(string prefabName)
    {
        foreach (var enemy in EnemyDatas)
        {
            if(enemy.Value.EnemyPrefabName == prefabName)
            return enemy.Value;
        }
        Debug.LogError("表里找不到怪物："+prefabName);
        return null;
    }


}