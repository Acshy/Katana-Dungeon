# 发布隐私政策和用户协议

1. 把 `privacy.html` 和 `terms.html` 中的开发者名称、邮箱以及 SDK 数据范围改成真实内容。
2. 将两个文件发布到 GitHub Pages、Gitee Pages、Cloudflare Pages 或其他 HTTPS 静态托管。
3. 把公开地址分别填入 `Assets/Prefabs/Managers/Managers.prefab` 的 `privacyPolicyUrl` 和 `termsOfServiceUrl`。
4. 发布 Android Release 前，在真实设备上测试首次启动、登录、实名认证、防沉迷限制、退出和重试。

编辑器和 Development Build 在两个地址为空时会使用内置测试文本；正式 Android Release 必须填写有效 HTTPS 地址。
