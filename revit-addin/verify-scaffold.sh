#!/bin/bash
# Verification script for BuildSpec Revit add-in scaffold
# Checks testable criteria from task buildspec-tde.1.4

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
PASS=0
FAIL=0

check() {
    if eval "$2"; then
        echo "  PASS: $1"
        ((PASS++))
    else
        echo "  FAIL: $1"
        ((FAIL++))
    fi
}

echo "=== BuildSpec Scaffold Verification ==="
echo ""

echo "[1] Required files exist"
check "BuildSpec.sln exists" "[ -f '$SCRIPT_DIR/BuildSpec.sln' ]"
check "BuildSpec.csproj exists" "[ -f '$SCRIPT_DIR/BuildSpec.csproj' ]"
check "BuildSpec.addin exists" "[ -f '$SCRIPT_DIR/BuildSpec.addin' ]"
check "App.cs exists" "[ -f '$SCRIPT_DIR/App.cs' ]"
check "ChatPanel.xaml exists" "[ -f '$SCRIPT_DIR/ChatPanel.xaml' ]"
check "ChatPanel.xaml.cs exists" "[ -f '$SCRIPT_DIR/ChatPanel.xaml.cs' ]"
echo ""

echo "[2] .addin has unique GUID (not Archie's B5F5C9A2-7D3E-4A1B-9C8F-2E6D4A3B1C0D)"
ARCHIE_ADDIN_GUID="B5F5C9A2-7D3E-4A1B-9C8F-2E6D4A3B1C0D"
check ".addin does not contain Archie GUID" "! grep -qi '$ARCHIE_ADDIN_GUID' '$SCRIPT_DIR/BuildSpec.addin'"
check ".addin contains an AddInId" "grep -q '<AddInId>' '$SCRIPT_DIR/BuildSpec.addin'"
echo ""

echo "[3] No IronPython dependency"
check "No IronPython in .csproj" "! grep -qi 'IronPython' '$SCRIPT_DIR/BuildSpec.csproj'"
check "No IronPython in App.cs" "! grep -qi 'IronPython' '$SCRIPT_DIR/App.cs'"
echo ""

echo "[4] Ribbon tab shows 'BuildSpec'"
check "App.cs contains BuildSpec tab name" "grep -q '\"BuildSpec\"' '$SCRIPT_DIR/App.cs'"
check "App.cs has PushButton for toggle" "grep -q 'PushButtonData' '$SCRIPT_DIR/App.cs'"
echo ""

echo "[5] Correct namespace and classes"
check "App.cs namespace is BuildSpec" "grep -q 'namespace BuildSpec' '$SCRIPT_DIR/App.cs'"
check "App.cs implements IExternalApplication" "grep -q 'IExternalApplication' '$SCRIPT_DIR/App.cs'"
check "ShowChatPanelCommand implements IExternalCommand" "grep -q 'IExternalCommand' '$SCRIPT_DIR/App.cs'"
check "ChatPanel implements IDockablePaneProvider" "grep -q 'IDockablePaneProvider' '$SCRIPT_DIR/ChatPanel.xaml.cs'"
echo ""

echo "[6] No Archie leftovers"
ARCHIE_PANE_GUID="C4A72B5E-8F3D-4E9A-B1C6-5D2E7F8A9B0C"
check "No Archie pane GUID in App.cs" "! grep -qi '$ARCHIE_PANE_GUID' '$SCRIPT_DIR/App.cs'"
check "No ArchieCopilot namespace" "! grep -q 'namespace ArchieCopilot' '$SCRIPT_DIR/App.cs'"
check "No ExternalEvent in App.cs" "! grep -q 'ExternalEvent' '$SCRIPT_DIR/App.cs'"
check "No RevitCommandHandler in App.cs" "! grep -q 'RevitCommandHandler' '$SCRIPT_DIR/App.cs'"
echo ""

echo "[7] .csproj configuration"
check "Targets net8.0-windows" "grep -q 'net8.0-windows' '$SCRIPT_DIR/BuildSpec.csproj'"
check "UseWPF enabled" "grep -q '<UseWPF>true</UseWPF>' '$SCRIPT_DIR/BuildSpec.csproj'"
check "PlatformTarget x64" "grep -q '<PlatformTarget>x64</PlatformTarget>' '$SCRIPT_DIR/BuildSpec.csproj'"
check "Newtonsoft.Json referenced" "grep -q 'Newtonsoft.Json' '$SCRIPT_DIR/BuildSpec.csproj'"
check "RevitAPI referenced" "grep -q 'RevitAPI' '$SCRIPT_DIR/BuildSpec.csproj'"
check "RevitAPIUI referenced" "grep -q 'RevitAPIUI' '$SCRIPT_DIR/BuildSpec.csproj'"
echo ""

echo "=== Results: $PASS passed, $FAIL failed ==="
[ "$FAIL" -eq 0 ] && echo "All checks passed." || echo "Some checks failed."
exit $FAIL
