/// <summary>
/// Code maintainer : Aorition
/// Update : 2023-08-31
/// </summary>

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace UnityEngine.Rendering.Universal
{

    public class FPS : MonoBehaviour
    {

        private const string HandlerNameDefine = "FPSHandler";

        private static FPS m_instance;
        public static FPS Instance { get { return m_instance; } }

        public static FPS GetOrCreate()
        {
            if (m_instance)
                return m_instance;
            var f = GameObject.FindObjectOfType<FPS>();
            if (!f)
                f = new GameObject(HandlerNameDefine).AddComponent<FPS>();
            return f;
        }

        public static FPS Create(GameObject node)
        {
            if (m_instance)
                return m_instance;

            return node.AddComponent<FPS>();
        }

        private GUIStyle Style;

        public Vector2 m_panelSize = new Vector2(300, 100);
        public Vector2 m_panelPosOffset = new Vector2(25, 25);

        [SerializeField]
        private Text m_UGUIText;
        private bool m_useUGUI;
        public bool UseUGUIDisplay => m_useUGUI;

        //强制锁定目标帧率
        public bool FoucsFrameRateLimt;
        public int FoucsTatgetFrameRate = 30;

#if UNITY_EDITOR && !RUNTIME
        [HideInInspector, NonSerialized] public bool FrameRateLimt;
        [HideInInspector, NonSerialized] public int SrcTatgetFrameRate = -1;
#endif
        private void Awake()
        {

            if(m_instance)
            {
                if (Application.isPlaying)
                    GameObject.Destroy(this);
                else
                    GameObject.DestroyImmediate(this);
                return;
            }

             m_instance = this;
            Application.runInBackground = true;

            if (m_UGUIText)
            {
                m_useUGUI = true;
            }
            else
            {
                m_useUGUI = false;
                Style = new GUIStyle();
                Style.fontSize = 20;
                Style.normal.textColor = new Color(0.8f, 0f, 0f, 1f);
            }

        }

        private void OnEnable()
        {
            if (m_useUGUI && m_UGUIText)
            {
                if (!m_UGUIText.gameObject.activeSelf)
                    m_UGUIText.gameObject.SetActive(true);
            }
        }

        private void OnDisable()
        {
            if(m_useUGUI && m_UGUIText)
            {
                if (m_UGUIText.gameObject.activeSelf)
                    m_UGUIText.gameObject.SetActive(false);
                m_UGUIText.text = "";
            }

            //防止脚本被关闭/删除后，导致"帧率限制"效果滞留
#if UNITY_EDITOR && !RUNTIME
            if (FrameRateLimt)
            {
                FrameRateLimt = false;
                Application.targetFrameRate = SrcTatgetFrameRate;
            }
#endif
        }

        private void OnDestroy()
        {
            if(m_instance == this)
                m_instance = null;
        }

        public void SetUGUITextContent(Text textContent)
        {
            m_UGUIText = textContent;
            if (m_UGUIText)
                m_useUGUI = true;
            else
                m_useUGUI = false;
        }

        private Queue<float> _timeQueue = new Queue<float>();
        private float _fps;

        public void Update()
        {
            int fNum = Math.Max((int) _fps, 10);

            _timeQueue.Enqueue(Time.unscaledDeltaTime);
            while (_timeQueue.Count > fNum)
            {
                _timeQueue.Dequeue();
            }
            float t = 0;
            foreach (float dt in _timeQueue)
            {
                t += dt;
            }
            _fps = _timeQueue.Count/t;

            if (m_useUGUI && m_UGUIText)
            {
                m_UGUIText.text = string.Format("FPS: \t{0}({1})\n", _fps.ToString("F3"), (1.0f / Time.unscaledDeltaTime).ToString("F3"))
                                + string.Format("FameTime(ms): \t{0}({1})\n", (1000f / _fps).ToString("F3"), (Time.unscaledDeltaTime * 1000).ToString("F3"))
                                + string.Format("Screen: \t{0}x{1}", Screen.width, Screen.height);
            }
            else
            {
                m_useUGUI = false;
            }

            if (FoucsFrameRateLimt)
            {                              //强制关闭垂直同步
                if (QualitySettings.vSyncCount > 0)
                    QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = FoucsTatgetFrameRate;
            }
        }

        private void OnGUI()
        {
            if (m_useUGUI) return;
            //GUI.Label(new Rect(Screen.width*0.5f + 500, 25, 100, 20),
            //    string.Format("FPS: {0}({1})", _fps, 1.0f/Time.unscaledDeltaTime), Style);
            //GUI.Label(new Rect(Screen.width*0.5f + 500, 45, 100, 20),
            //    string.Format("FameTime(ms): {0}({1})", 1000f/_fps, Time.unscaledDeltaTime * 1000), Style);
            //GUI.Label(new Rect(Screen.width * 0.5f + 500, 65, 100, 20),
            //    string.Format("Screen: {0}x{1}", Screen.width, Screen.height), Style);

            GUI.BeginGroup(new Rect(Screen.width * 2 / 5f + m_panelPosOffset.x, m_panelPosOffset.y, m_panelSize.x, m_panelSize.y));
            {
                GUILayout.Label(string.Format("FPS: {0}({1})", _fps.ToString("F3"), (1.0f / Time.unscaledDeltaTime).ToString("F3")), Style);
                GUILayout.Label(string.Format("FameTime(ms): {0}({1})", (1000f / _fps).ToString("F3"), (Time.unscaledDeltaTime * 1000).ToString("F3")), Style);
                GUILayout.Label(string.Format("Screen: {0}x{1}", Screen.width, Screen.height), Style);
            }
            GUI.EndGroup();

        }
    }
}