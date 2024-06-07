using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;

#if UNITY_EDITOR
namespace MAXHelper {
    public static class MAXHelperDefineSymbols {
        public static readonly string USE_MAX_DEF = "MADPIXEL_USE_MAX";

        public static void DefineSymbols(bool bActive = true) {
            string alreadyDefined = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.Android);
            List<string> scriptingDefinesStringList = alreadyDefined.Split(';').ToList();
            if (bActive) {
                if (scriptingDefinesStringList.Contains(USE_MAX_DEF)) {
                    return;
                }
                scriptingDefinesStringList.Add(USE_MAX_DEF);
            }
            else {
                if (scriptingDefinesStringList.Contains(USE_MAX_DEF)) {
                    scriptingDefinesStringList.Remove(USE_MAX_DEF);
                }
                else {
                    return;
                }
            }


            PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.Android, string.Join(";", scriptingDefinesStringList.ToArray()));
        }

        public static bool HasMAXActivated() {
            string alreadyDefined = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.Android);
            List<string> scriptingDefinesStringList = alreadyDefined.Split(';').ToList();
            return scriptingDefinesStringList.Contains(USE_MAX_DEF);
        }
    } 
}
#endif