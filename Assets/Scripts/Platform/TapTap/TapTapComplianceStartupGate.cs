using System;
using System.Collections;
using KatanaDungeon.ThirdParty.TapTap;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Blocks Android gameplay until TapTap compliance returns code 500.
/// </summary>
public sealed class TapTapComplianceStartupGate : MonoBehaviour
{
    private static readonly Color Backdrop = new Color(0.025f, 0.04f, 0.055f, 0.94f);
    private static readonly Color Panel = new Color(0.12f, 0.16f, 0.18f, 1f);
    private static readonly Color Primary = new Color(0.19f, 0.55f, 0.48f, 1f);
    private static readonly Color Secondary = new Color(0.23f, 0.29f, 0.31f, 1f);

    private KatanaDungeon.ThirdParty.TapTap.TapTapSdkCoreBridge sdkCore;
    private TapTapComplianceBridge compliance;
    private Font font;
    private Text statusText;
    private Button verifyButton;
    private bool completed;
    private bool approved;
    private bool verificationStarted;

    public static IEnumerator WaitForApproval(
        KatanaDungeon.ThirdParty.TapTap.TapTapSdkCoreBridge sdkCore,
        TapTapComplianceBridge compliance,
        Font font,
        Action<bool> onCompleted)
    {
        var host = new GameObject("TapTapComplianceStartupGate");
        var gate = host.AddComponent<TapTapComplianceStartupGate>();
        gate.sdkCore = sdkCore;
        gate.compliance = compliance;
        gate.font = font;
        gate.Build();
        gate.Subscribe();
        gate.Begin();

        while (!gate.completed)
        {
            yield return null;
        }

        gate.Unsubscribe();
        onCompleted?.Invoke(gate.approved);
        Destroy(host);
    }

    private void Build()
    {
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10001;
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
        var card = CreateImage(transform, "CompliancePanel", Panel);
        SetCentered(card.rectTransform, new Vector2(920, 860));

        CreateText(card.transform, "Title", "实名与防沉迷验证", 48, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -92), new Vector2(820, 96));
        CreateText(card.transform, "Description",
            "完成实名认证与防沉迷校验后，才能开始游戏。校验通过前不会进入游戏。",
            30, TextAnchor.UpperLeft, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0, -235), new Vector2(780, 150));
        statusText = CreateText(card.transform, "Status", "正在检查 TapTap 服务…", 28,
            TextAnchor.MiddleCenter, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0, -420), new Vector2(790, 120));
        verifyButton = CreateButton(card.transform, "Verify", "重新验证",
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -590),
            new Vector2(760, 96), Primary, Verify);
        CreateButton(card.transform, "Quit", "退出游戏", new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f), new Vector2(0, -720), new Vector2(760, 84),
            Secondary, Quit);
    }

    private void Subscribe()
    {
        KatanaDungeon.ThirdParty.TapTap.TapTapSdkCoreBridge.InitializationCompleted += OnSdkInitialized;
        if (compliance != null)
        {
            compliance.LoginCompleted += OnLoginCompleted;
            compliance.ComplianceCompleted += OnComplianceCompleted;
        }
    }

    private void Unsubscribe()
    {
        KatanaDungeon.ThirdParty.TapTap.TapTapSdkCoreBridge.InitializationCompleted -= OnSdkInitialized;
        if (compliance != null)
        {
            compliance.LoginCompleted -= OnLoginCompleted;
            compliance.ComplianceCompleted -= OnComplianceCompleted;
        }
    }

    private void Begin()
    {
        if (sdkCore == null || compliance == null)
        {
            SetStatus("TapTap 防沉迷组件缺失，无法开始游戏。");
            verifyButton.interactable = false;
            completed = true;
            return;
        }

        if (KatanaDungeon.ThirdParty.TapTap.TapTapSdkCoreBridge.IsInitialized)
        {
            OnSdkInitialized(true, null);
            return;
        }

        if (!KatanaDungeon.ThirdParty.TapTap.TapTapSdkCoreBridge.IsInitializing)
        {
            sdkCore.InitializeAfterPrivacyConsent();
        }

        SetStatus("正在连接实名认证与防沉迷服务…");
        verifyButton.interactable = false;
    }

    private void Verify()
    {
        if (sdkCore == null || compliance == null)
        {
            return;
        }

        verifyButton.interactable = false;
        if (!KatanaDungeon.ThirdParty.TapTap.TapTapSdkCoreBridge.IsInitialized)
        {
            SetStatus("正在重试连接防沉迷服务…");
            sdkCore.InitializeAfterPrivacyConsent();
            return;
        }

        SetStatus("正在重新进行实名与防沉迷校验…");
        compliance.StartCompliance();
    }

    private void OnSdkInitialized(bool success, string error)
    {
        if (!success)
        {
            SetStatus("防沉迷服务初始化失败，请重试或退出游戏。");
            verifyButton.interactable = true;
            return;
        }

        compliance.NotifyTapSdkInitialized(true);
        if (verificationStarted)
        {
            return;
        }

        verificationStarted = true;
        SetStatus("正在进行实名认证与防沉迷校验…");
        verifyButton.interactable = false;
        compliance.StartCompliance();
    }

    private void OnComplianceCompleted(int code, string message)
    {
        if (code == 500)
        {
            approved = true;
            completed = true;
            return;
        }

        verifyButton.interactable = code != 1030 && code != 1050 && code != 1100;
        if (code == 1000 || code == 1001)
        {
            verifyButton.interactable = true;
            SetStatus("校验未完成，可以重新验证或退出游戏。");
        }
        else if (code == 1030 || code == 1050 || code == 1100)
        {
            SetStatus("当前账号暂不能进入游戏。请按实名与防沉迷提示操作，或退出游戏。");
        }
        else
        {
            SetStatus("校验未完成，请检查网络或实名信息后重试。");
        }
    }

    private void Quit()
    {
        completed = true;
        approved = false;
        Application.Quit();
    }

    private void SetStatus(string value)
    {
        if (statusText != null)
        {
            statusText.text = value;
        }
    }

    private Image CreateImage(Transform parent, string objectName, Color color)
    {
        var item = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        item.transform.SetParent(parent, false);
        var image = item.GetComponent<Image>();
        image.color = color;
        return image;
    }

    private Button CreateButton(Transform parent, string objectName, string label,
        Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size, Color color,
        UnityEngine.Events.UnityAction action)
    {
        var image = CreateImage(parent, objectName, color);
        SetAnchored(image.rectTransform, anchor, pivot, position, size);
        var button = image.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(action);
        var text = CreateText(image.transform, "Label", label, 30, TextAnchor.MiddleCenter,
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        text.rectTransform.offsetMin = Vector2.zero;
        text.rectTransform.offsetMax = Vector2.zero;
        return button;
    }

    private Text CreateText(Transform parent, string objectName, string value, int size,
        TextAnchor alignment, Vector2 anchorMin, Vector2 anchorMax, Vector2 position, Vector2 dimensions)
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
