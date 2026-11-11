using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoldEffect : ItemEffect
{
    private int goldValue;

    public GoldEffect(int value)
    {
        goldValue = value;
    }

    public override void TriggerEffect(ItemEffectTriggerValue? value)
    {
        base.TriggerEffect(value);
        for (int i = 0; i < goldValue; i++)
        {
            DropManager.Inst.SpwanCoinAt(roleController.transform.position);
        }
    }
}