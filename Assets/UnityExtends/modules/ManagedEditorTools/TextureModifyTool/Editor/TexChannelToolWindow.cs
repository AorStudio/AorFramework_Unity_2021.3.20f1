using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

#if FRAMEWORKDEF
using AorBaseUtility;
using Framework.Extends;
#else
using AORCore;
using AORCore.Editor;
#endif

namespace UnityEngine.Rendering.Universal.Editor.Utility
{
    /// <summary>
    /// 位图通道分离/合并工具
    /// </summary>
    public class TexChannelToolWindow :UnityEditor.EditorWindow
    {

        private static GUIStyle _titleStyle;
        protected static GUIStyle titleStyle
        {
            get {
                if(_titleStyle == null)
                {
                    _titleStyle = new GUIStyle(EditorStyles.largeLabel);
                    _titleStyle.fontSize = 16;
                    _titleStyle.fontStyle = FontStyle.Bold;
                }
                return _titleStyle;
            }
        }

        private static GUIStyle _sTitleStyle;
        protected static GUIStyle sTitleStyle
        {
            get {
                if(_sTitleStyle == null)
                {
                    _sTitleStyle = new GUIStyle(EditorStyles.largeLabel);
                    _sTitleStyle.fontSize = 14;
                    _sTitleStyle.fontStyle = FontStyle.Bold;
                }
                return _sTitleStyle;
            }
        }

        //--------------------------------------------------------------

        public static void MergeTextureColorsUsingChannel(Texture2D target, TChannelMInfo info, ref Color[] colors)
        {

            bool changeRW = false;
            TextureImporterCompression ogCpn = TextureImporterCompression.Uncompressed;
            bool changeCompression = false;
            string path = AssetDatabase.GetAssetPath(target);
            TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath(path);
            if(importer)
            {
                if(!importer.isReadable)
                {
                    changeRW = true;
                    importer.isReadable = true;
                }

                if(!importer.textureCompression.Equals(TextureImporterCompression.Uncompressed))
                {
                    changeCompression = true;
                    ogCpn = importer.textureCompression;
                    importer.textureCompression = TextureImporterCompression.Uncompressed;
                }

                if(changeRW || changeCompression)
                {
                    importer.SaveAndReimport();
                }
            }

            Color[] srcColors = target.GetPixels();

            for(int i = 0; i < colors.Length; i++)
            {
                colors[i] = culColorByMInfo(colors[i], srcColors[i], info);
            }

            if(changeRW || changeCompression)
            {
                if(changeRW)
                {
                    importer.isReadable = false;
                }
                if(changeCompression)
                {
                    importer.textureCompression = ogCpn;
                }
                importer.SaveAndReimport();
            }

        }

        private static Color culColorByMInfo(Color tarColor, Color srcColor, TChannelMInfo info)
        {
            float r = tarColor.r;
            float g = tarColor.g;
            float b = tarColor.b;
            float a = tarColor.a;

            if(info.R != TChannel.Dispose)
                getTChannelValueByInfo(TChannel.R, srcColor, info.R, ref r, ref g, ref b, ref a);
            if(info.G != TChannel.Dispose)
                getTChannelValueByInfo(TChannel.G, srcColor, info.G, ref r, ref g, ref b, ref a);
            if(info.B != TChannel.Dispose)
                getTChannelValueByInfo(TChannel.B, srcColor, info.B, ref r, ref g, ref b, ref a);
            if(info.A != TChannel.Dispose)
                getTChannelValueByInfo(TChannel.A, srcColor, info.A, ref r, ref g, ref b, ref a);

            return new Color(r, g, b, a);
        }

        private static void getTChannelValueByInfo(TChannel channel, Color srcColor, TChannel inf, ref float r, ref float g, ref float b, ref float a)
        {

            switch(inf)
            {
                case TChannel.R:
                    {
                        switch(channel)
                        {
                            case TChannel.R:
                                {
                                    r += srcColor.r;
                                }
                                break;
                            case TChannel.G:
                                {
                                    r += srcColor.g;
                                }
                                break;
                            case TChannel.B:
                                {
                                    r += srcColor.b;
                                }
                                break;
                            case TChannel.A:
                                {
                                    r += srcColor.a;
                                }
                                break;
                        }
                    }
                break;
                case TChannel.G:
                    {
                        switch(channel)
                        {
                            case TChannel.R:
                                {
                                    g += srcColor.r;
                                }
                                break;
                            case TChannel.G:
                                {
                                    g += srcColor.g;
                                }
                                break;
                            case TChannel.B:
                                {
                                    g += srcColor.b;
                                }
                                break;
                            case TChannel.A:
                                {
                                    g += srcColor.a;
                                }
                                break;
                        }
                    }
                break;
                case TChannel.B:
                    {
                        switch(channel)
                        {
                            case TChannel.R:
                                {
                                    b += srcColor.r;
                                }
                                break;
                            case TChannel.G:
                                {
                                    b += srcColor.g;
                                }
                                break;
                            case TChannel.B:
                                {
                                    b += srcColor.b;
                                }
                                break;
                            case TChannel.A:
                                {
                                    b += srcColor.a;
                                }
                                break;
                        }
                    }
                break;
                case TChannel.A:
                    {
                        switch(channel)
                        {
                            case TChannel.R:
                                {
                                    a += srcColor.r;
                                }
                                break;
                            case TChannel.G:
                                {
                                    a += srcColor.g;
                                }
                                break;
                            case TChannel.B:
                                {
                                    a += srcColor.b;
                                }
                                break;
                            case TChannel.A:
                                {
                                    a += srcColor.a;
                                }
                                break;
                        }
                    }
                break;
                case TChannel.One:
                    {
                        switch(channel)
                        {
                            case TChannel.R:
                                {
                                    r = 1.0f;
                                }
                                break;
                            case TChannel.G:
                                {
                                    g = 1.0f;
                                }
                                break;
                            case TChannel.B:
                                {
                                    b = 1.0f;
                                }
                                break;
                            case TChannel.A:
                                {
                                    a = 1.0f;
                                }
                                break;
                        }
                    }
                break;
            }       


        }

        /// <summary>
        /// 从源图中分离某(多个)通道
        /// </summary>
        /// <param name="src">源图</param>
        /// <param name="tcInfo">通道分离信息</param>
        /// <returns>返回分离通道后的新图</returns>
        public static Texture2D SliceChannel(Texture2D src, TChannelInfo tcInfo, out TextureImporter importer)
        {
            bool changeRW = false;
            TextureImporterCompression ogCpn = TextureImporterCompression.Uncompressed;
            bool changeCompression = false;
            string path = AssetDatabase.GetAssetPath(src);
            importer = (TextureImporter)TextureImporter.GetAtPath(path);
            if(importer)
            {
                if(!importer.isReadable)
                {
                    changeRW = true;
                    importer.isReadable = true;
                }

                if(!importer.textureCompression.Equals(TextureImporterCompression.Uncompressed))
                {
                    changeCompression = true;
                    ogCpn = importer.textureCompression;
                    importer.textureCompression = TextureImporterCompression.Uncompressed;
                }

                if(changeRW || changeCompression)
                {
                    importer.SaveAndReimport();
                }
            }

            //            Texture2D tex = new Texture2D(src.width, src.height, src.format, src.mipmapCount > 0);
            Texture2D tex = new Texture2D(src.width, src.height, TextureFormat.RGBA32, src.mipmapCount > 0);
            Color[] srcColors = src.GetPixels();
            Color[] nColors = new Color[srcColors.Length];

            for(int i = 0; i < srcColors.Length; i++)
            {

                if(tcInfo.TTF == TFFormat.JPG && tcInfo.A)
                {
                    EditorUtility.DisplayDialog("提示", "分离JPG图片不支持Alpha通道,该操作被忽略.", "OK");
                    continue;
                }

                //仅分离出R通道,需要特殊处理
                if(!tcInfo.StrictMode && tcInfo.R && !tcInfo.G && !tcInfo.B && !tcInfo.A)
                {
                    nColors[i] = new Color(srcColors[i].r, srcColors[i].r, srcColors[i].r, 1);
                }
                //仅分离出G通道,需要特殊处理
                else if(!tcInfo.StrictMode && !tcInfo.R && tcInfo.G && !tcInfo.B && !tcInfo.A)
                {
                    nColors[i] = new Color(srcColors[i].g, srcColors[i].g, srcColors[i].g, 1);
                }
                //仅分离出B通道,需要特殊处理
                else if(!tcInfo.StrictMode && !tcInfo.R && !tcInfo.G && tcInfo.B && !tcInfo.A)
                {
                    nColors[i] = new Color(srcColors[i].b, srcColors[i].b, srcColors[i].b, 1);
                }
                //仅分离出A通道,需要特殊处理
                else if(!tcInfo.StrictMode && !tcInfo.R && !tcInfo.G && !tcInfo.B && tcInfo.A)
                {
                    nColors[i] = new Color(srcColors[i].a, srcColors[i].a, srcColors[i].a, 1);
                }
                //常规逻辑
                else
                {
                    if(tcInfo.StrictMode)
                    {
                        nColors[i] = new Color(
                                tcInfo.R ? srcColors[i].r : 0,
                                tcInfo.G ? srcColors[i].g : 0,
                                tcInfo.B ? srcColors[i].b : 0,
                                tcInfo.A ? srcColors[i].a : 0
                                );
                    }
                    else
                    {
                        nColors[i] = new Color(
                                tcInfo.R ? srcColors[i].r : 0,
                                tcInfo.G ? srcColors[i].g : 0,
                                tcInfo.B ? srcColors[i].b : 0,
                                tcInfo.A ? srcColors[i].a : 1
                                );
                    }
                }

            }

            tex.SetPixels(nColors);
            tex.Apply();

            if(changeRW || changeCompression)
            {
                if(changeRW)
                {
                    importer.isReadable = false;
                }
                if(changeCompression)
                {
                    importer.textureCompression = ogCpn;
                }
                importer.SaveAndReimport();
            }

            return tex;
        }

        public static void SaveTextureToFile(Texture2D tex, TextureImporter srcImporter, string path)
        {
            byte[] bytes;
            EditorAssetInfo editorAssetInfo = new EditorAssetInfo(path);
            switch(editorAssetInfo.suffix.ToLower())
            {
                case ".jpg":
                    {
                        bytes = tex.EncodeToJPG();
                    }
                break;
                case ".tga":
                    {
                        bytes = tex.EncodeToTGA();
                    }
                    break;
                default:
                    {
                        bytes = tex.EncodeToPNG();
                    }
                break;
            }

            AorIO.SaveBytesToFile(path, bytes);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath(path);
            if(importer)
            {
                importer.textureType = srcImporter.textureType;
                importer.textureShape = srcImporter.textureShape;
                importer.sRGBTexture = srcImporter.sRGBTexture;
                importer.alphaSource = srcImporter.alphaSource;
                importer.alphaIsTransparency = srcImporter.alphaIsTransparency;

                importer.npotScale = srcImporter.npotScale;
                importer.isReadable = srcImporter.isReadable;

                importer.mipmapEnabled = srcImporter.mipmapEnabled;
                importer.borderMipmap = srcImporter.borderMipmap;
                importer.mipmapFilter = srcImporter.mipmapFilter;
                importer.fadeout = srcImporter.fadeout;
                importer.mipmapFadeDistanceStart = srcImporter.mipmapFadeDistanceStart;
                importer.mipmapFadeDistanceEnd = srcImporter.mipmapFadeDistanceEnd;

                importer.wrapMode = srcImporter.wrapMode;
                importer.filterMode = srcImporter.filterMode;
                importer.anisoLevel = srcImporter.anisoLevel;

                importer.textureCompression = srcImporter.textureCompression;
                importer.allowAlphaSplitting = srcImporter.allowAlphaSplitting;
                importer.compressionQuality = srcImporter.compressionQuality;
                importer.crunchedCompression = srcImporter.crunchedCompression;
                importer.maxTextureSize = srcImporter.maxTextureSize;
                //etc ...

                importer.SetPlatformTextureSettings(srcImporter.GetDefaultPlatformTextureSettings());

                importer.SaveAndReimport();
            }

        }

        //-------------------------------------------------------------

        private static TexChannelToolWindow _instance;

        [MenuItem("Window/FrameworkTools/Bitmaps/Texture2D 通道分离(合并)工具")]
        public static TexChannelToolWindow init()
        {

            _instance = UnityEditor.EditorWindow.GetWindow<TexChannelToolWindow>();
            _instance.minSize = new Vector2(495, 612);

            return _instance;
        }

        public enum TChannel 
        { 
            Dispose,
            R,
            G,
            B,
            A,
            One
        }

        public struct TChannelMInfo
        {

            public TChannelMInfo(TChannel R,TChannel G,TChannel B, TChannel A)
            {
                this.R = R;
                this.G = G;
                this.B = B;
                this.A = A;
            }

            public TChannel R;
            public TChannel G;
            public TChannel B;
            public TChannel A;

            public string ToShortString()
            {
                return "_" 
                    + (R != TChannel.Dispose ? R.ToString() : "0")
                    + (G != TChannel.Dispose ? G.ToString() : "0")
                    + (B != TChannel.Dispose ? B.ToString() : "0")
                    + (A != TChannel.Dispose ? A.ToString() : "0");
            }

        }

        public enum TFFormat
        {
            PNG,
            TGA,
            JPG
        }

        public struct TChannelInfo
        {

            public TChannelInfo(bool r, bool g, bool b, bool a)
            {
                this.R = r;
                this.G = g;
                this.B = b;
                this.A = a;
                this.TTF = TFFormat.PNG;
                this.StrictMode = false;
            }

            public TChannelInfo(bool r, bool g, bool b, bool a, TFFormat ttf)
            {
                this.R = r;
                this.G = g;
                this.B = b;
                this.A = a;
                this.TTF = ttf;
                this.StrictMode = false;
            }

            public TChannelInfo(bool r, bool g, bool b, bool a, TFFormat ttf, bool StrictMode)
            {
                this.R = r;
                this.G = g;
                this.B = b;
                this.A = a;
                this.TTF = ttf;
                this.StrictMode = StrictMode;
            }

            public bool R;
            public bool G;
            public bool B;
            public bool A;
            public TFFormat TTF;
            public bool StrictMode;

            public string GetSuffix()
            {
                switch(this.TTF)
                {
                    case TFFormat.JPG: return ".jpg";
                    case TFFormat.TGA: return ".tga";
                    default: return ".png";
                }
            }

            public string ToShortString()
            {
                return "_" + (R ? "R" : "") + (G ? "G" : "") + (B ? "B" : "") + (A ? "A" : "");
            }

        }

        private static string[] _menuLabels = new string[] { "通道分离", "通道(映射)合并" };
        private static int m_menuIndex;
        private Vector2 _scrollPos = new Vector2();
        private void OnGUI()
        {
            _scrollPos = GUILayout.BeginScrollView(_scrollPos);
            {
                GUILayout.Space(15);
                _draw_toolTitle_UI();
                GUILayout.Space(15);

                m_menuIndex = GUILayout.Toolbar(m_menuIndex, _menuLabels, GUILayout.Height(28));
                switch(m_menuIndex)
                {
                    case 1:
                        {
                            _draw_savePath_UI(true);
                            _draw_MergeTex_UI();
                        }
                    break;
                    default:
                        {
                            _draw_savePath_UI();
                            _draw_SliceChannel_UI();
                        }
                    break;
                }

                GUILayout.Space(15);
            }
            GUILayout.EndScrollView();

            //EditorPlusMethods.Draw_DebugWindowSizeUI();
        }

        //--------------------------------------

        private void _draw_toolTitle_UI()
        {
            GUILayout.BeginVertical("box");
            {
                GUILayout.Space(10);

                GUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("      位图通道编辑工具      ", titleStyle);
                    GUILayout.FlexibleSpace();
                }
                GUILayout.EndHorizontal();

                GUILayout.Space(10);

                GUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("提示: 本工具仅支持PNG/JPG/TGA位图文件.");
                    GUILayout.FlexibleSpace();
                }
                GUILayout.EndHorizontal();

                GUILayout.Space(10);
            }
            GUILayout.EndVertical();
        }

        //=============================================================================

        private bool m_useSrcTexPathDir = true;

        private string m_savePath;

        private void verifySavePath(Texture2D src)
        {
            if(m_useSrcTexPathDir)
            {
                string srcPath = AssetDatabase.GetAssetPath(src);
                if(!string.IsNullOrEmpty(srcPath))
                {
                    EditorAssetInfo info = new EditorAssetInfo(srcPath);
                    m_savePath = info.dirPath;
                }
            }
        }

        private void _draw_savePath_UI(bool focus = false)
        {
            GUILayout.BeginVertical("box");
            {
                GUILayout.Space(5);
                _draw_subTitle_UI("------ 设置生成路径 ------");
                GUILayout.Space(5);

                if(focus)
                {
                    m_useSrcTexPathDir = false;
                }
                else
                {
                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.Space(24);
                        GUILayout.Label("使用源图片所在的文件夹作为生成路径");
                        GUILayout.FlexibleSpace();
                        m_useSrcTexPathDir = EditorGUILayout.Toggle(m_useSrcTexPathDir);
                        GUILayout.FlexibleSpace();
                    }
                    GUILayout.EndHorizontal();
                }

                if(!m_useSrcTexPathDir)
                {

                    GUILayout.Space(5);

                    GUILayout.BeginVertical("box");
                    {
                        GUILayout.Space(5);

                        GUILayout.BeginHorizontal();
                        {
                            GUILayout.FlexibleSpace();
                            GUILayout.Label("设置生成路径");
                            GUILayout.FlexibleSpace();
                        }
                        GUILayout.EndHorizontal();

                        GUILayout.Space(5);

                        GUILayout.BeginHorizontal();
                        {
                            m_savePath = EditorGUILayout.TextField(m_savePath);
                            if(GUILayout.Button("UseSelection", GUILayout.Width(120)))
                            {
                                if(Selection.activeObject)
                                {
                                    string tp = AssetDatabase.GetAssetPath(Selection.activeObject);
                                    if(!string.IsNullOrEmpty(tp))
                                    {

                                        EditorAssetInfo info = new EditorAssetInfo(tp);
                                        m_savePath = info.dirPath;

                                    }
                                    else
                                    {
                                        m_savePath = "";
                                    }
                                }
                                else
                                {
                                    m_savePath = "";
                                }
                            }
                            if(GUILayout.Button("Set", GUILayout.Width(50)))
                            {
                                m_savePath = EditorUtility.SaveFolderPanel("设置保存路径", "", "");
                                m_savePath = m_savePath.Replace(Application.dataPath, "Assets");
                            }
                        }
                        GUILayout.EndHorizontal();

                        GUILayout.Space(5);
                    }
                    GUILayout.EndVertical();
                }
                GUILayout.Space(5);
            }
            GUILayout.EndVertical();
        }

        //=============================================================================

        private Texture2D _srcTexture;
        private readonly List<TChannelInfo> _sliceList = new List<TChannelInfo>();
        private int _sid = 0;
        private static string[] _tagLabels = {"快捷分离","高级分离"};

        private void _draw_SliceChannel_UI()
        {

            GUILayout.BeginVertical("box");
            {
                GUILayout.Space(5);
                _draw_subTitle_UI("------ 分离通道 ------");
                GUILayout.Space(5);

                _sid = GUILayout.Toolbar(_sid, _tagLabels);

                GUILayout.Space(5);

                GUILayout.BeginHorizontal("box");
                {
                    GUILayout.Label("源图");
                    _srcTexture = (Texture2D)EditorGUILayout.ObjectField(_srcTexture, typeof(Texture2D), false);
                    if(GUILayout.Button("SetFormSelection", GUILayout.Width(120)))
                    {
                        if(Selection.objects.Length > 0)
                        {
                            if(Selection.objects[0] is Texture2D)
                            {
                                _srcTexture = (Texture2D)Selection.objects[0];
                            }
                        }
                    }
                }
                GUILayout.EndHorizontal();

                GUILayout.Space(5);


                switch(_sid)
                {
                    case 1:
                        {

                            GUILayout.BeginVertical("box");
                            {
                                GUILayout.Space(5);
                                GUILayout.Label("提示信息: 勾选<严格模式>将严格按照通道信息分离图片.");
                                GUILayout.Space(5);
                            }
                            GUILayout.EndVertical();

                            GUILayout.BeginVertical("box");
                            {

                                GUILayout.Space(5);
                                GUILayout.Label("设置");
                                GUILayout.Space(5);

                                if(_sliceList.Count == 0)
                                {
                                    _draw_noDataTip_UI();
                                }

                                for(var i = 0; i < _sliceList.Count; i++)
                                {

                                    if(i > 0)
                                    {
                                        GUILayout.Space(2);
                                    }

                                    GUILayout.BeginHorizontal();

                                    GUILayout.Label("No." + (i + 1));

                                    GUILayout.FlexibleSpace();

                                    bool r = _sliceList[i].R;
                                    bool g = _sliceList[i].G;
                                    bool b = _sliceList[i].B;
                                    bool a = _sliceList[i].A;
                                    TFFormat ttf = _sliceList[i].TTF;
                                    bool sm = _sliceList[i].StrictMode;

                                    bool dirty = __draw_TChannelInfo_UI(ref r, ref g, ref b, ref a, ref ttf, ref sm);
                                    if(dirty)
                                    {
                                        _sliceList[i] = new TChannelInfo(r, g, b, a, ttf, sm);
                                    }

                                    if(GUILayout.Button("-", GUILayout.Width(50)))
                                    {

                                        _sliceList.RemoveAt(i);
                                        Repaint();
                                        return;
                                    }

                                    GUILayout.EndHorizontal();
                                }

                                GUILayout.BeginHorizontal();
                                {
                                    GUILayout.FlexibleSpace();

                                    if(GUILayout.Button("+", GUILayout.Width(200), GUILayout.Height(22)))
                                    {
                                        _sliceList.Add(new TChannelInfo());
                                        Repaint();
                                        return;
                                    }
                                }
                                GUILayout.EndHorizontal();

                            }
                            GUILayout.EndVertical();

                            //GUILayout.Space(5);
                            GUILayout.FlexibleSpace();

                            //--------------------

                            if(GUILayout.Button("Start", GUILayout.Height(28)))
                            {

                                if(!_srcTexture)
                                {
                                    //Error :: 源图为空
                                    EditorUtility.DisplayDialog("提示", "未设置源图片,请设置源图片.", "确定");
                                    return;
                                }

                                if(!_vaildInputData(_sliceList))
                                    return;

                                for(var i = 0; i < _sliceList.Count; i++)
                                {


                                    EditorUtility.DisplayProgressBar("处理中..", "正在生成分离文件..." + i + " / " + _sliceList.Count, (float)i / _sliceList.Count);

                                    TChannelInfo info = _sliceList[i];
                                    TextureImporter importer;
                                    Texture2D nTex = SliceChannel(_srcTexture, info, out importer);
                                    if(nTex)
                                    {
                                        string nfName = _srcTexture.name + info.ToShortString() + info.GetSuffix();
                                        verifySavePath(_srcTexture);
                                        string path = m_savePath + "/" + nfName;

                                        //save;
                                        SaveTextureToFile(nTex, importer, path);
                                    }
                                    else
                                    {
                                        //Error 分离失败
                                    }

                                }

                                EditorUtility.ClearProgressBar();

                            }
                        }
                        break;

                    default: //0
                        {
                            GUILayout.BeginVertical("box");
                            {
                                GUILayout.Space(5);
                                GUILayout.Label("信息:快速模式将源图按非严格模式拆分为RBG和Alpha两张PNG图片.");
                                GUILayout.Space(5);
                            }
                            GUILayout.EndVertical();

                            GUILayout.FlexibleSpace();

                            if(GUILayout.Button("Start", GUILayout.Height(28)))
                            {

                                if(!_srcTexture)
                                {
                                    //Error :: 源图为空
                                    EditorUtility.DisplayDialog("提示", "未设置源图片,请设置源图片.", "确定");
                                    return;
                                }

                                //生成快速Info
                                List<TChannelInfo> _tmpInfos = new List<TChannelInfo>();
                                _tmpInfos.Add(new TChannelInfo(true, true, true, false));
                                _tmpInfos.Add(new TChannelInfo(false, false, false, true));

                                for(var i = 0; i < _tmpInfos.Count; i++)
                                {

                                    EditorUtility.DisplayProgressBar("处理中..", "正在生成分离文件..." + i + " / " + _tmpInfos.Count, (float)i / _tmpInfos.Count);

                                    TChannelInfo info = _tmpInfos[i];
                                    TextureImporter importer;
                                    Texture2D nTex = SliceChannel(_srcTexture, info, out importer);
                                    if(nTex)
                                    {
                                        string nfName = _srcTexture.name + info.ToShortString() + info.GetSuffix();
                                        verifySavePath(_srcTexture);
                                        string path = m_savePath + "/" + nfName;

                                        //save;
                                        SaveTextureToFile(nTex, importer, path);
                                    }
                                    else
                                    {
                                        //Error 分离失败
                                    }

                                }

                                _tmpInfos.Clear();
                                EditorUtility.ClearProgressBar();

                            }
                        }
                        break;
                }
                GUILayout.Space(5);
            }
            GUILayout.EndVertical();

        }

        //=============================================================================

        private readonly List<TChannelMInfo> _mergeInfoList = new List<TChannelMInfo>();
        private readonly List<Texture2D> _srcMergeList = new List<Texture2D>();

        private void _draw_MergeTex_UI()
        {
            GUILayout.BeginVertical("box");
            {
                GUILayout.Space(5);
                _draw_subTitle_UI("------ 合并通道 ------");
                GUILayout.Space(5);

                GUILayout.BeginVertical("box");
                {

                    GUILayout.Space(5);
                    GUILayout.Label("设置");
                    GUILayout.Space(5);

                    if(_mergeInfoList.Count == 0)
                    {
                        _draw_noDataTip_UI();
                    }

                    for(int i = 0; i < _mergeInfoList.Count; i++)
                    {
                        if(i > 0)
                        {
                            GUILayout.Space(2);
                        }

                        GUILayout.BeginVertical("box");
                        {
                            GUILayout.BeginHorizontal();
                            {
                                GUILayout.Label("No." + (i + 1));

                                GUILayout.FlexibleSpace();

                                _srcMergeList[i] = (Texture2D)EditorGUILayout.ObjectField(_srcMergeList[i], typeof(Texture2D), false);
                                if(GUILayout.Button(new GUIContent("set", "Set Form Selection"), GUILayout.Width(32)))
                                {
                                    if(Selection.objects.Length > 0)
                                    {
                                        if(Selection.objects[0] is Texture2D)
                                        {
                                            _srcMergeList[i] = (Texture2D)Selection.objects[0];
                                        }
                                    }
                                }
                            }

                            GUILayout.EndHorizontal();
                            if(_srcMergeList[i])
                            {
                                GUILayout.BeginHorizontal();
                                {
                                    TChannel r = _mergeInfoList[i].R;
                                    TChannel g = _mergeInfoList[i].G;
                                    TChannel b = _mergeInfoList[i].B;
                                    TChannel a = _mergeInfoList[i].A;
                                    bool dirty = __draw_TChannelMInfo_UI(ref r, ref g, ref b, ref a);
                                    if(dirty)
                                    {
                                        _mergeInfoList[i] = new TChannelMInfo(r, g, b, a);
                                    }

                                    GUILayout.FlexibleSpace();

                                    if(GUILayout.Button("-", GUILayout.Width(50)))
                                    {
                                        _mergeInfoList.RemoveAt(i);
                                        _srcMergeList.RemoveAt(i);
                                        Repaint();
                                        return;
                                    }
                                }
                                GUILayout.EndHorizontal();
                            }
                            else
                            {
                                GUILayout.BeginHorizontal();
                                {
                                    GUILayout.FlexibleSpace();
                                    if(GUILayout.Button("-", GUILayout.Width(100)))
                                    {
                                        _mergeInfoList.RemoveAt(i);
                                        _srcMergeList.RemoveAt(i);
                                        Repaint();
                                        return;
                                    }
                                }
                                GUILayout.EndHorizontal();

                            }
                        }
                        GUILayout.EndVertical();

                    }

                    GUILayout.BeginHorizontal();
                    {

                        GUILayout.FlexibleSpace();

                        if(GUILayout.Button("+", GUILayout.Width(200), GUILayout.Height(22)))
                        {
                            _mergeInfoList.Add(new TChannelMInfo());
                            _srcMergeList.Add(null);
                            Repaint();
                            return;
                        }

                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();

                GUILayout.Space(5);

                if (GUILayout.Button("Start", GUILayout.Height(28)))
                {
                    if (!_vaildInputData(_mergeInfoList)) return;

                    //检查 _srcMergeList里面的图是否一样大
                    bool isSameSize = true;
                    int w = 0, h = 0;
                    bool mipmap = false;
                    string cacheTexName = "uname";
                    TextureImporter cacheImporter = null;

                    for (int v = 0; v < _srcMergeList.Count; v++)
                    {
                        if (v > 0)
                        {
                            if (w != _srcMergeList[v].width || h != _srcMergeList[v].height)
                            {
                                isSameSize = false;
                                break;
                            }
                        }
                        else
                        {
                            w = _srcMergeList[v].width;
                            h = _srcMergeList[v].height;
                            mipmap = _srcMergeList[v].mipmapCount > 0;
                            string path = AssetDatabase.GetAssetPath(_srcMergeList[v]);
                            cacheImporter = (TextureImporter)TextureImporter.GetAtPath(path);
                            cacheTexName = _srcMergeList[v].name;
                        }
                    }

                    if (isSameSize)
                    {
                        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, mipmap);
                        Color[] nColors = new Color[w * h];
                        for (var i = 0; i < _mergeInfoList.Count; i++)
                        {

                            EditorUtility.DisplayProgressBar("处理中..", "正在合并通道..." + i + " / " + _mergeInfoList.Count, (float)i / _mergeInfoList.Count);

                            if (_srcMergeList[i])
                            {
                                MergeTextureColorsUsingChannel(_srcMergeList[i], _mergeInfoList[i], ref nColors);
                            }
                        }

                        EditorUtility.ClearProgressBar();

                        tex.SetPixels(nColors);
                        tex.Apply();

                        string path = m_savePath + "/" + cacheTexName + "_merged.png";

                        SaveTextureToFile(tex, cacheImporter, path);

                    }
                    else
                    {
                        //Error :: 输入的图不一样大
                        EditorUtility.DisplayDialog("警告", "参与合并的位图大小不一致,导致通道合并失败.", "确定");
                    }

                }

                GUILayout.Space(5);
            }
            GUILayout.EndVertical();
        }

        //-----------------------------------------------------------------

        private bool _vaildInputData(IList list)
        {
            if (!m_useSrcTexPathDir && string.IsNullOrEmpty(m_savePath))
            {
                //Error :: 保存路径为空
                EditorUtility.DisplayDialog("提示", "保存路径未设置,请设置保存路径.", "确定");
                return false;
            }

            if (list.Count == 0)
            {
                EditorUtility.DisplayDialog("提示", "通道数据未设置,请添加通道设置.", "确定");
                //Error :: 导出图设置为空
                return false;
            }

            return true;
        }

        private bool __draw_TChannelInfo_UI(ref bool r, ref bool g, ref bool b, ref bool a, ref TFFormat ttf, ref bool strictMode)
        {
            bool dirty = false;

            bool nr = EditorGUILayout.ToggleLeft("R", r, GUILayout.Width(50));
            if (nr != r)
            {
                r = nr;
                dirty = true;
            }
            bool ng = EditorGUILayout.ToggleLeft("G", g, GUILayout.Width(50));
            if (ng != g)
            {
                g = ng;
                dirty = true;
            }
            bool nb = EditorGUILayout.ToggleLeft("B", b, GUILayout.Width(50));
            if (nb != b)
            {
                b = nb;
                dirty = true;
            }
            bool na = EditorGUILayout.ToggleLeft("A", a, GUILayout.Width(50));
            if (na != a)
            {
                a = na;
                dirty = true;
            }
            TFFormat nttf = (TFFormat)EditorGUILayout.EnumPopup(ttf, GUILayout.Width(80));
            if(nttf != ttf)
            {
                ttf = nttf;
                dirty = true;
            }
            bool nStrictMode = EditorGUILayout.ToggleLeft("严格模式", strictMode, GUILayout.Width(80));
            if(nStrictMode != strictMode)
            {
                strictMode = nStrictMode;
                dirty = true;
            }
            return dirty;
        }

        private bool __draw_TChannelMInfo_UI(ref TChannel r, ref TChannel g, ref TChannel b, ref TChannel a)
        {
            bool dirty = false;
            float labelWidth = 12;
            float popupWidth = 80;
            float interval = 5;
            GUILayout.BeginHorizontal("box");
            {

                GUILayout.Space(48);

                GUILayout.Label("目标通道设置");

                GUILayout.FlexibleSpace();

                GUILayout.Label("R", GUILayout.Width(labelWidth));
                TChannel nr = (TChannel)EditorGUILayout.EnumPopup(r, GUILayout.Width(popupWidth));
                if(nr != r)
                {
                    r = nr;
                    dirty = true;
                }
                GUILayout.Space(interval);
                GUILayout.Label("G", GUILayout.Width(labelWidth));
                TChannel ng = (TChannel)EditorGUILayout.EnumPopup(g, GUILayout.Width(popupWidth));
                if(ng != g)
                {
                    g = ng;
                    dirty = true;
                }
                GUILayout.Space(interval);
                GUILayout.Label("B", GUILayout.Width(labelWidth));
                TChannel nb = (TChannel)EditorGUILayout.EnumPopup(b, GUILayout.Width(popupWidth));
                if(nb != b)
                {
                    b = nb;
                    dirty = true;
                }
                GUILayout.Space(interval);
                GUILayout.Label("A", GUILayout.Width(labelWidth));
                TChannel na = (TChannel)EditorGUILayout.EnumPopup(a, GUILayout.Width(popupWidth));
                if(na != a)
                {
                    a = na;
                    dirty = true;
                }
            }
            GUILayout.EndHorizontal();

            return dirty;
        }

        private void _draw_noDataTip_UI()
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Space(32);
                GUILayout.Label("<-- 暂无数据, 请单击 \"+\" 键添加数据. -->");
            }
            GUILayout.EndHorizontal();
        }

        private void _draw_subTitle_UI(string label)
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                GUILayout.Label(label, sTitleStyle);
                GUILayout.FlexibleSpace();
            }
            GUILayout.EndHorizontal();
        }

    }
}


