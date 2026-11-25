using System;
using UnityEngine;

#if KATANA_TAPTAP_SDK_V4
using TapSDK.Compliance;
#endif

/// <summary>
/// Starts TapTap real-name and anti-addiction checks with a locally stored player ID.
/// </summary>
public sealed class TapTapComplianceBridge : MonoBehaviour
{
    private const string UserIdentifierKey = "katana_taptap_compliance_user_id";

    public static TapTapComplianceBridge Instance { get; private set; }

    private bool tapSdkInitialized;
    private bool complianceCallbackRegistered;
    private bool verificationPending;
    private string userIdentifier;

    public bool TapSdkInitialized => tapSdkInitialized;
    public string UserIdentifier => userIdentifier;
    public event Action<int, string> ComplianceCompleted;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        userIdentifier = GetOrCreateUserIdentifier();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void NotifyTapSdkInitialized(bool initialized)
    {
        tapSdkInitialized = initialized;
        if (!initialized)
        {
            Debug.LogWarning("TapTap Core initialization failed; compliance is unavailable.");
            return;
        }

#if KATANA_TAPTAP_SDK_V4
        if (!complianceCallbackRegistered)
        {
            TapTapCompliance.RegisterComplianceCallback(OnComplianceResult);
            complianceCallbackRegistered = true;
        }
#endif
    }

    public void StartCompliance()
    {
#if KATANA_TAPTAP_SDK_V4
        if (!tapSdkInitialized)
        {
            ComplianceCompleted?.Invoke(1200, "TapTap Core is not initialized.");
            return;
        }

        if (verificationPending)
        {
            return;
        }

        verificationPending = true;
        TapTapCompliance.Startup(userIdentifier);
#else
        ComplianceCompleted?.Invoke(1200, "TapTap compliance package is not installed.");
#endif
    }

#if KATANA_TAPTAP_SDK_V4
    private void OnComplianceResult(int code, string extra)
    {
        verificationPending = false;
        ComplianceCompleted?.Invoke(code, extra);
    }
#endif

    private static string GetOrCreateUserIdentifier()
    {
        var identifier = PlayerPrefs.GetString(UserIdentifierKey, string.Empty);
        if (!string.IsNullOrWhiteSpace(identifier))
        {
            return identifier;
        }

        identifier = Guid.NewGuid().ToString("N");
        PlayerPrefs.SetString(UserIdentifierKey, identifier);
        PlayerPrefs.Save();
        return identifier;
    }
}
