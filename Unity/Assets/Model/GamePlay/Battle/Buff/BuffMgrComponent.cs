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

    public Dictionary<long, BuffGroup> buffGroupDic; 

    //如果是节日,活动等导致的长时间BUFF. 比如新人奖励(等级在50级之前,获得的经验值额外增加50%),或者常驻一个持续一周的副本内属性提升的BUFF
    //这种类型的,在另外个地方,单独管理即可. (监听进入副本的事件,而后给角色添加BUFF)

    private const long calSpan = 1000;

    public void Awake()
    {
        buffGroupDic = new Dictionary<long, BuffGroup>();
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
                            GameCalNumericTool.CalDotDamage(buffGroup.sourceUnitId, GetParent<Unit>(), dot);
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


    public  void AddBuffGroup(long groupId, BuffGroup group)
    {
        try
        {


            //刷新BUFF,暂时没做叠加

            if (buffGroupDic.TryGetValue(groupId,out var oldBuff))
            {
                //刷新新时长
                oldBuff.duration = group.duration;
                buffGroupDic[groupId] = oldBuff;
                TimeSpanHelper.Timer timer = TimeSpanHelper.GetTimer(oldBuff.GetHashCode());
                timer.interval = (long)(oldBuff.duration * 1000);
                timer.timing = 0;
                return;
            }

            BuffGroup newGroup = group;
            newGroup.BuffGroupId = group.BuffGroupId;
            Unit target = Parent as Unit;
            Unit source = null;
            if (group.sourceUnitId != 0)
                source = UnitComponent.Instance.Get(group.sourceUnitId);
            else
                source = target;
            newGroup.OnBuffGroupAdd(source, target);
            buffGroupDic[groupId] = newGroup;
            
            if (newGroup.duration > 0)
            {
                TimeSpanHelper.Timer timer = TimeSpanHelper.GetTimer(newGroup.GetHashCode());
                timer.interval = (long)(newGroup.duration * 1000);
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
        if (!buffGroupDic.TryGetValue(groupId,out group))
        {
            return;
        }
        Unit target = Parent as Unit;
        Unit source = null;
        if (group.sourceUnitId != 0)
            source = UnitComponent.Instance.Get(group.sourceUnitId);
        else
            source = target;
        group.OnBuffGroupRemove(source,target);
        buffGroupDic.Remove(groupId);
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
        buffGroupDic.Clear();
    }

}

