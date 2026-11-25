# TapTap Compliance

The game uses TapTap only for real-name verification and anti-addiction checks,
and TapADN for advertising. It does not call TapTap Login, TapDB analytics,
cloud save, achievements, leaderboards, or social APIs.

## Player identifier

The compliance bridge creates a random identifier and stores it in PlayerPrefs.
It sends this identifier to the TapTap compliance SDK after privacy consent.
The identifier is local to one installation: clearing app data, reinstalling,
or changing devices creates a new identifier. It is not a cross-device account.

The official compliance package declares `com.taptap.sdk.login` as a package
dependency. Unity Package Manager therefore downloads that package, but the game
does not initialize it or call any login API. Removing the dependency would
require modifying or replacing the official compliance package.

## Required project configuration

- `com.taptap.sdk.core` and `com.taptap.sdk.compliance` use the same SDK version.
- `KATANA_TAPTAP_SDK_V4` is enabled for Android builds.
- The TapTap developer application uses the exact Android package name and
  release signing certificate configured for this game.
- `Managers` contains `TapTapSdkCoreBridge`, `TapTapComplianceBridge`, and
  `TapTapComplianceStartupGate`.
- Privacy policy and user agreement URLs are valid HTTPS links in the prefab.

The Android startup flow enters gameplay only when compliance returns code
`500`. Restricted or incomplete checks keep gameplay blocked.
