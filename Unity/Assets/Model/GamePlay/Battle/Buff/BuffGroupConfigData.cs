using ETModel;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 一组BUFF,用来组成玩家眼中的装备/道具/技能等等附加的持续性效果
/// </summary>
[Serializable]
public class BuffGroupConfigData
{
    [LabelText("Buff类型Id")]
    [LabelWidth(150)]
    public int buffTypeId;   //从buff配置表中读取Buff应该显示出来的名字/描述等信息
    /// <summary>
    /// 这个是每个同类的BuffGroup都引用同类型的BuffList.所以这里设计为结构体中的引用类型.这样复制结构体的时候,引用可以保证是同一份.
    /// </summary>
    [ListDrawerSettings(ShowItemCount = true)]
    public List<BaseBuffData> buffList;

    public int IsExist(string buffId)
    {
        int num = 0;
        foreach (var v in buffList)
        {
            if (v.GetBuffIdType() == buffId)
            {
                num++;
            }
        }
        return num;
    }
}

