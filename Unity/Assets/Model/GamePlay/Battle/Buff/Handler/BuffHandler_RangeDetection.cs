﻿using ETModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Box2DSharp;
using Box2DSharp.Collision;
using System.Numerics;
using Box2DSharp.Common;

[BuffType(BuffIdType.RangeDetection)]
public class BuffHandler_RangeDetection : BaseBuffHandler, IBuffActionWithSetOutputHandler
{

    public IBufferValue[] ActionHandle(ref BuffHandlerVar buffHandlerVar)
    {
        Buff_RangeDetection buff_RangeDetection = (Buff_RangeDetection)buffHandlerVar.data;

        if (!buffHandlerVar.GetBufferValue(out BufferValue_Pos bufferValue_Pos))
        {
            return null;
        }
        //主要目的是返回多个群体目标对象
        BufferValue_TargetUnits bufferValue_TargetUnits = new BufferValue_TargetUnits();


        PolyshapeQueryCallback polyshapeQueryCallback = new PolyshapeQueryCallback();
        AABB ab = new AABB();

        switch (buff_RangeDetection.shapeType)
        {
            case Buff_RangeDetection.CollisionShape.Box:
                //根据传入进来的方向和位置计算做范围检测的区域



                if (!buffHandlerVar.GetBufferValue(out BufferValue_Dir bufferValue_Dir))
                {
                    Log.Error("Box检测没有收到方向  " + buffHandlerVar.skillId);
                    return null;
                }

                Vector2 pos = bufferValue_Pos.aimPos.ToVector2();


                var transform = new Transform(in pos, bufferValue_Dir.dir.ToRotation2D().Angle);
                var Vertices = new Vector2[4];

                float hx = buff_RangeDetection.shapeValue.x;
                float hy = buff_RangeDetection.shapeValue.y;
                Vertices[0].Set(-hx, -hy);
                Vertices[1].Set(hx, -hy);
                Vertices[2].Set(hx, hy);
                Vertices[3].Set(-hx, hy);
          
                for (var i = 0; i < 4; ++i)
                {
                    Vertices[i] = MathUtils.Mul(transform, Vertices[i]);
                }              
                ab.UpperBound = ab.LowerBound = Vertices[0];
                for (var i = 1; i < 4; ++i)
                {
                    var v = Vertices[i];
                    ab.LowerBound = Vector2.Min(ab.LowerBound, v);
                    ab.UpperBound = Vector2.Max(ab.UpperBound, v);
                }
                var r = new Vector2(Settings.PolygonRadius, Settings.PolygonRadius);
                ab.LowerBound -= r;
                ab.UpperBound += r;
                break;
            case Buff_RangeDetection.CollisionShape.Circle:

                pos = bufferValue_Pos.aimPos.ToVector2();
                transform = new Transform(in pos, 0);
                var p = MathUtils.Mul(transform.Rotation, transform.Position);
                float raidus = buff_RangeDetection.shapeValue.x;
                ab.LowerBound.Set(p.X - raidus, p.Y - raidus);
                ab.UpperBound.Set(p.X + raidus, p.Y + raidus);

                break;
        }
        //Log.Debug(ab.LowerBound.ToString() + ab.UpperBound.ToString());
        PhysicWorldComponent.Instance.world.QueryAABB(polyshapeQueryCallback, ab);

        if (polyshapeQueryCallback.units == null || polyshapeQueryCallback.units.Count == 0)
        {
            Log.Debug("没有检测到任何单位");
            return null;
        }



        for (int i = polyshapeQueryCallback.units.Count - 1; i >= 0; i--)
        {
            //默认层(一般是特效,墙壁等)
            if (polyshapeQueryCallback.units[i].UnitData.groupIndex ==  GroupIndex.Default ||
                polyshapeQueryCallback.units[i].UnitData.unitLayer == UnitLayer.Default)
            {
                polyshapeQueryCallback.units.RemoveAt(i);
                continue;
            }

            if (buff_RangeDetection.FindFriend)
            {
                if (polyshapeQueryCallback.units[i].UnitData.groupIndex != buffHandlerVar.source.UnitData.groupIndex)
                {
                    polyshapeQueryCallback.units.RemoveAt(i);
                    continue;
                }
            }
            else
            {
                if (polyshapeQueryCallback.units[i].UnitData.groupIndex == buffHandlerVar.source.UnitData.groupIndex)
                {
                    polyshapeQueryCallback.units.RemoveAt(i);
                    continue;
                }
            }
            //高度区域检测
            if (Math.Abs((bufferValue_Pos.aimPos.y + buff_RangeDetection.halfHeight) - (polyshapeQueryCallback.units[i].Position.y + polyshapeQueryCallback.units[i].OffsetY))
                > (buff_RangeDetection.halfHeight + polyshapeQueryCallback.units[i].HalfHeight))
            {
                polyshapeQueryCallback.units.RemoveAt(i);
                Log.Debug("目前高度不在检测的范围内!");
                continue;
            }
            else
            {
                Log.Debug("目前高度在检测的范围内!");
            }
        }

        //拿到了所有检测到的Unit
        bufferValue_TargetUnits.targets = polyshapeQueryCallback.units.ToArray();
        

       // Log.Debug("范围检测到了  " + bufferValue_TargetUnits.targets.Length+ "  个目标");
        return new IBufferValue[] { bufferValue_TargetUnits };
    }
}



