using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.Universal.LightmapConfigurator.LightmapConfigurator;

namespace UnityEngine.Rendering.Universal.LightmapConfigurator
{

    public class LightmapConfiguratorManager// : MonoBehaviour
    {

        private static LightmapConfiguratorManager m_Instance;
        public static LightmapConfiguratorManager Instance
        {
            get
            {
                if (m_Instance == null)
                {
                    //GameObject go = GameObject.Find("LightmapConfiguratorManager");
                    //if (!go)
                    //{
                    //    go = new GameObject("LightmapConfiguratorManager");
                    //}
                    //GameObject.DontDestroyOnLoad(go);
                    //m_Instance = go.GetComponent<LightmapConfiguratorManager>();
                    //if (!m_Instance) m_Instance = go.AddComponent<LightmapConfiguratorManager>();
                    m_Instance = new LightmapConfiguratorManager();
                }
                return m_Instance;
            }
        }

        private readonly List<LightmapConfigurator> m_managedDatas = new List<LightmapConfigurator>();
        /// <summary>
        /// key:lightMapDataRecorder.HashCode,value:引用计数
        /// </summary>
        private readonly Dictionary<int, int> m_lightMapDataRecorderHashCountDic = new Dictionary<int, int>();
        /// <summary>
        /// key:LightMapDataRecorder.HashCode, value:LightMapDataRecorder
        /// </summary>
        private readonly Dictionary<int, LightMapDataRecorder> m_managedLightMapDataRecordersDic = new Dictionary<int, LightMapDataRecorder>();
        private readonly Dictionary<int, LightmapData> m_managedLightmapDataDic = new Dictionary<int, LightmapData>();
        /// <summary>
        /// 
        /// </summary>
        private readonly List<int> m_managedLightMapDataRecorderHashs = new List<int>();
        private readonly List<LightmapData> m_managedLightmapDatas = new List<LightmapData>();

        private readonly List<LightProbes> m_managedLightProbes = new List<LightProbes>();

        private LightMapDataRecorder m_emptyRecorder = new LightMapDataRecorder();
        private LightmapData m_empty = new LightmapData();
        
        /// <summary>
        /// LightmapConfiguratorManager重置方法, 建议在转换整个场景时调用
        /// </summary>
        public void Reset()
        {
            LightmapSettings.lightmaps = null;
            LightmapSettings.lightProbes = null;

            m_managedDatas.Clear();
            m_lightMapDataRecorderHashCountDic.Clear();
            m_managedLightMapDataRecordersDic.Clear();
            m_managedLightmapDataDic.Clear();

            m_managedLightProbes.Clear();
        }

        public void Register(LightmapConfigurator lcConfigurator)
        {
            if (!m_managedDatas.Contains(lcConfigurator))
            {
                m_managedDatas.Add(lcConfigurator);
                if (RegisterLightMapDataRecorders(lcConfigurator))
                {
                    UpdateManagedLightMapDatas();
                }

                UpdateRenderersLightmapSetting(lcConfigurator);

                if (lcConfigurator.lightProbes != null)
                {
                    LightmapSettings.lightProbes = lcConfigurator.lightProbes;
                    m_managedLightProbes.Add(lcConfigurator.lightProbes);
                }
            }
            //refreshLightmapSettings();
            //m_refreshDirty = true;

        }

        public void Unregister(LightmapConfigurator lcConfigurator)
        {
            if (m_managedDatas.Remove(lcConfigurator))
            {

                if (UnregisterLightMapDataRecorders(lcConfigurator))
                {
                    UpdateManagedLightMapDatas();
                }

                lcConfigurator.DisableLightmapParams();

                if (lcConfigurator.lightProbes != null)
                {
                    m_managedLightProbes.Remove(lcConfigurator.lightProbes);
                    if(m_managedLightProbes.Count > 0)
                    {
                        LightmapSettings.lightProbes = m_managedLightProbes[m_managedLightProbes.Count - 1];
                    }
                    else
                    {
                        LightmapSettings.lightProbes = null;
                    }
                }
            }
            //refreshLightmapSettings();
            //m_refreshDirty = true;
        }

        private bool RegisterLightMapDataRecorders(LightmapConfigurator lcConfigurator)
        {
            bool isDirty = false;
            foreach (var mdr in lcConfigurator.lightMapDataRecorders)
            {
                int hash = mdr.GetHashCode();
                if (!m_lightMapDataRecorderHashCountDic.ContainsKey(hash))
                {
                    m_lightMapDataRecorderHashCountDic.Add(hash, 0);
                }
                if(m_lightMapDataRecorderHashCountDic[hash] == 0)
                {
                    m_managedLightMapDataRecordersDic.Add(hash, mdr);
                    LightmapData lightmapData = new LightmapData();
                    mdr.Restore(lightmapData);
                    m_managedLightmapDataDic.Add(hash, lightmapData);
                    isDirty = true;
                }
                m_lightMapDataRecorderHashCountDic[hash]++;
            }
            return isDirty;
        }

        private bool UnregisterLightMapDataRecorders(LightmapConfigurator lcConfigurator)
        {
            bool isDirty = false;
            foreach (var mdr in lcConfigurator.lightMapDataRecorders)
            {
                int hash = mdr.GetHashCode();
                if (m_lightMapDataRecorderHashCountDic.ContainsKey(hash))
                {
                    m_lightMapDataRecorderHashCountDic[hash]--;
                    if(m_lightMapDataRecorderHashCountDic[hash] <= 0)
                    {
                        m_managedLightMapDataRecordersDic.Remove(hash);
                        m_managedLightmapDataDic.Remove(hash);
                        isDirty = true;
                    }
                }
            }
            return isDirty;
        }

        private void UpdateManagedLightMapDatas()
        {
            m_managedLightMapDataRecorderHashs.Clear();
            m_managedLightmapDatas.Clear();
            foreach (var kv in m_lightMapDataRecorderHashCountDic)
            {
                if(kv.Value > 0)
                {
                    int hash = m_managedLightMapDataRecordersDic[kv.Key].GetHashCode();
                    m_managedLightMapDataRecorderHashs.Add(hash);
                    m_managedLightmapDatas.Add(m_managedLightmapDataDic[kv.Key]);
                }
                else
                {
                    m_managedLightMapDataRecorderHashs.Add(0);
                    m_managedLightmapDatas.Add(m_empty);
                }
            }
            LightmapSettings.lightmaps = m_managedLightmapDatas.ToArray();
        }

        private void UpdateRenderersLightmapSetting(LightmapConfigurator lcConfigurator)
        {
            List<int> refKeyList = new List<int>();
            foreach (var mdr in lcConfigurator.lightMapDataRecorders)
            {
                var hash = mdr.GetHashCode();
                var refKey = m_managedLightMapDataRecorderHashs.FindIndex((i) => i == hash);
                refKeyList.Add(refKey);
            }
            lcConfigurator.UpdateRenderersLightmapSetting(refKeyList.ToArray());
        }

        //private void Update()
        //{
        //    if (m_refreshDirty)
        //    {
        //        refreshLightmapSettings();
        //        m_refreshDirty = false;
        //    }
        //}

        //private void refreshLightmapSettings()
        //{

        //    m_managedRefKeyDic.Clear();
        //    m_managedLightmapDatas.Clear();

        //    int len = m_managedDatas.Count;
        //    for (int i = 0; i < len; i++)
        //    {
        //        LightmapConfigurator lcr = m_managedDatas[i];
        //        if (i == 0)
        //        {
        //            LightmapSettings.lightmapsMode = lcr.lightmapMode;
        //            m_managedLightProbes = lcr.lightProbes;
        //        }

        //        if (lcr.lightMapDataRecorders != null && lcr.lightMapDataRecorders.Length > 0)
        //        {
        //            int jlen = lcr.lightMapDataRecorders.Length;
        //            int[] localRefkeys = new int[jlen];
        //            for (int j = 0; j < jlen; j++)
        //            {
        //                int key = lcr.LightmapDataKeys[j];
        //                if (!m_managedRefKeyDic.ContainsKey(key))
        //                {
        //                    localRefkeys[j] = m_managedLightmapDatas.Count;
        //                    LightmapData lightmapData = new LightmapData();
        //                    lcr.lightMapDataRecorders[j].Restore(lightmapData);
        //                    m_managedLightmapDatas.Add(lightmapData);
        //                    m_managedRefKeyDic.Add(key, j);
        //                }
        //                else
        //                {
        //                    localRefkeys[j] = m_managedRefKeyDic[key];
        //                }
        //            }

        //            lcr.UpdateRenderersLightmapSetting(localRefkeys);
        //            //if (lcr.StaticBatchingList != null && lcr.StaticBatchingList.Count > 0)
        //            //{
        //            //   StaticBatchingUtility.Combine(lcr.StaticBatchingList.ToArray(), lcr.gameObject);
        //            //}
        //        }
        //    }

        //    LightmapSettings.lightmaps = m_managedLightmapDatas.ToArray();
        //    LightmapSettings.lightProbes = m_managedLightProbes;
        //}

    }

}
