#!/bin/bash
# Verification script for BuildScope Revit add-in scaffold
# Checks testable criteria from task buildscope-tde.1.4

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

echo "=== BuildScope Scaffold Verification ==="
echo ""

echo "[1] Required files exist"
check "BuildScope.sln exists" "[ -f '$SCRIPT_DIR/BuildScope.sln' ]"
check "BuildScope.csproj exists" "[ -f '$SCRIPT_DIR/BuildScope.csproj' ]"
check "BuildScope.addin exists" "[ -f '$SCRIPT_DIR/BuildScope.addin' ]"
check "App.cs exists" "[ -f '$SCRIPT_DIR/App.cs' ]"
check "ChatPanel.xaml exists" "[ -f '$SCRIPT_DIR/ChatPanel.xaml' ]"
check "ChatPanel.xaml.cs exists" "[ -f '$SCRIPT_DIR/ChatPanel.xaml.cs' ]"
echo ""

echo "[2] .addin has unique GUID (not Archie's B5F5C9A2-7D3E-4A1B-9C8F-2E6D4A3B1C0D)"
ARCHIE_ADDIN_GUID="B5F5C9A2-7D3E-4A1B-9C8F-2E6D4A3B1C0D"
check ".addin does not contain Archie GUID" "! grep -qi '$ARCHIE_ADDIN_GUID' '$SCRIPT_DIR/BuildScope.addin'"
check ".addin contains an AddInId" "grep -q '<AddInId>' '$SCRIPT_DIR/BuildScope.addin'"
echo ""

echo "[3] No IronPython dependency"
check "No IronPython in .csproj" "! grep -qi 'IronPython' '$SCRIPT_DIR/BuildScope.csproj'"
check "No IronPython in App.cs" "! grep -qi 'IronPython' '$SCRIPT_DIR/App.cs'"
echo ""

echo "[4] Ribbon tab shows 'BuildScope'"
check "App.cs contains BuildScope tab name" "grep -q '\"BuildScope\"' '$SCRIPT_DIR/App.cs'"
check "App.cs has PushButton for toggle" "grep -q 'PushButtonData' '$SCRIPT_DIR/App.cs'"
echo ""

echo "[5] Correct namespace and classes"
check "App.cs namespace is BuildScope" "grep -q 'namespace BuildScope' '$SCRIPT_DIR/App.cs'"
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
check "Targets net8.0-windows" "grep -q 'net8.0-windows' '$SCRIPT_DIR/BuildScope.csproj'"
check "UseWPF enabled" "grep -q '<UseWPF>true</UseWPF>' '$SCRIPT_DIR/BuildScope.csproj'"
check "PlatformTarget x64" "grep -q '<PlatformTarget>x64</PlatformTarget>' '$SCRIPT_DIR/BuildScope.csproj'"
check "Newtonsoft.Json referenced" "grep -q 'Newtonsoft.Json' '$SCRIPT_DIR/BuildScope.csproj'"
check "RevitAPI referenced" "grep -q 'RevitAPI' '$SCRIPT_DIR/BuildScope.csproj'"
check "RevitAPIUI referenced" "grep -q 'RevitAPIUI' '$SCRIPT_DIR/BuildScope.csproj'"
echo ""

echo "=== Results: $PASS passed, $FAIL failed ==="
[ "$FAIL" -eq 0 ] && echo "All checks passed." || echo "Some checks failed."
exit $FAIL
