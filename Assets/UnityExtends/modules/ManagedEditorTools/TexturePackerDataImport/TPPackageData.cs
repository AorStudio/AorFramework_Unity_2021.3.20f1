using AORCore;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace TexturePackerDataImport
{

    /// <summary>
    /// TexturePacker (tm) (v3.0+) 产出JSON的数据对象
    /// 注意: 仅支持 TexturePacker中设置DataFormat： JSON(Array) / JSON(Hash) / Unity3D 生成的JSON数据
    /// 
    /// Author     : Aorition
    /// UpdateDate : 2024-03-20
    /// 
    /// </summary>
    [Serializable]
    public class TPPackageData
    {

        /// <summary>
        /// 解析 TexturePacker.dataFormat 为 JSON(Array)的文本数据
        /// </summary>
        public static TPPackageData ParseJSONArrayFormatText(string json)
        {
            return JsonUtility.FromJson<TPPackageData>(json);
        }

        /// <summary>
        /// 解析 TexturePacker.dataFormat 为 Unity/JSON(Hash)的文本数据
        /// </summary>
        public static TPPackageData ParseJSONHashFormatText(string json)
        {
            TPPackageData data = new TPPackageData();
            Dictionary<string, object> dic = Json.DecodeToDic(json);
            data.meta = TPPackageMetaData.Parse(dic["meta"] as Dictionary<string, object>);

            Dictionary<string, object> frames = dic["frames"] as Dictionary<string, object>;

            List<TPFrameInfoData> framesList = new List<TPFrameInfoData>();
            foreach (KeyValuePair<string, object> kv in frames)
            {
                framesList.Add(TPFrameInfoData.Prase(kv.Key, kv.Value as Dictionary<string, object>));
            }
            data.frames = framesList.ToArray();
            return data;
        }

        public TPFrameInfoData[] frames;
        public TPPackageMetaData meta;

    }

    #region Inner Datas  

    [Serializable]
    public class TPPackageMetaData
    {
        public static TPPackageMetaData Parse(Dictionary<string, object> dic)
        {
            TPPackageMetaData meta = new TPPackageMetaData();
            meta.app = dic["app"].ToString();
            meta.version = dic["version"].ToString();
            meta.image = dic["image"].ToString();
            meta.format = dic["format"].ToString();
            meta.size = TPFV2.Parse(dic["size"] as Dictionary<string, object>);
            meta.scale = float.Parse(dic["scale"].ToString());
            meta.smartupdate = dic["smartupdate"].ToString();
            return meta;
        }

        public string app;
        public string version;
        public string image;
        public string format;
        public TPFV2 size;
        public float scale;
        public string smartupdate;
    }

    [Serializable]
    public class TPFrameInfoData
    {

        public static TPFrameInfoData Prase(string name, Dictionary<string, object> dic)
        {
            TPFrameInfoData data = new TPFrameInfoData();
            data.filename = name;
            data.frame = TPFV4.Parse(dic["frame"] as Dictionary<string, object>);
            data.rotated = (bool)dic["rotated"];
            data.trimmed = (bool)dic["trimmed"];
            data.spriteSourceSize = TPFV4.Parse(dic["spriteSourceSize"] as Dictionary<string, object>);
            data.sourceSize = TPFV2.Parse(dic["sourceSize"] as Dictionary<string, object>);
            return data;
        }

        public string filename;
        public TPFV4 frame;
        public bool rotated;
        public bool trimmed;
        public TPFV4 spriteSourceSize;
        public TPFV2 sourceSize;
    }

    [Serializable]
    public struct TPFV2
    {

        public static TPFV2 Parse(ref Vector2 v2)
        {
            int w = (int)v2.x;
            int h = (int)v2.y;
            return new TPFV2 { w = w, h = h };
        }

        public static TPFV2 Parse(Dictionary<string, object> dic)
        {
            int w = int.Parse(dic["w"].ToString());
            int h = int.Parse(dic["h"].ToString());
            return new TPFV2 { w = w, h = h };
        }

        public int w;
        public int h;

        public Vector2 ToVec()
        {
            return new Vector2(w, h);
        }
    }

    [Serializable]
    public struct TPFV4
    {

        public static TPFV4 Parse(ref Vector4 v4)
        {
            int x = (int)v4.x;
            int y = (int)v4.y;
            int w = (int)v4.z;
            int h = (int)v4.w;
            return new TPFV4 { x = x, y = y, w = w, h = h };
        }

        public static TPFV4 Parse(Dictionary<string, object> dic)
        {
            int x = int.Parse(dic["x"].ToString());
            int y = int.Parse(dic["y"].ToString());
            int w = int.Parse(dic["w"].ToString());
            int h = int.Parse(dic["h"].ToString());
            return new TPFV4 { x = x, y = y, w = w, h = h };
        }

        public int x;
        public int y;
        public int w;
        public int h;

        public Rect ToRect()
        {
            return new Rect(x, y, w, h);
        }

        public Vector4 ToVec()
        {
            return new Vector4(x, y, w, h);
        }
    }

    #endregion

}

