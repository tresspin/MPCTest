using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using MadPixel;
using MAXHelper;

[InitializeOnLoad]
public class MPCChecker {
    private static readonly List<string> ObsoleteDirectoriesToDelete = new List<string> {
        "Assets/MadPixel",
        "Assets/AppMetrica",
        "Assets/AppsFlyer",
        "Assets/ExternalDependencyManager",
    };

    static MPCChecker() {
        CheckObsoleteFiles();
        CheckApplovinInProject();

#if UNITY_ANDROID
        int target = (int)PlayerSettings.Android.targetSdkVersion;
        if (target == 0) {
            int highestInstalledVersion = GetHigestInstalledSDK();
            target = highestInstalledVersion;
        }

        if (target < 33 || PlayerSettings.Android.minSdkVersion < AndroidSdkVersions.AndroidApiLevel24) {
            if (EditorPrefs.HasKey(Key)) {
                string lastMPCVersionChecked = EditorPrefs.GetString(Key);
                string currVersion = GetVersion();
                if (lastMPCVersionChecked != currVersion) {
                    ShowSwitchTargetWindow(target);
                }
            }
            else {
                ShowSwitchTargetWindow(target);
            }
        }
        SaveKey();
#endif
    }


#if UNITY_ANDROID
    private static string appKey = null;
    private static string Key {
        get {
            if (string.IsNullOrEmpty(appKey)) {
                appKey = GetMd5Hash(Application.dataPath) + "MPCv";
            }

            return appKey;
        }
    }

    private static void ShowSwitchTargetWindow(int target) {
        MPCTargetCheckerWindow.ShowWindow(target, (int)PlayerSettings.Android.targetSdkVersion);

        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;
        PlayerSettings.Android.targetSdkVersion = (AndroidSdkVersions)33;
    }

    private static void ShowDirectoriesDeletedWindow() {
        MPCTargetCheckerWindow.ShowDirectoriesDeletedWindow();
    }


    private static string GetMd5Hash(string input) {
        MD5 md5 = MD5.Create();
        byte[] data = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < data.Length; i++) {
            sb.Append(data[i].ToString("x2"));
        }

        return sb.ToString();
    }

    #region Save/Delete keys
    public static void SaveKey() {
        EditorPrefs.SetString(Key, GetVersion());
    }

    //[MenuItem("Mad Pixel/DeleteKey", priority = 1)]
    public static void DeleteEditorPrefs() {
        EditorPrefs.DeleteKey(Key);
    }
    #endregion

    #region Check highest and min droid SDK version
    private static int GetHigestInstalledSDK() {
        string s = Path.Combine(GetHighestInstalledAPI(), "platforms");
        string[] directories = Directory.GetDirectories(s);
        int maxV = 0;
        foreach (string directory in directories) {
            string version = directory.Substring(directory.Length - 2, 2);
            int.TryParse(version, out int v);
            if (v > 0) {
                maxV = Mathf.Max(v, maxV);
            }
        }
        return maxV;
    }

    private static string GetHighestInstalledAPI() {
        return EditorPrefs.GetString("AndroidSdkRoot");
    }
    #endregion
#endif


    #region Check obsolete AM, AF, EDM4U folders
    private static void CheckObsoleteFiles() {
        var oldSettings = AssetDatabase.LoadAssetAtPath<MAXCustomSettings>("Assets/MadPixel/MAXHelper/Configs/MAXCustomSettings.asset");
        if (oldSettings != null) {
            AssetDatabase.CreateAsset(oldSettings, MPCSetupWindow.CONFIGS_PATH);
        }

        List<string> directoriesToDelete = new List<string>();
        foreach (string directory in ObsoleteDirectoriesToDelete) {
            if (CheckExistence(directory)) {
                directoriesToDelete.Add(directory);
            }
        }



        if (directoriesToDelete.Count > 0) {
            foreach (string directory in directoriesToDelete) {
                FileUtil.DeleteFileOrDirectory(directory);
            }
            AssetDatabase.Refresh();
            Debug.LogWarning("ATTENTION: Cleanup complete!");
        }
    } 

    private static bool CheckExistence(string location) {
        return File.Exists(location) ||
               Directory.Exists(location) ||
               (location.EndsWith("/*") && Directory.Exists(Path.GetDirectoryName(location)));
    }
    #endregion



    #region Check MAX SDK in project
    private static void CheckApplovinInProject() {
        if (HasMAXsdkInProject()) {
            MAXHelperDefineSymbols.DefineSymbols(true);
        }
        else {
            MAXHelperDefineSymbols.DefineSymbols(false);

        }
    }

    public static bool HasMAXsdkInProject() {
        return Directory.Exists("Assets/MaxSdk") &&
               Directory.Exists("Assets/MaxSdk/Sripts") &&
               File.Exists("Assets/MaxSdk/Sripts/MaxSDK.cs");
    } 
    #endregion



    public static string GetVersion() {
        var assembly = typeof(MPCChecker).Assembly;

        // See https://docs.unity3d.com/ScriptReference/PackageManager.PackageInfo.FindForAssembly.html
        var packageInfo = UnityEditor.PackageManager.PackageInfo.FindForAssembly(assembly);

        // Finally we have access to the version!
        var version = packageInfo.version;
        return version;
    }
}
