﻿using System;
using System.Reflection;

#if UNITY_2021_3_OR_NEWER
using UnityEditor.SceneManagement;
#elif UNITY_2018_3_OR_NEWER
using UnityEditor.Experimental.SceneManagement;
#endif

public class PrefabSavingUtil {

    public static void SavePrefab(PrefabStage prefabStage)
    {
        if (prefabStage == null)
            throw new ArgumentNullException();

        var savePrefabMethod = prefabStage.GetType().GetMethod("SavePrefab", BindingFlags.NonPublic | BindingFlags.Instance);
        if (savePrefabMethod == null)
            throw new InvalidOperationException();

        savePrefabMethod.Invoke(prefabStage, null);
    }
}
