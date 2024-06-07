using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MAXHelper {
    [CreateAssetMenu(fileName = "MAXCustomSettings", menuName = "MAXHelper/Configs/MAXCustomSettings", order = 1)]
    public class MAXCustomSettings : ScriptableObject {
        public bool bUseRewardeds;
        public bool bUseInters;
        public bool bUseBanners;
        public bool bShowMediationDebugger;

        public string SDKKey;

        public string BannerID;
        public string InterstitialID;
        public string RewardedID;

        public string BannerID_IOS;
        public string InterstitialID_IOS;
        public string RewardedID_IOS;

        public Color BannerBackground;

        public string appMetricaKey;
        public string appsFlyerKey;
    }

    public static class DeafultKeys {
        public const string APPLOVIN_SDK_KEY = "R5ZeDg0t8rV5BQ4h_72SUwzDKUOipd1Ju_H3yph9eKZV6NZBDqI_rLKZmyFWiyFWdOn4ITSHwMdob2TtWHuzio";
        public const string APPSFLYER_SDK_KEY = "bAfXoQibEMwiDKEGT6UHTG";
    }
}
