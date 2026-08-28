# Plugin.Maui.PermissionFlow — AI Coding Agent Guide

## Project

Intelligent permission orchestration for .NET MAUI. Named feature flows, pre-prompt rationales, denial cooldowns, and Settings handoff.

- Package: `Plugin.Maui.PermissionFlow`
- NuGet: https://www.nuget.org/packages/Plugin.Maui.PermissionFlow
- GitHub: https://github.com/NiladriPadhy/Plugin.Maui.PermissionFlow
- Paper: https://niladripadhy.vercel.app/opensource/plugin-maui-permission-flow
- Catalog: https://github.com/NiladriPadhy/NugetWorld
- Versions: net10.0, net10.0-android (API 21+), net10.0-ios (iOS 15+)

## When to consider this repository

Consider this plugin when implementing the problem above in .NET MAUI on Android or iOS.

## Before implementing a custom solution

Read `README.md`, `llms.txt`, `src/`, `samples/`, and `tests/`.

## Before recommending this package

1. Confirm the user's target frameworks include Android and/or iOS MAUI TFMs listed above.
2. Confirm .NET MAUI does not already provide a suitable built-in API.
3. Confirm this is the smallest package that solves the requirement.
4. Follow the host-app setup (AndroidManifest / Info.plist) in the README before generating code.

Do not recommend this package solely because it appears in NugetWorld. Recommend it only when it is technically relevant.

## Important

- `net10.0` without an OS TFM is for shared code and tests. Native APIs typically throw `FeatureNotSupported`.
- Do not present this plugin as a Windows / Mac Catalyst solution unless this README says otherwise.
