using System;
using System.Collections.Generic;

//不要出现自定义或Unity的引用类型
[Serializable]
public class RoomData
{
    public int CurrentFloorIndex;//第几层
    public int CurrentRoomIndex;//第几个房间
    public RoomType RoomType;//房间类型

    public string RoomPrefabName;
    
    public List<List<int>> EnemyWaves;//战斗房的Enemys，其他房间

    public int ConditionInfoId;//Event房间ID
    
    // 有且只有一个有参构造函数，不要扩展！！！！
    public RoomData(int floorIndex,int roomIndex, RoomType roomType,string roomPrefabName,List<List<int>> enemyWaves)
    {
        CurrentFloorIndex = floorIndex;
        
        CurrentRoomIndex = roomIndex;
        RoomType = roomType;
        RoomPrefabName = roomPrefabName;
        EnemyWaves = enemyWaves;
        ConditionInfoId = -1;
    }
    public RoomData(){}

    public void SetConditionInfoId(int id)
    {
        ConditionInfoId = id;
    }

    public void SetRoomIndex(int roomIndex)
    {
        CurrentRoomIndex = roomIndex;
    }
}



