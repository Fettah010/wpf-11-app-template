# DevTemWpf.UI — 1.8.0 prototype

> **Experimental:** this package is an architecture spike, not a supported
> 1.x API or a stable public package. Do not depend on its package ID, version,
> resource keys or layout for production applications.

The package contains the reference app's compiled design-token dictionaries.
It deliberately does not contain application pages, controls, shell behavior
or services. The consuming app remains the composition root.

## Prototype consumer

The package-only canary in `experiments/1.8.0/ui-package-consumer/` demonstrates
the intended merge order. The app's theme and controls dictionaries come first;
then the library's semantic colors, typography, spacing and shape tokens:

```xml
<ResourceDictionary.MergedDictionaries>
  <ui:ThemesDictionary Theme="Dark" />
  <ui:ControlsDictionary />
  <ResourceDictionary Source="/DevTemWpf.UI;component/DesignTokens/SemanticColors.xaml" />
  <ResourceDictionary Source="/DevTemWpf.UI;component/DesignTokens/Typography.xaml" />
  <ResourceDictionary Source="/DevTemWpf.UI;component/DesignTokens/Spacing.Comfortable.xaml" />
  <ResourceDictionary Source="/DevTemWpf.UI;component/DesignTokens/Shapes.xaml" />
</ResourceDictionary.MergedDictionaries>
```

The density dictionary is selected by the app and can be replaced at runtime:

```xml
<ResourceDictionary Source="/DevTemWpf.UI;component/DesignTokens/Spacing.Compact.xaml" />
```

The app must use the library's referenced-assembly pack URI form and keep
WPF-UI dictionaries earlier in the merge order. Do not copy the prototype's
token keys into application code without treating them as unstable.

## Verification

From the repository root, run:

```powershell
powershell -ExecutionPolicy Bypass -File eng/test-ui-package.ps1
```

This packs the prototype, restores a clean temporary WPF consumer from the
local package plus NuGet, builds with warnings as errors, and launches it. The
temporary consumer has no project reference to the gallery app.
