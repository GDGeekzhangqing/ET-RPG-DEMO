using ETModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[ObjectSystem]
public class BuffConfigComponentAwakeSystem : AwakeSystem<BuffConfigComponent>
{
    public override void Awake(BuffConfigComponent self)
    {
        self.Awake();
    }
}

public class BuffConfigComponent : ETModel.Component
{
    public Dictionary<int, BuffGroupConfigData> buffConfigDataDic;

    public static BuffConfigComponent instance;

    private const string abName = "BuffConfig.unity3d";

    public void Awake()
    {
        instance = this;
        buffConfigDataDic = new Dictionary<int, BuffGroupConfigData>();
        Game.Scene.GetComponent<ResourcesComponent>().LoadBundle(abName);
        var buffCollection = Game.Scene.GetComponent<ResourcesComponent>().GetAsset(abName, "BuffCollection") as BuffCollection;
#if UNITY_EDITOR
        TestDeserialize();
#else
        buffConfigDataDic = buffCollection.buffConfigData;
#endif
    }

    void TestDeserialize()
    {
        buffConfigDataDic = MessagePack.MessagePackSerializer.Deserialize<Dictionary<int, BuffGroupConfigData>>(File.ReadAllBytes(Application.dataPath+ "../../../Config/BuffConfigData.bytes"),
           MessagePack.Resolvers.ContractlessStandardResolver.Instance);
    }

    public BuffGroupConfigData GetBuffConfigData(int buffTypeId)
    {
        BuffGroupConfigData data;
        if (!buffConfigDataDic.TryGetValue(buffTypeId, out data))
        {
            return null;
        }
        return data;
    }

}

