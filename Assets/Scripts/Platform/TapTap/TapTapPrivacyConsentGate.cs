using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class TapTapPrivacyConsentGate : MonoBehaviour
{
    private const string ConsentKey = "katana_taptap_privacy_consent_version";
    private const string EmbeddedPolicyNotice =
        "当前为内置隐私政策。游戏会在本地保存防沉迷服务使用的随机玩家标识，并在同意后连接 TapTap 实名与防沉迷服务及 TapADN 广告服务。个人开发者请在发布前核实 SDK 数据范围，替换为完整政策文本并提供联系邮箱。";
    private const string EmbeddedTermsNotice =
        "当前为内置用户协议。玩家应遵守法律法规和平台规则，不得利用程序漏洞或破坏服务。游戏内容、服务范围和数据处理方式以正式发布的用户协议为准。个人开发者请在发布前替换为自己的完整协议文本，并提供联系邮箱。";
    private static readonly Color Backdrop = new Color(0.025f, 0.04f, 0.055f, 0.94f);
    private static readonly Color Panel = new Color(0.12f, 0.16f, 0.18f, 1f);
    private static readonly Color Primary = new Color(0.19f, 0.55f, 0.48f, 1f);
    private static readonly Color Secondary = new Color(0.23f, 0.29f, 0.31f, 1f);

    private bool accepted;
    private bool finished;
    private bool checkedConsent;
    private bool canAccept;
    private bool allowOfflineContinue;
    private string consentVersion;
    private Button acceptButton;
    private Button consentToggle;
    private Font font;

    public static bool HasCurrentConsent(string version, string policyUrl, string termsUrl)
    {
        return HasUsableDocuments(policyUrl, termsUrl) &&
               !string.IsNullOrWhiteSpace(version) &&
               PlayerPrefs.GetString(ConsentKey, string.Empty) == version.Trim();
    }

    public static IEnumerator Show(
        string policyUrl,
        string termsUrl,
        string version,
        Font font,
        bool allowOfflineContinue,
        Action<bool> onDecision)
    {
        if (HasCurrentConsent(version, policyUrl, termsUrl))
        {
            onDecision?.Invoke(true);
            yield break;
        }

        var host = new GameObject("TapTapPrivacyConsentGate");
        var gate = host.AddComponent<TapTapPrivacyConsentGate>();
        gate.font = font;
        gate.allowOfflineContinue = allowOfflineContinue;
        gate.consentVersion = string.IsNullOrWhiteSpace(version) ? string.Empty : version.Trim();
        gate.canAccept = HasUsableDocuments(policyUrl, termsUrl) && !string.IsNullOrWhiteSpace(gate.consentVersion);
        gate.Build(policyUrl, termsUrl, version);
        while (!gate.finished)
        {
            yield return null;
        }

        onDecision?.Invoke(gate.accepted);
        Destroy(host);
    }

    private void Build(string policyUrl, string termsUrl, string version)
    {
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10000;
        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;
        gameObject.AddComponent<GraphicRaycaster>();
        if (EventSystem.current == null)
        {
            gameObject.AddComponent<EventSystem>();
            gameObject.AddComponent<StandaloneInputModule>();
        }

        var shade = CreateImage(transform, "Backdrop", Backdrop);
        Stretch(shade.rectTransform);
        var card = CreateImage(transform, "ConsentPanel", Panel);
        SetCentered(card.rectTransform, new Vector2(920, 1050));

        CreateText(card.transform, "Title", "隐私保护指引", 54, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -94), new Vector2(820, 100));
        CreateText(card.transform, "Summary",
            "为了提供实名认证、防沉迷和广告服务，相关 SDK 仅在你同意隐私政策和用户协议后初始化。当前未填写网页地址时显示内置测试文本。",
            32, TextAnchor.UpperLeft, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0, -240), new Vector2(790, 190));

        var policy = CreateButton(card.transform, "PrivacyPolicy",
            IsHttpUrl(policyUrl) ? "查看隐私政策" : "查看内置隐私政策", new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f), new Vector2(0, -455), new Vector2(760, 92), Secondary,
            () => ShowEmbeddedNotice(EmbeddedPolicyNotice, policyUrl));
        policy.interactable = IsHttpUrl(policyUrl);
        var terms = CreateButton(card.transform, "TermsOfService",
            IsHttpUrl(termsUrl) ? "查看用户协议" : "查看内置用户协议", new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f), new Vector2(0, -570), new Vector2(760, 92), Secondary,
            () => ShowEmbeddedNotice(EmbeddedTermsNotice, termsUrl));
        terms.interactable = IsHttpUrl(termsUrl);

        consentToggle = CreateButton(card.transform, "ConsentToggle", "□ 我已阅读并同意以上协议",
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -705),
            new Vector2(790, 90), Panel, ToggleConsent);
        CreateText(card.transform, "MissingLinks",
            HasValidLinks(policyUrl, termsUrl) ? string.Empty : "当前使用内置测试文本；正式发布前请填写两条 HTTPS 政策地址。",
            26, TextAnchor.MiddleCenter, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0, -800), new Vector2(790, 90));

        acceptButton = CreateButton(card.transform, "Accept", "同意并继续", new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f), new Vector2(0, -900), new Vector2(760, 92), Primary, Accept);
        CreateButton(card.transform, "OfflineContinue",
            allowOfflineContinue ? "暂不同意，离线继续" : "不同意并退出游戏", new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f), new Vector2(0, -1005), new Vector2(760, 80), Secondary,
            ContinueOffline);

        acceptButton.interactable = false;
        if (string.IsNullOrWhiteSpace(version))
        {
            Debug.LogWarning("Privacy policy version is empty; consent cannot be saved.");
        }
    }

    private void ToggleConsent()
    {
        checkedConsent = !checkedConsent;
        SetButtonLabel(consentToggle, checkedConsent ? "☑ 我已阅读并同意以上协议" : "□ 我已阅读并同意以上协议");
        acceptButton.interactable = checkedConsent && canAccept;
    }

    private void Accept()
    {
        if (!checkedConsent || !canAccept)
        {
            return;
        }

        accepted = true;
        finished = true;
        PlayerPrefs.SetString(ConsentKey, consentVersion);
        PlayerPrefs.Save();
    }

    private void ContinueOffline()
    {
        if (!allowOfflineContinue)
        {
            Application.Quit();
            return;
        }

        finished = true;
    }

    private static bool HasValidLinks(string policyUrl, string termsUrl)
    {
        return IsHttpUrl(policyUrl) && IsHttpUrl(termsUrl);
    }

    private static bool HasUsableDocuments(string policyUrl, string termsUrl)
    {
        if (HasValidLinks(policyUrl, termsUrl))
        {
            return true;
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        return string.IsNullOrWhiteSpace(policyUrl) && string.IsNullOrWhiteSpace(termsUrl);
#else
        return false;
#endif
    }

    private static bool IsHttpUrl(string value)
    {
        return Uri.TryCreate(value, UriKind.Absolute, out var uri) &&
               uri.Scheme == Uri.UriSchemeHttps;
    }

    private static void OpenLink(string value)
    {
        if (IsHttpUrl(value))
        {
            Application.OpenURL(value.Trim());
        }
    }

    private static void ShowEmbeddedNotice(string notice, string url)
    {
        if (IsHttpUrl(url))
        {
            OpenLink(url);
            return;
        }

        Debug.Log(notice);
    }

    private Image CreateImage(Transform parent, string objectName, Color color)
    {
        var item = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        item.transform.SetParent(parent, false);
        var image = item.GetComponent<Image>();
        image.color = color;
        return image;
    }

    private Button CreateButton(Transform parent, string objectName, string label, Vector2 anchor,
        Vector2 pivot, Vector2 position, Vector2 size, Color color, UnityEngine.Events.UnityAction action)
    {
        var image = CreateImage(parent, objectName, color);
        SetAnchored(image.rectTransform, anchor, pivot, position, size);
        var button = image.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(action);
        CreateText(image.transform, "Label", label, 30, TextAnchor.MiddleCenter,
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero).rectTransform.offsetMin = Vector2.zero;
        return button;
    }

    private Text CreateText(Transform parent, string objectName, string value, int size, TextAnchor alignment,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 position, Vector2 dimensions)
    {
        var item = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        item.transform.SetParent(parent, false);
        var rect = item.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = dimensions;
        var text = item.GetComponent<Text>();
        text.text = value;
        text.font = font != null ? font : Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = size;
        text.color = Color.white;
        text.alignment = alignment;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.raycastTarget = false;
        return text;
    }

    private static void SetButtonLabel(Button button, string label)
    {
        var text = button.GetComponentInChildren<Text>();
        if (text != null)
        {
            text.text = label;
        }
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static void SetCentered(RectTransform rect, Vector2 size)
    {
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = size;
    }

    private static void SetAnchored(RectTransform rect, Vector2 anchor, Vector2 pivot,
        Vector2 position, Vector2 size)
    {
        rect.anchorMin = rect.anchorMax = anchor;
        rect.pivot = pivot;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }
}
