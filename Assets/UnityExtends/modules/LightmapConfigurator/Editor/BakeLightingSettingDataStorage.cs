using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;

namespace UnityEngine.Rendering.Universal.LightmapConfigurator.Editor
{
    [Serializable]
    public class BakeLightingSettingDataStorage
    {

        //[MenuItem("DevTest/Save")]
        public static void CreateAndSaveToJSON()
        {
            string savePath = EditorUtility.SaveFilePanelInProject("save", "BakeLightingSettingDataStorage", "bls", "保存");
            if (!string.IsNullOrEmpty(savePath))
            {
                var data = BakeLightingSettingDataStorage.Create();
                if (File.Exists(savePath))
                    File.Delete(savePath);
                File.WriteAllText(savePath, JsonUtility.ToJson(data), new UTF8Encoding(false));
            }
        }

        public static void LoadJSONAndApplyToEnv()
        {
            string loadPath = EditorUtility.OpenFilePanel("Open JSONFile", "", "bls");
            if (!string.IsNullOrEmpty(loadPath))
            {
                string json = File.ReadAllText(loadPath);
                if (!string.IsNullOrEmpty(json))
                {
                    BakeLightingSettingDataStorage storgae = JsonUtility.FromJson<BakeLightingSettingDataStorage>(json);
                    storgae.ApplyDatas();
                }
            }
        }

        public static BakeLightingSettingDataStorage Create()
        {
            var storage = new BakeLightingSettingDataStorage();
            storage.LightmappingSetting = LightmappingSettingDataStorage.Create();
            storage.RenderSetting = RenderSettingDataStorage.Create();
            return storage;
        }

        public LightmappingSettingDataStorage LightmappingSetting;
        public RenderSettingDataStorage RenderSetting;

        public void ApplyDatas()
        {
            LightmappingSetting.ApplyDatas();
            RenderSetting.ApplyDatas();
        }

    }

    [Serializable]
    public class RenderSettingDataStorage
    {   
        public static RenderSettingDataStorage Create()
        {
            var storage = new RenderSettingDataStorage();

            //RenderSettings.skybox
            if (RenderSettings.skybox)
            {
                if (RenderSettings.skybox.name.ToLower() == "default-skybox")
                    storage.GUID_skybox = "default-skybox";
                else
                    storage.GUID_skybox = AssetDatabase.GUIDToAssetPath(AssetDatabase.GetAssetPath(RenderSettings.skybox));
            }
            else
            {
                storage.GUID_skybox = "";
            }
            storage.ambientMode = RenderSettings.ambientMode;
            storage.ambientSkyColor = RenderSettings.ambientSkyColor;
            storage.ambientEquatorColor = RenderSettings.ambientEquatorColor;
            storage.ambientGroundColor = RenderSettings.ambientGroundColor;
            storage.ambientLight = RenderSettings.ambientLight;
            storage.ambientProbe = SHL2Wraper.CreateFromSphericalHarmonicsL2(RenderSettings.ambientProbe);
            storage.ambientIntensity = RenderSettings.ambientIntensity;
            storage.defaultReflectionMode = RenderSettings.defaultReflectionMode;
            storage.defaultReflectionResolution = RenderSettings.defaultReflectionResolution;
            storage.subtractiveShadowColor = RenderSettings.subtractiveShadowColor;
            storage.reflectionIntensity = RenderSettings.reflectionIntensity;
            storage.reflectionBounces = RenderSettings.reflectionBounces;
            storage.GUID_customReflection = RenderSettings.customReflection != null ? AssetDatabase.GUIDToAssetPath(AssetDatabase.GetAssetPath(RenderSettings.customReflection)) : "";
            storage.haloStrength = RenderSettings.haloStrength;
            storage.flareStrength = RenderSettings.flareStrength;
            storage.flareFadeSpeed = RenderSettings.flareFadeSpeed;
            storage.fog = RenderSettings.fog;
            storage.fogColor = RenderSettings.fogColor;
            storage.fogDensity = RenderSettings.fogDensity;
            storage.fogStartDistance = RenderSettings.fogStartDistance;
            storage.fogEndDistance = RenderSettings.fogEndDistance;
            storage.fogMode = RenderSettings.fogMode;
            return storage;
        }
        
        public string GUID_skybox;
        //public string GUIDOrInstanceID_sun; //Unity有自己的管理光源的方式，故此配置无需保存RenderSettings.run
        public AmbientMode ambientMode; //UnityEngine.Rendering.AmbientMode
        public Color ambientSkyColor;
        public Color ambientEquatorColor;
        public Color ambientGroundColor;
        public Color ambientLight;
        public SHL2Wraper ambientProbe;
        public float ambientIntensity;
        public DefaultReflectionMode defaultReflectionMode;
        public int defaultReflectionResolution;
        public Color subtractiveShadowColor;
        public float reflectionIntensity;
        public int reflectionBounces;
        public string GUID_customReflection;
        public float haloStrength;
        public float flareStrength;
        public float flareFadeSpeed;
        public bool fog;
        public Color fogColor;
        public float fogDensity;
        public float fogStartDistance;
        public float fogEndDistance;
        public FogMode fogMode;

        public void ApplyDatas()
        {
            RenderSettings.skybox = string.IsNullOrEmpty(GUID_skybox) ? null : AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(GUID_skybox));

            
            if (!string.IsNullOrEmpty(GUID_skybox))
            { 
                if (GUID_skybox.ToLower() == "default-skybox")
                {
                    RenderSettings.skybox = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Skybox.mat");
                }
                else
                    RenderSettings.skybox = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(GUID_skybox));
            }

            //RenderSettings.sun = string.IsNullOrEmpty(GUIDOrInstanceID_sun) ? null : EditorUtility.InstanceIDToObject(int.Parse(GUIDOrInstanceID_sun)) as Light;
            RenderSettings.ambientMode = ambientMode;
            RenderSettings.ambientSkyColor = ambientSkyColor;
            RenderSettings.ambientEquatorColor = ambientEquatorColor;
            RenderSettings.ambientGroundColor = ambientGroundColor;
            RenderSettings.ambientLight = ambientLight;
            RenderSettings.ambientProbe = ambientProbe.ToSphericalHarmonicsL2();
            RenderSettings.ambientIntensity = ambientIntensity;
            RenderSettings.defaultReflectionMode = defaultReflectionMode;
            RenderSettings.defaultReflectionResolution = defaultReflectionResolution;
            RenderSettings.subtractiveShadowColor = subtractiveShadowColor;
            RenderSettings.reflectionIntensity = reflectionIntensity;
            RenderSettings.reflectionBounces = reflectionBounces;
            RenderSettings.customReflection = string.IsNullOrEmpty(GUID_customReflection) ? null : AssetDatabase.LoadAssetAtPath<Cubemap>(AssetDatabase.GUIDToAssetPath(GUID_customReflection));
            RenderSettings.haloStrength = haloStrength;
            RenderSettings.flareStrength = flareStrength;
            RenderSettings.flareFadeSpeed = flareFadeSpeed;
            RenderSettings.fog = fog;
            RenderSettings.fogColor = fogColor;
            RenderSettings.fogDensity = fogDensity;
            RenderSettings.fogStartDistance = fogStartDistance;
            RenderSettings.fogEndDistance = fogEndDistance;
            RenderSettings.fogMode = fogMode;
        }

    }

    [Serializable]
    public struct SHL2Wraper
    {
        public static SHL2Wraper CreateFromSphericalHarmonicsL2(SphericalHarmonicsL2 shl2)
        {
            return new SHL2Wraper
            {
                shr0 = shl2[0, 0],
                shr1 = shl2[0, 1],
                shr2 = shl2[0, 2],
                shr3 = shl2[0, 3],
                shr4 = shl2[0, 4],
                shr5 = shl2[0, 5],
                shr6 = shl2[0, 6],
                shr7 = shl2[0, 7],
                shr8 = shl2[0, 8],
                shg0 = shl2[1, 0],
                shg1 = shl2[1, 1],
                shg2 = shl2[1, 2],
                shg3 = shl2[1, 3],
                shg4 = shl2[1, 4],
                shg5 = shl2[1, 5],
                shg6 = shl2[1, 6],
                shg7 = shl2[1, 7],
                shg8 = shl2[1, 8],
                shb0 = shl2[2, 0],
                shb1 = shl2[2, 1],
                shb2 = shl2[2, 2],
                shb3 = shl2[2, 3],
                shb4 = shl2[2, 4],
                shb5 = shl2[2, 5],
                shb6 = shl2[2, 6],
                shb7 = shl2[2, 7],
                shb8 = shl2[2, 8]
            };
        }

        public float shr0;
        public float shr1;
        public float shr2;
        public float shr3;
        public float shr4;
        public float shr5;
        public float shr6;
        public float shr7;
        public float shr8;
        public float shg0;
        public float shg1;
        public float shg2;
        public float shg3;
        public float shg4;
        public float shg5;
        public float shg6;
        public float shg7;
        public float shg8;
        public float shb0;
        public float shb1;
        public float shb2;
        public float shb3;
        public float shb4;
        public float shb5;
        public float shb6;
        public float shb7;
        public float shb8;

        public SphericalHarmonicsL2 ToSphericalHarmonicsL2()
        {
            var shl2 = new SphericalHarmonicsL2();
            shl2[0, 0] = shr0;
            shl2[0, 1] = shr1;
            shl2[0, 2] = shr2;
            shl2[0, 3] = shr3;
            shl2[0, 4] = shr4;
            shl2[0, 5] = shr5;
            shl2[0, 6] = shr6;
            shl2[0, 7] = shr7;
            shl2[0, 8] = shr8;
            shl2[1, 0] = shg0;
            shl2[1, 1] = shg1;
            shl2[1, 2] = shg2;
            shl2[1, 3] = shg3;
            shl2[1, 4] = shg4;
            shl2[1, 5] = shg5;
            shl2[1, 6] = shg6;
            shl2[1, 7] = shg7;
            shl2[1, 8] = shg8;
            shl2[2, 0] = shb0;
            shl2[2, 1] = shb1;
            shl2[2, 2] = shb2;
            shl2[2, 3] = shb3;
            shl2[2, 4] = shb4;
            shl2[2, 5] = shb5;
            shl2[2, 6] = shb6;
            shl2[2, 7] = shb7;
            shl2[2, 8] = shb8;
            return shl2;
        }

    }

    [Serializable]
    public class LightmappingSettingDataStorage 
    {
        public static LightmappingSettingDataStorage Create()
        {
            Type lightmapEditorSettingsType = typeof(LightmapEditorSettings);
            LightmappingSettingDataStorage ins = new LightmappingSettingDataStorage();
            FieldInfo[] fieldInfos = typeof(LightmappingSettingDataStorage).GetFields(BindingFlags.Instance | BindingFlags.Public);
            foreach (FieldInfo fieldInfo in fieldInfos)
            {
                fieldInfo.SetValue(ins, lightmapEditorSettingsType.GetProperty(fieldInfo.Name, BindingFlags.Static | BindingFlags.Public).GetValue(null));
            }
            return ins;
        }

        public LightmapEditorSettings.Lightmapper lightmapper;
        public LightmapsMode lightmapsMode;
        public MixedLightingMode mixedBakeMode;
        public LightmapEditorSettings.Sampling sampling;
        public int directSampleCount;
        public int indirectSampleCount;
        public int bounces;
        public bool prioritizeView;
        public LightmapEditorSettings.FilterMode filteringMode;
        public LightmapEditorSettings.DenoiserType denoiserTypeDirect;
        public LightmapEditorSettings.DenoiserType denoiserTypeIndirect;
        public LightmapEditorSettings.DenoiserType denoiserTypeAO;
        public LightmapEditorSettings.FilterType filterTypeDirect;
        public LightmapEditorSettings.FilterType filterTypeIndirect;
        public LightmapEditorSettings.FilterType filterTypeAO;
        public int filteringGaussRadiusDirect;
        public int filteringGaussRadiusIndirect;
        public int filteringGaussRadiusAO;
        public float filteringAtrousPositionSigmaDirect;
        public float filteringAtrousPositionSigmaIndirect;
        public float filteringAtrousPositionSigmaAO;
        public int environmentSampleCount;
        public float lightProbeSampleCountMultiplier;
        public int maxAtlasSize;
        public float realtimeResolution;
        public float bakeResolution;
        public bool textureCompression;
        public UnityEngine.Rendering.ReflectionCubemapCompression reflectionCubemapCompression;
        public bool enableAmbientOcclusion;
        public float aoMaxDistance;
        public float aoExponentIndirect;
        public float aoExponentDirect;
        public int padding;
        public bool exportTrainingData;
        public string trainingDataDestination;

        public void ApplyDatas()
        {
            Type lightmapEditorSettingsType = typeof(LightmapEditorSettings);
            FieldInfo[] fieldInfos = typeof(LightmappingSettingDataStorage).GetFields(BindingFlags.Instance | BindingFlags.Public);
            foreach (FieldInfo fieldInfo in fieldInfos)
            {
                lightmapEditorSettingsType.GetProperty(fieldInfo.Name, BindingFlags.Static | BindingFlags.Public).SetValue(null, fieldInfo.GetValue(this));
            }
        }
    }

    //BakeLightingSettingDataStorage
}


