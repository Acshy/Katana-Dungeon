using System;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    private void Update()
    {
        if (BattleManager.Inst == null ||
            BattleManager.Inst.CurrentPlayer == null)
        {
            return;
        }
        var pos = BattleManager.Inst.CurrentPlayer.transform.position;
        transform.position = new Vector3(pos.x,transform.position.y,pos.z);
    }
}
