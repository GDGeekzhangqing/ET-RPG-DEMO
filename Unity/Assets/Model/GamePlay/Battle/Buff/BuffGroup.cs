using ETModel;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


public struct BuffGroupInitData
{
    public long sourceUnitId;// 添加到一个Unit的BuffMgr上时,这个用以记录这个buffGroup的来源

    public int skillLevel;//添加到一个Unit身上时,这个用以记录对应技能的等级
    //[InfoBox("该值对应buff配置表里的某个元素")]
    //[LabelText("Buff类型Id")]
    //[LabelWidth(150)]
    public int buffTypeId;   //从buff配置表中读取Buff应该显示出来的名字/描述等信息
    //[InfoBox("-1代表持续到BUFF组被解除,0代表瞬间完成.大于0代表持续一段时间")]
    //[LabelText("Buff持续时间")]
    //[LabelWidth(150)]
    public float duration;
}

/// <summary>
/// 一组BUFF,用来组成玩家眼中的装备/道具/技能等等附加的持续性效果
/// </summary>
public class BuffGroup
{
    public BuffGroupInitData data;

    public System.Threading.CancellationTokenSource cancellationTokenSource = new System.Threading.CancellationTokenSource(); // 用以移除Buff

    public static Dictionary<(long, string), long> BuffGroupIdCollection = new Dictionary<(long, string), long>();

    public static long GetBuffGroupId(Unit unit, BaseBuffData buffData)
    {
        var key = (unit.Id, buffData.buffSignal);
        if (BuffGroupIdCollection.TryGetValue(key, out long id))
        {
            return id;
        }
        BuffGroupIdCollection[key] = IdGenerater.GenerateId();
        return BuffGroupIdCollection[key];
    }

    public long BuffGroupId
    {
        get;set;
    }

    public List<BaseBuffData> GetBuffList()
    {
        return  BuffConfigComponent.instance.GetBuffConfigData(data.buffTypeId).buffList;
    }

    public void OnBuffGroupAdd(Unit source, Unit target)
    {
        var buffList = GetBuffList();
        if (buffList.Count > 0)
        {
            foreach (var v in buffList)
            {
                Add(in v, source, target);
            }
        }
    }

    void Add(in BaseBuffData buff, Unit source, Unit target)
    {
        BaseBuffHandler baseBuffHandler = BuffHandlerComponent.Instance.GetHandler(buff.GetBuffIdType());
        IBuffActionWithGetInputHandler buffActionWithGetInputHandler = baseBuffHandler as IBuffActionWithGetInputHandler;
        if (buffActionWithGetInputHandler != null)
        {
            BuffHandlerVar var1 = new BuffHandlerVar();
            var1.bufferValues = new Dictionary<Type, IBufferValue>();
            var1.bufferValues[typeof(BufferValue_TargetUnits)] = new BufferValue_TargetUnits() { targets = new Unit[1] { target } };
            var1.source = source;
            var1.skillLevel = data.skillLevel;
            var1.playSpeed = 1;// 这个应该从角色属性计算得出,不过这里就先恒定为1好了.
            var1.data = buff;
            buffActionWithGetInputHandler.ActionHandle(ref var1);
        }

    }


    public void OnBuffGroupRemove(Unit source, Unit target)
    {
        var buffList = GetBuffList();
        if (buffList.Count > 0)
        {
            foreach (var v in buffList)
            {
                Remove(in v, source, target);
            }
        }
    }



    void Remove(in BaseBuffData v,Unit source,Unit target)
    {
        BaseBuffHandler baseBuffHandler = BuffHandlerComponent.Instance.GetHandler(v.GetBuffIdType());
        IBuffRemoveHanlder buffRemoveHanlder = baseBuffHandler as IBuffRemoveHanlder;
        if (buffRemoveHanlder != null)
        {
            BuffHandlerVar buffHandlerVar = new BuffHandlerVar();
            buffHandlerVar.bufferValues = new Dictionary<Type, IBufferValue>(1);
            buffHandlerVar.bufferValues[typeof(BufferValue_TargetUnits)] = new BufferValue_TargetUnits() { targets = new Unit[1] { target } };
            buffHandlerVar.source = source;
            buffHandlerVar.playSpeed = 1;// 这个应该从角色属性计算得出,不过这里就先恒定为1好了.
            buffHandlerVar.data = v;
            buffRemoveHanlder.Remove(ref buffHandlerVar);
        }
    }

    public void OnBuffUpdate(Unit source, Unit target,BaseBuffData baseBuffData)
    {
        BaseBuffHandler baseBuffHandler = BuffHandlerComponent.Instance.GetHandler(baseBuffData.GetBuffIdType());
        IBuffUpdateHanlder buffUpdateHandler = baseBuffHandler as IBuffUpdateHanlder;
        if (buffUpdateHandler != null)
        {
            BuffHandlerVar buffHandlerVar = new BuffHandlerVar();
            buffHandlerVar.bufferValues = new Dictionary<Type, IBufferValue>(1);
            buffHandlerVar.bufferValues[typeof(BufferValue_TargetUnits)] = new BufferValue_TargetUnits() { targets = new Unit[1] { target } };
            buffHandlerVar.source = source;
            buffHandlerVar.playSpeed = 1;// 这个应该从角色属性计算得出,不过这里就先恒定为1好了.
            buffHandlerVar.data = baseBuffData;
            buffUpdateHandler.Update(ref buffHandlerVar);
        }
    }
}

