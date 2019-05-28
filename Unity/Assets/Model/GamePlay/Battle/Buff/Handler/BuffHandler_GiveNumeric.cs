﻿using ETModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

[BuffType(BuffIdType.GiveNumeric)]
public class BuffHandler_GiveNumeric : BaseBuffHandler, IBuffActionWithGetInputHandler
{

    public void ActionHandle(ref BuffHandlerVar buffHandlerVar)
    {
        Buff_GiveNumeric buff = (Buff_GiveNumeric)buffHandlerVar.data;
        if (!buffHandlerVar.GetBufferValue(out BufferValue_TargetUnits targetUnits))
        {
            return;
        }

        foreach (var v in targetUnits.targets)
        {
            Game.EventSystem.Run(EventIdType.NumbericChange, buff.numericType, v.Id, buff.value);

        }
    }
}



