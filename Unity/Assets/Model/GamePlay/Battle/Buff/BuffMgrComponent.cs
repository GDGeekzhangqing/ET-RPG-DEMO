using ETModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[ObjectSystem]
public class BuffMgrComponentAwakeSystem : AwakeSystem<BuffMgrComponent>
{
    public override void Awake(BuffMgrComponent self)
    {
        self.Awake();
    }
}

/// <summary>
/// Unit身上管理所有BUFF的组件
/// </summary>
public class BuffMgrComponent : ETModel.Component
{

    public Dictionary<long, BuffGroup> allBuff; // 可能有装备提供的半永久Buff.所有Buff都在内

    public Dictionary<long,BuffGroup> updateBuff; // 需要和UI层交互的,显示在界面上的Buff

    //如果是节日,活动等导致的长时间BUFF. 比如新人奖励(等级在50级之前,获得的经验值额外增加50%),或者常驻一个持续一周的副本内属性提升的BUFF
    //这种类型的,在另外个地方,单独管理即可. (监听进入副本的事件,而后给角色添加BUFF)

    private const long calSpan = 1000;

    public void Awake()
    {
        allBuff = new Dictionary<long, BuffGroup>();
        updateBuff = new Dictionary<long, BuffGroup>();
    }

    async ETVoid DealWithBuffGroup(BuffGroup buffGroup)
    {
        while (true)
        {
            await TimerComponent.Instance.WaitAsync(calSpan, buffGroup.cancellationTokenSource.Token);

            TimeSpanHelper.Timer timer = TimeSpanHelper.GetTimer(buffGroup.GetHashCode());
            long now = TimeHelper.Now();
            if (now - timer.timing >= calSpan)
            {
                timer.timing = now;
            }
            var buffList = buffGroup.GetBuffList();
            if (buffList.Count > 0)
            {
                foreach (var v in buffList)
                {
                    switch (v)
                    {
                        case Buff_DOT dot:
                            GameCalNumericTool.CalDotDamage(buffGroup.data.sourceUnitId, GetParent<Unit>(), dot);
                            break;

                        default:
                            break;
                    }
                }
            }

            if (timer.timing >= timer.interval)
            {
                RemoveGroup(buffGroup.BuffGroupId);
                TimeSpanHelper.Remove(buffGroup.GetHashCode());
                return;
            }
        }
    }


    public  void AddBuffGroup(long buffGroupId,in BuffGroupInitData buffGroupInitData)
    {
        try
        {


            //刷新BUFF,暂时没做叠加

            if (allBuff.TryGetValue(buffGroupId, out var oldBuff))
            {
                //刷新新时长
                oldBuff.data.duration = buffGroupInitData.duration;
               // buffGroupDic[groupId] = oldBuff; 改为类了,不需要再赋值回去
                TimeSpanHelper.Timer timer = TimeSpanHelper.GetTimer(oldBuff.GetHashCode());
                timer.interval = (long)(oldBuff.data.duration * 1000);
                timer.timing = 0;
                return;
            }

            BuffGroup newGroup = new BuffGroup();
            newGroup.BuffGroupId = buffGroupId;
            newGroup.data = buffGroupInitData;
            Unit target = Parent as Unit;
            Unit source = null;
            if (buffGroupInitData.sourceUnitId != 0)
                source = UnitComponent.Instance.Get(buffGroupInitData.sourceUnitId);
            else
                source = target;
            newGroup.OnBuffGroupAdd(source, target);
            allBuff[buffGroupId] = newGroup;
            
            //暂时只考虑duration>0的才需要和UI交互
            if (newGroup.data.duration > 0)
            {
                updateBuff.Add(newGroup.BuffGroupId,newGroup);
                TimeSpanHelper.Timer timer = TimeSpanHelper.GetTimer(newGroup.GetHashCode());
                timer.interval = (long)(newGroup.data.duration * 1000);
                timer.timing = 0;
                DealWithBuffGroup(newGroup).Coroutine();
            }
        }
        catch (Exception e)
        {
            Log.Error(e.ToString());
        }
    }
    public void RemoveGroup(long groupId)
    {
      
        BuffGroup group;
        if (!allBuff.TryGetValue(groupId,out group))
        {
            return;
        }
        Unit target = Parent as Unit;
        Unit source = null;
        if (group.data.sourceUnitId != 0)
            source = UnitComponent.Instance.Get(group.data.sourceUnitId);
        else
            source = target;
        group.OnBuffGroupRemove(source,target);
        group.cancellationTokenSource.Cancel();
        allBuff.Remove(groupId);
        if (updateBuff.ContainsKey(groupId))
        {
            
            updateBuff.Remove(groupId);
        }
        //抛出事件,影响其他地方的改变
    }




    public void ClearBuffGroupOnBattleEnd()
    {

        //TODO:后续如果出现诸如冰冻等限制类类型的DEBUFF,会在这里统一去除
    }


    public override void Dispose()
    {
        if (IsDisposed)
            return;
        base.Dispose();
        allBuff.Clear();
    }

}

