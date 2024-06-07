using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using System.IO;
using MAXHelper;

namespace MadPixel {
    public class MPCSetupWindow : EditorWindow {
        public static string CONFIGS_PATH = "Assets/MadPixel/MAXCustomSettings.asset";

        #region Fields
        public static string CONFIGS_FOLDER = "Assets/MadPixel";
        private const string PACKAGE_PATH = "Assets/MadPixel/MAXHelper/Configs/MaximumPack.unitypackage";
        private const string MEDIATIONS_PATH = "Assets/MAXSdk/Mediation/";
        private const string MAX_INSTALL_PACKAGE = "Packages/MadPixelCore/Editor/UnityPackages/AppLovin-MAX-Unity-Plugin.unitypackage";

        private const string MPC_FOLDER = "https://github.com/MadPixelDevelopment/MadPixelCore/releases";
        private const string MAX_PACK_INDEPENDENT = "https://github.com/MadPixelDevelopment/MadPixelCore/raw/main/Assets/MadPixel/MAXHelper/Configs/MaximumPack.unitypackage";
        private const string ADS_DOC = "https://docs.google.com/document/d/1lx9wWCD4s8v4aXH1pb0oQENz01UszdalHtnznmQv2vc/edit#heading=h.y039lv8byi2i";

        private List<string> MAX_VARIANT_PACKAGES = new List<string>() { "ByteDance", "Fyber", "Google", "InMobi", "Mintegral", "MyTarget", "Vungle"};

        private Vector2 scrollPosition;
        private static readonly Vector2 windowMinSize = new Vector2(450, 250);
        private static readonly Vector2 windowPrefSize = new Vector2(850, 400);

        private GUIStyle titleLabelStyle;
        private GUIStyle warningLabelStyle; 
        private GUIStyle linkLabelStyle;
        private GUIStyle versionsLabelStyle;

        private static GUILayoutOption sdkKeyLabelFieldWidthOption = GUILayout.Width(120);
        private static GUILayoutOption sdkKeyTextFieldWidthOption = GUILayout.Width(650);
        private static GUILayoutOption buttonFieldWidth = GUILayout.Width(160);
        private static GUILayoutOption adUnitLabelWidthOption = GUILayout.Width(140);
        private static GUILayoutOption adUnitTextWidthOption = GUILayout.Width(150);
        private static GUILayoutOption adMobLabelFieldWidthOption = GUILayout.Width(100);
        private static GUILayoutOption adMobUnitTextWidthOption = GUILayout.Width(280);
        private static GUILayoutOption adUnitToggleOption = GUILayout.Width(180);
        private static GUILayoutOption bannerColorLabelOption = GUILayout.Width(250);

        private MAXCustomSettings CustomSettings;
        private bool bMaxVariantInstalled;
        #endregion


        #region Menu Item
        [MenuItem("Mad Pixel Core/Setup", priority = 0)]
        public static void ShowWindow() {
            var Window = EditorWindow.GetWindow<MPCSetupWindow>("MadPixelCore", true);

            Window.Setup();
        }

        private void Setup() {
            minSize = windowMinSize;
            LoadConfigFromFile();
            AddImportCallbacks();
            CheckMaxVersion();

        }
        #endregion



        #region Editor Window Lifecyle Methods

        private void OnGUI() { 
            if (CustomSettings != null) {
                using (var scrollView = new EditorGUILayout.ScrollViewScope(scrollPosition, false, false)) {
                    scrollPosition = scrollView.scrollPosition;

                    GUILayout.Space(5);

                    titleLabelStyle = new GUIStyle(EditorStyles.label) {
                        fontSize = 14,
                        fontStyle = FontStyle.Bold,
                        fixedHeight = 20
                    };

                    versionsLabelStyle = new GUIStyle(EditorStyles.label) {
                        fontSize = 12,
                    };
                    ColorUtility.TryParseHtmlString("#C4ECFD", out Color vColor);
                    versionsLabelStyle.normal.textColor = vColor;


                    if (linkLabelStyle == null) {
                        linkLabelStyle = new GUIStyle(EditorStyles.label) {
                            fontSize = 12,
                            wordWrap = false,
                        };
                    }
                    ColorUtility.TryParseHtmlString("#7FD6FD", out Color C);
                    linkLabelStyle.normal.textColor = C;
#if USE_MAX_DEF
                    // Draw AppLovin MAX plugin details
                    EditorGUILayout.LabelField("1. Fill in your SDK Key", titleLabelStyle);

                    DrawSDKKeyPart();

                    DrawUnitIDsPart();

                    DrawTestPart();

                    DrawInstallButtons();
#else
                    DrawInstallMAX();
#endif
                    DrawLinks();
                }
            }


            if (GUI.changed) {
#if USE_MAX_DEF
                AppLovinSettings.Instance.SaveAsync();
#endif
                EditorUtility.SetDirty(CustomSettings);
            }
        }

        private void OnDisable() {
#if USE_MAX_DEF
            if (CustomSettings != null) {
                AppLovinSettings.Instance.SdkKey = CustomSettings.SDKKey;
            }
#endif

            AssetDatabase.SaveAssets();
        }


#endregion

        #region Draw Functions

        private void DrawInstallMAX() {
            GUI.enabled = true; 
            EditorGUILayout.LabelField("You dont use Applovin MAX SDK. Do you want to install it?", titleLabelStyle);
            GUILayout.BeginHorizontal();
            GUILayout.Space(4);
            if (GUILayout.Button(new GUIContent("Install MAX SDK"), buttonFieldWidth)) {
                OnInstallMAXClick();
            }
            GUILayout.EndHorizontal();
        }

#if USE_MAX_DEF
        private void DrawSDKKeyPart() {
            GUI.enabled = true;
            CustomSettings.SDKKey = DrawTextField("AppLovin SDK Key", CustomSettings.SDKKey, sdkKeyLabelFieldWidthOption, sdkKeyTextFieldWidthOption);

            using (new EditorGUILayout.VerticalScope("box")) {
                GUILayout.Space(4);
                GUILayout.BeginHorizontal();
                GUILayout.Space(4);
                AppLovinSettings.Instance.QualityServiceEnabled = GUILayout.Toggle(AppLovinSettings.Instance.QualityServiceEnabled, "  Enable MAX Ad Review (turn this on for production build)");
                GUILayout.EndHorizontal();
                GUILayout.Space(4);
            }
        }

        private void DrawUnitIDsPart() {
            GUILayout.Space(16);
            EditorGUILayout.LabelField("2. Fill in your Ad Unit IDs (from MadPixel managers)", titleLabelStyle);
            using (new EditorGUILayout.VerticalScope("box")) {
                if (CustomSettings == null) {
                    LoadConfigFromFile();
                }

                GUI.enabled = true;
                GUILayout.Space(4);
                GUILayout.BeginHorizontal();
                GUILayout.Space(4);
                CustomSettings.bUseRewardeds = GUILayout.Toggle(CustomSettings.bUseRewardeds, "Use Rewarded Ads", adUnitToggleOption);
                GUI.enabled = CustomSettings.bUseRewardeds;
                CustomSettings.RewardedID = DrawTextField("Rewarded Ad Unit (Android)", CustomSettings.RewardedID, adUnitLabelWidthOption, adUnitTextWidthOption);
                CustomSettings.RewardedID_IOS = DrawTextField("Rewarded Ad Unit (IOS)", CustomSettings.RewardedID_IOS, adUnitLabelWidthOption, adUnitTextWidthOption);
                GUILayout.EndHorizontal();
                GUILayout.Space(4);

                GUI.enabled = true;
                GUILayout.Space(4);
                GUILayout.BeginHorizontal();
                GUILayout.Space(4);
                CustomSettings.bUseInters = GUILayout.Toggle(CustomSettings.bUseInters, "Use Interstitials", adUnitToggleOption);
                GUI.enabled = CustomSettings.bUseInters;
                CustomSettings.InterstitialID = DrawTextField("Inerstitial Ad Unit (Android)", CustomSettings.InterstitialID, adUnitLabelWidthOption, adUnitTextWidthOption);
                CustomSettings.InterstitialID_IOS = DrawTextField("Interstitial Ad Unit (IOS)", CustomSettings.InterstitialID_IOS, adUnitLabelWidthOption, adUnitTextWidthOption);
                GUILayout.EndHorizontal();
                GUILayout.Space(4);

                GUI.enabled = true;
                GUILayout.Space(4);
                GUILayout.BeginHorizontal();
                GUILayout.Space(4);
                CustomSettings.bUseBanners = GUILayout.Toggle(CustomSettings.bUseBanners, "Use Banners", adUnitToggleOption);
                GUI.enabled = CustomSettings.bUseBanners;
                CustomSettings.BannerID = DrawTextField("Banner Ad Unit (Android)", CustomSettings.BannerID, adUnitLabelWidthOption, adUnitTextWidthOption);
                CustomSettings.BannerID_IOS = DrawTextField("Banner Ad Unit (IOS)", CustomSettings.BannerID_IOS, adUnitLabelWidthOption, adUnitTextWidthOption);
                GUILayout.EndHorizontal();
                GUILayout.Space(4);

                GUI.enabled = true;
                GUILayout.Space(4);
                GUILayout.BeginHorizontal();
                if (CustomSettings.bUseBanners) {
                    GUILayout.Space(24);

                    CustomSettings.BannerBackground = EditorGUILayout.ColorField("Banner Background Color: ", CustomSettings.BannerBackground, bannerColorLabelOption);

                    GUILayout.Space(4);

                }

                GUILayout.EndHorizontal();

                GUI.enabled = true;
            }
        }

        private void DrawTestPart() {
            GUILayout.Space(16);
            EditorGUILayout.LabelField("3. For testing mediations: enable Mediation Debugger", titleLabelStyle);

            using (new EditorGUILayout.VerticalScope("box")) {
                GUILayout.Space(4);
                GUILayout.BeginHorizontal();
                GUILayout.Space(4);

                if (warningLabelStyle == null) {
                    warningLabelStyle = new GUIStyle(EditorStyles.label) {
                        fontSize = 13,
                        fontStyle = FontStyle.Bold,
                        fixedHeight = 20
                    };
                }

                ColorUtility.TryParseHtmlString("#D22F2F", out Color C);
                warningLabelStyle.normal.textColor = C;

                if (CustomSettings.bShowMediationDebugger) {
                    EditorGUILayout.LabelField("For Test builds only. Do NOT enable this option in the production build!", warningLabelStyle);
                }

                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Space(4);
                CustomSettings.bShowMediationDebugger = GUILayout.Toggle(CustomSettings.bShowMediationDebugger, "Show Mediation Debugger", adUnitToggleOption);
                GUILayout.EndHorizontal();
            }
        }
        

        private void DrawInstallButtons() {
            GUILayout.Space(16);
            EditorGUILayout.LabelField("4. Install our full mediations", titleLabelStyle);
            using (new EditorGUILayout.VerticalScope("box")) {
                GUILayout.BeginHorizontal();
                GUILayout.Space(10);

                if (!MackPackUnitypackageExists()) {
                    EditorGUILayout.LabelField("You dont have MaximunPack.unitypackage in your project. Probably your git added it to gitignore", sdkKeyTextFieldWidthOption);
                    
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(10);

                    if (GUILayout.Button(new GUIContent("Download latest Maximum mediations package"), adMobUnitTextWidthOption)) {
                        Application.OpenURL(MAX_PACK_INDEPENDENT);
                    }

                    GUILayout.EndHorizontal();
                    GUILayout.Space(10);
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(10);
                }

                GUI.enabled = MackPackUnitypackageExists();
                if (bMaxVariantInstalled) {
                    EditorGUILayout.LabelField("You have installed default Maximum pack of mediations", sdkKeyTextFieldWidthOption);
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(10);
                }
                if (GUILayout.Button(new GUIContent(bMaxVariantInstalled ? "Reimport maximum pack" : "Install maximum pack"), buttonFieldWidth)) {
                    AssetDatabase.ImportPackage(PACKAGE_PATH, true);
                    CheckMaxVersion();
                }

                GUI.enabled = true;
                GUILayout.EndHorizontal();

                //if (bMaxVariantInstalled) {

                    GUILayout.Space(10);
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(10);

                    AppLovinSettings.Instance.AdMobAndroidAppId = DrawTextField("AndroidAdMobID",
                        AppLovinSettings.Instance.AdMobAndroidAppId, adMobLabelFieldWidthOption, adMobUnitTextWidthOption);
                    AppLovinSettings.Instance.AdMobIosAppId = DrawTextField("IOSAdMobID",
                        AppLovinSettings.Instance.AdMobIosAppId, adMobLabelFieldWidthOption, adMobUnitTextWidthOption);

                    GUILayout.Space(5);
                    GUILayout.EndHorizontal();
                //}
            }
        }
#endif

        private void DrawLinks() {
            GUILayout.Space(15);

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Read MPC Documentation", GUILayout.Width(150));
            if (GUILayout.Button(new GUIContent("here"), GUILayout.Width(50))) {
                Application.OpenURL(ADS_DOC);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Download latest MadPixelCore plugin", GUILayout.Width(215));
            if (GUILayout.Button(new GUIContent("from here"), GUILayout.Width(70))) {
                Application.OpenURL(MPC_FOLDER);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Ads Manager v." + AdsManager.Version, versionsLabelStyle, sdkKeyLabelFieldWidthOption);
            GUILayout.EndHorizontal();



            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("MPC v." + MPCChecker.GetVersion(), versionsLabelStyle, sdkKeyLabelFieldWidthOption);
            GUILayout.EndHorizontal();
        }

        private string DrawTextField(string fieldTitle, string text, GUILayoutOption labelWidth, GUILayoutOption textFieldWidthOption = null) {
            GUILayout.BeginHorizontal();
            GUILayout.Space(4);
            EditorGUILayout.LabelField(new GUIContent(fieldTitle), labelWidth);
            GUILayout.Space(4);
            text = (textFieldWidthOption == null) ? GUILayout.TextField(text) : GUILayout.TextField(text, textFieldWidthOption);
            GUILayout.Space(4);
            GUILayout.EndHorizontal();
            GUILayout.Space(4);

            return text;
        }

#endregion

        #region Helpers
        private void LoadConfigFromFile() {
            var Obj = AssetDatabase.LoadAssetAtPath(CONFIGS_PATH, typeof(MAXCustomSettings));
            if (Obj != null) {
                CustomSettings = (MAXCustomSettings)Obj;
            } else {
                Debug.Log("CustomSettings file doesn't exist, creating a new one...");
                if (!Directory.Exists(CONFIGS_FOLDER)) {
                    Directory.CreateDirectory(CONFIGS_FOLDER);
                }
                var Instance = MAXCustomSettings.CreateInstance("MAXCustomSettings");
                AssetDatabase.CreateAsset(Instance, CONFIGS_PATH);
                EditorUtility.SetDirty(Instance);
                AssetDatabase.SaveAssetIfDirty(Instance);
                CustomSettings = (MAXCustomSettings)Instance;
            }
        }

        private void CheckMaxVersion() {
            string[] filesPaths = System.IO.Directory.GetFiles(MEDIATIONS_PATH);
            if (filesPaths != null && filesPaths.Length > 0) {
                List<string> Paths = filesPaths.ToList();
                bool bMissingPackage = false;
                foreach (string PackageName in MAX_VARIANT_PACKAGES) {
                    if (!filesPaths.Contains(MEDIATIONS_PATH + PackageName + ".meta")) {
                        bMissingPackage = true;
                        break;
                    }
                }

                bMaxVariantInstalled = !bMissingPackage;
            }
        }

        private void AddImportCallbacks() {
            AssetDatabase.importPackageCompleted += packageName => {
                Debug.Log($"Package {packageName} installed");
                CheckMaxVersion();
            };

            AssetDatabase.importPackageCancelled += packageName => {
                Debug.Log($"Package {packageName} cancelled");
            };

            AssetDatabase.importPackageFailed += (packageName, errorMessage) => {
                Debug.Log($"Package {packageName} failed");
            };
        }

        private void OnInstallMAXClick() {
            if (File.Exists(MAX_INSTALL_PACKAGE)) {
                AssetDatabase.ImportPackage(MAX_INSTALL_PACKAGE, true);
            }
            else {
                Application.OpenURL("https://github.com/AppLovin/AppLovin-MAX-Unity-Plugin/releases/download/release_6_5_2/AppLovin-MAX-Unity-Plugin-6.5.2-Android-12.5.0-iOS-12.5.0.unitypackage");
            }
        }

        private bool MackPackUnitypackageExists() {
            return File.Exists(PACKAGE_PATH);
        }

        #endregion
    }
}
