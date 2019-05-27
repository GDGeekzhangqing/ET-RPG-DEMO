using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

[CreateAssetMenu(menuName = "游戏设置/Buff配置", fileName = "BuffCollection")]
public class BuffCollection : SerializedScriptableObject
{
    public Dictionary<int, BuffGroupConfigData> buffConfigData = new Dictionary<int, BuffGroupConfigData>();

#if UNITY_EDITOR
    [Button("保存所有buff信息至文件", 25)]
    public void SaveToFile()
    {

        var bin = MessagePack.MessagePackSerializer.Serialize(buffConfigData, MessagePack.Resolvers.ContractlessStandardResolver.Instance);
        File.WriteAllBytes(Application.dataPath + "../../../Config/BuffConfigData.bytes", bin);

        Debug.Log("保存成功!");
    }
#endif
}
