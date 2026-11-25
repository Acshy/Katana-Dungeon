using System;
using TapSDK.Core;
using UnityEngine;

namespace KatanaDungeon.ThirdParty.TapTap
{
    /// <summary>
    /// Initializes TapTap Core for compliance without enabling TapDB events.
    /// </summary>
    public sealed class TapTapSdkCoreBridge : MonoBehaviour
    {
        private static TapTapSdkCoreBridge instance;
        private static bool initializing;
        private static bool initialized;

        [Header("TapTap application")]
        [SerializeField] private string clientId;
        [SerializeField] private string clientToken;
        [SerializeField] private TapTapRegionType region = TapTapRegionType.CN;
        [SerializeField] private bool enableSdkLog;

        public static bool IsInitialized => initialized;
        public static bool IsInitializing => initializing;
        public static string InitializationError { get; private set; }
        public static event Action<bool, string> InitializationCompleted;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void InitializeAfterPrivacyConsent()
        {
            if (initialized || initializing)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientToken))
            {
                CompleteInitialization(false, "TapTap Client ID or Client Token is empty.");
                return;
            }

            initializing = true;
            InitializationError = null;

            var coreOptions = new TapTapSdkOptions
            {
                clientId = clientId.Trim(),
                clientToken = clientToken.Trim(),
                region = region,
                enableLog = enableSdkLog,
            };

            TapTapSDK.AddInitCallback(new InitCallback(this));
            TapTapSDK.Init(coreOptions, new TapTapSdkBaseOptions[0]);
        }

        private void CompleteInitialization(bool success, string error)
        {
            initializing = false;
            initialized = success;
            InitializationError = success ? null : error;
            InitializationCompleted?.Invoke(success, error);

            if (!success)
            {
                Debug.LogError($"TapTap Core initialization failed: {error}");
            }
        }

        private sealed class InitCallback : ITapInitCallback
        {
            private readonly TapTapSdkCoreBridge owner;

            public InitCallback(TapTapSdkCoreBridge owner)
            {
                this.owner = owner;
            }

            public void OnInitSuccess()
            {
                owner.CompleteInitialization(true, null);
            }

            public void OnInitFail(int errorCode, string errorMessage)
            {
                owner.CompleteInitialization(false, $"{errorCode}: {errorMessage}");
            }
        }
    }
}
