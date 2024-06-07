using System.Collections.Generic;
using MadPixel;
using MAXHelper;
using UnityEngine;
using UnityEngine.Purchasing;
using Io.AppMetrica;
using Io.AppMetrica.Profile;
using UnityEngine.Purchasing.MiniJSON;
using Attribute = Io.AppMetrica.Profile.Attribute;

namespace MadPixelAnalytics {
    public class AppMetricaComp : MonoBehaviour {
        #region Fields
        [SerializeField] private bool bSendAdRevenue = false;
        [SerializeField] private bool bSendLatencyEvents = false;
        [SerializeField] private bool bLogEventsOnDevice = false;
#if UNITY_EDITOR
        [SerializeField] private bool bLogEventsInEditor = true;
#endif 
        #endregion


        private void OnDestroy() {
#if USE_MAX_DEF
            if (bSendAdRevenue) {
                MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent -= OnAdRevenuePaidEvent;
                MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent -= OnAdRevenuePaidEvent;
                MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent -= OnAdRevenuePaidEvent;
            }
            if (bSendLatencyEvents) {
                MaxSdkCallbacks.Interstitial.OnAdLoadedEvent -= OnInterLoadedEvent;
                MaxSdkCallbacks.Rewarded.OnAdLoadedEvent -= OnRewardedLoadedEvent;
                MaxSdkCallbacks.Banner.OnAdLoadedEvent -= OnBannerLoadedEvent;

                MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent -= OnInterLoadFailedEvent;
                MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent -= OnRewardedLoadFailedEvent;
                MaxSdkCallbacks.Banner.OnAdLoadFailedEvent -= OnBannerLoadFailedEvent;
            }
#endif
        }


        public void Init() {
#if USE_MAX_DEF
            if (bSendAdRevenue) {
                MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += OnAdRevenuePaidEvent;
                MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += OnAdRevenuePaidEvent;
                MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += OnAdRevenuePaidEvent;
            }

            if (bSendLatencyEvents) {
                MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += OnInterLoadedEvent;
                MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += OnRewardedLoadedEvent;
                MaxSdkCallbacks.Banner.OnAdLoadedEvent += OnBannerLoadedEvent;

                MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += OnInterLoadFailedEvent;
                MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += OnRewardedLoadFailedEvent;
                MaxSdkCallbacks.Banner.OnAdLoadFailedEvent += OnBannerLoadFailedEvent;
            }
#endif
        }

#if USE_MAX_DEF

        #region Ads Related
        #region Banner Load/Fail
        private void OnBannerLoadFailedEvent(string adUnitID, MaxSdkBase.ErrorInfo errorInfo) {
            SendCustomEvent("on_ad_load_fail", new Dictionary<string, object>() {
                {"latency", GetLatencyRange(errorInfo.WaterfallInfo.LatencyMillis)},
                {"waterfall_name", errorInfo.WaterfallInfo.TestName},
                {"ad_type", "banner"},
            });
        }
        private void OnBannerLoadedEvent(string adUnitID, MaxSdkBase.AdInfo adInfo) {
            SendCustomEvent("on_ad_loaded", new Dictionary<string, object>() {
                {"latency", GetLatencyRange(adInfo.WaterfallInfo.LatencyMillis)},
                {"waterfall_name", adInfo.WaterfallInfo.TestName},
                {"ad_type", "banner"},
            });
        } 
        #endregion



        #region Rewarded Load/Fail
        private void OnRewardedLoadedEvent(string adUnitID, MaxSdkBase.AdInfo adInfo) {
            SendCustomEvent("on_ad_loaded", new Dictionary<string, object>() {
                {"latency", GetLatencyRange(adInfo.WaterfallInfo.LatencyMillis)},
                {"waterfall_name", adInfo.WaterfallInfo.TestName},
                {"ad_type", "rewarded"},
            });
        }
        private void OnRewardedLoadFailedEvent(string adUnitID, MaxSdkBase.ErrorInfo errorInfo) {
            SendCustomEvent("on_ad_load_fail", new Dictionary<string, object>() {
                {"latency", GetLatencyRange(errorInfo.WaterfallInfo.LatencyMillis)},
                {"waterfall_name", errorInfo.WaterfallInfo.TestName},
                {"ad_type", "rewarded"},
            });
        } 
        #endregion



        #region Inter Load/Fail
        private void OnInterLoadedEvent(string adUnitID, MaxSdkBase.AdInfo adInfo) {
            SendCustomEvent("on_ad_loaded", new Dictionary<string, object>() {
                {"latency", GetLatencyRange(adInfo.WaterfallInfo.LatencyMillis)},
                {"waterfall_name", adInfo.WaterfallInfo.TestName},
                {"ad_type", "interstitial"},
            });
        }
        private void OnInterLoadFailedEvent(string adUnitID, MaxSdkBase.ErrorInfo errorInfo) {
            SendCustomEvent("on_ad_load_fail", new Dictionary<string, object>() {
                {"latency", GetLatencyRange(errorInfo.WaterfallInfo.LatencyMillis)},
                {"waterfall_name", errorInfo.WaterfallInfo.TestName},
                {"ad_type", "interstitial"},
            });
        } 
        #endregion


        public void OnAdRevenuePaidEvent(string a_adUnit, MaxSdk.AdInfo a_adInfo) {
            if (a_adInfo.Revenue > 0) {
                AdRevenue adRevenue = new AdRevenue(a_adInfo.Revenue, "USD");
                AppMetrica.ReportAdRevenue(adRevenue);
            }
        }

        public void VideoAdWatched(AdInfo AdInfo) {
            SendCustomEvent("video_ads_watch", GetAdAttributes(AdInfo));
        }

        public void VideoAdAvailable(AdInfo AdInfo) {
            SendCustomEvent("video_ads_available", GetAdAttributes(AdInfo));
        }

        public void VideoAdStarted(AdInfo AdInfo) {
            SendCustomEvent("video_ads_started", GetAdAttributes(AdInfo));
        }


        public void VideoAdError(MaxSdkBase.AdInfo adInfo, MaxSdkBase.ErrorInfo EInfo, string placement) {
            Dictionary<string, object> eventAttributes = new Dictionary<string, object>();

            string NetworkName = "unknown";
            if (adInfo != null && !string.IsNullOrEmpty(adInfo.NetworkName)) {
                NetworkName = adInfo.NetworkName;
            }

            string AdLoadFailureInfo = "NULL";
            string Message = "NULL";
            string Code = "NULL";
            if (EInfo != null) {
                if (!string.IsNullOrEmpty(EInfo.Message)) {
                    Message = EInfo.Message;
                }
                if (!string.IsNullOrEmpty(EInfo.AdLoadFailureInfo)) {
                    AdLoadFailureInfo = EInfo.AdLoadFailureInfo;
                }

                Code = EInfo.Code.ToString();
            }

            eventAttributes.Add("network", NetworkName);
            eventAttributes.Add("error_message", Message);
            eventAttributes.Add("error_code", Code);
            eventAttributes.Add("ad_load_failure_info", AdLoadFailureInfo);
            eventAttributes.Add("placement", placement);
            SendCustomEvent("ad_display_error", eventAttributes);
        }

        #endregion

#endif
        public void RateUs(int rateResult) {
            Dictionary<string, object> eventAttributes = new Dictionary<string, object>();
            eventAttributes.Add("rate_result", rateResult);
            SendCustomEvent("rate_us", eventAttributes);
        }

        public void ABTestInitMetricaAttributes(string Value) {
            UserProfile profile = new UserProfile();
            //List<UserProfileUpdate> profileUpdates = new List<UserProfileUpdate>() {
            //    new UserProfileUpdate()
            //};

            //profileUpdates.Add(new UserProfileUpdate("ab_test_group").WithValue(Value));
            profile.Apply(Attribute.CustomString("ab_test_group").WithValue(Value));
            AppMetrica.ReportUserProfile(profile);
            AppMetrica.SendEventsBuffer();
        }



        #region Purchases
        public void PurchaseSucceed(MPReceipt Receipt) {
            Dictionary<string, object> eventAttributes = new Dictionary<string, object>();
            eventAttributes.Add("inapp_id", Receipt.Product.definition.storeSpecificId);
            eventAttributes.Add("currency", Receipt.Product.metadata.isoCurrencyCode);
            eventAttributes.Add("price", (float)Receipt.Product.metadata.localizedPrice);
            SendCustomEvent("payment_succeed", eventAttributes);

            HandlePurchase(Receipt.Product, Receipt.Data, Receipt.Signature);
        }

        public void HandlePurchase(Product Product, string data, string signature) {
            Revenue Revenue = new Revenue((long)Product.metadata.localizedPrice, Product.metadata.isoCurrencyCode);
            Revenue.Quantity = 1;
            Revenue.ProductID = Product.definition.storeSpecificId;

            Revenue.Receipt Receipt = new Revenue.Receipt();
            Receipt.Signature = signature;
            Receipt.Data = data;

            Revenue.ReceiptValue = Receipt;

#if UNITY_EDITOR
            return;
#else
            AppMetrica.ReportRevenue(Revenue);
#endif
        }
        #endregion


        #region Helpers

        public void SendCustomEvent(string eventName, Dictionary<string, object> parameters, bool bSendEventsBuffer = false) {
            if (parameters == null) {
                parameters = new Dictionary<string, object>();
            }

            bool debugLog = bLogEventsOnDevice;

#if UNITY_EDITOR
            debugLog = bLogEventsInEditor;
#else
            AppMetrica.ReportEvent(eventName, parameters.toJson());

            if (bSendEventsBuffer) {
                AppMetrica.SendEventsBuffer();
            }
#endif
            if (debugLog) {
                string eventParams = "";
                foreach (string key in parameters.Keys) {
                    eventParams = eventParams + "\n" + key + ": " + parameters[key].ToString();
                }

                Debug.Log($"Event: {eventName} and params: {eventParams}");
            }
        }


        private Dictionary<string, object> GetAdAttributes(AdInfo Info) {
            Dictionary<string, object> eventAttributes = new Dictionary<string, object>();
            string adType = "interstitial";
            if (Info.AdType == AdsManager.EAdType.REWARDED) {
                adType = "rewarded";
            } else if (Info.AdType == AdsManager.EAdType.BANNER) {
                adType = "banner";
            }
            eventAttributes.Add("ad_type",  adType);
            eventAttributes.Add("placement", Info.Placement);
            eventAttributes.Add("connection", Info.HasInternet ? 1 : 0);
            eventAttributes.Add("result", Info.Availability);

            return eventAttributes;
        }

        private string GetLatencyRange(long latencyMillis) {
            if (latencyMillis < 1000) { return "<1"; }
            if (latencyMillis < 6000) { return "1-5"; }
            if (latencyMillis < 10000) { return "5-10"; }
            if (latencyMillis < 15000) { return "10-15"; }
            if (latencyMillis < 20000) { return "15-20"; }
            if (latencyMillis < 25000) { return "20-25"; }
            if (latencyMillis < 30000) { return "25-30"; }
            if (latencyMillis < 40000) { return "30-40"; }
            if (latencyMillis < 50000) { return "40-50"; }
            if (latencyMillis < 60000) { return "50-60"; }
            if (latencyMillis < 90000) { return "60-90"; }
            if (latencyMillis < 120000) { return "90-120"; }
            if (latencyMillis < 180000) { return "120-180"; }

            return ">180";
        }

        #endregion

    }

    public static class AppMetricaActivator {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Activate() {
            AppMetrica.Activate(new AppMetricaConfig("APIKey") {
                // copy settings from prefab
                CrashReporting = true, // prefab field 'Exceptions Reporting'
                SessionTimeout = 10, // prefab field 'Session Timeout Sec'
                LocationTracking = false, // prefab field 'Location Tracking'
                Logs = false, // prefab field 'Logs'
                FirstActivationAsUpdate = !IsFirstLaunch(), // prefab field 'Handle First Activation As Update'
                DataSendingEnabled = true, // prefab field 'Statistics Sending'
            });
        }

        private static bool IsFirstLaunch() {
            // Implement logic to detect whether the app is opening for the first time.
            // For example, you can check for files (settings, databases, and so on),
            // which the app creates on its first launch.
            return true;
        }
    }
}
