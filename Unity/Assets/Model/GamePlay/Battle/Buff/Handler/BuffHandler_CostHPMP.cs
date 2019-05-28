﻿using ETModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

[BuffType(BuffIdType.CostHPMP)]
public class BuffHandler_CostHPMP : BaseBuffHandler, IBuffActionWithGetInputHandler
{

    public void ActionHandle(ref BuffHandlerVar buffHandlerVar)
    {
        Buff_CostHP_MP cost = (Buff_CostHP_MP)buffHandlerVar.data;

        if (!buffHandlerVar.GetBufferValue(out BufferValue_TargetUnits targetUnits))
        {
            return;
        }
        foreach (var v in targetUnits.targets)
        {
            Unit target = v;
            NumericComponent numericComponent = target.GetComponent<NumericComponent>();
            float updateHp = -cost.costHp - cost.costHpInPct * numericComponent.GetAsFloat(NumericType.HPMax_Final);
            float updateMp = -cost.costMp - cost.costMpInPct * numericComponent.GetAsFloat(NumericType.MPMax_Final);
            if (updateHp != 0)
                Game.EventSystem.Run(EventIdType.NumbericChange, NumericType.HP, target.Id, updateHp);
            if (updateMp != 0)
                Game.EventSystem.Run(EventIdType.NumbericChange, NumericType.MP, target.Id, updateMp);
        }
    }
}




