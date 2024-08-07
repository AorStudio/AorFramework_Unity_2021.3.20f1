using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using NodeGraph;

public class CustomPrefabProcess :IPrefabProcess
{

    public void Reset()
    {
        //
    }

    public string ResultInfoDescribe()
    {
        return "自定义PrefabProcess处理完成.";
    }

    public bool PrefabProcess(string path, GameObject prefab, ref List<string> ResultInfoList)
    {

        return true;
    }


}
