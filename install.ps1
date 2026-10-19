<#
.SYNOPSIS
  Installs (or removes) the TotalAgility connector in Power PDF.

.DESCRIPTION
  Run from an elevated PowerShell, from the folder that holds PPDF.TotalAgility.Connector.dll:
      .\install.ps1                     # install / update
      .\install.ps1 -Uninstall          # unregister and remove the DLLs
  What it does (same as steps 1-5 of the README):
    1. copies the connector files to <Power PDF>\bin\Connectors
    2. registers the DLL with regasm /codebase (32-bit regasm for Program Files (x86))
    3. adds the TotalAgility buttons to "Publish Mode.xml" and the display name to every NameAndTitle.xml
    4. deletes the cached %appdata%\Kofax\PDF\PowerPDF\UILayout\Publish.xml
  Edited XML files get a one-time .bak copy next to them.
#>
param(
    [string]$PowerPdfRoot = "C:\Program Files (x86)\Kofax\Power PDF 51",
    [string]$SourceDir = $PSScriptRoot,
    [switch]$Uninstall
)

$ErrorActionPreference = 'Stop'

$PublishModeSnippet = @'
<PFFGroup name="connector::TotalAgility" GroupType="PFFTitleBlock" autoZip="0">
    <PFFButton name="connector::tool::TotalAgility:menuItem0"/>
    <PFFButton name="connector::tool::TotalAgility:menuItem1"/>
</PFFGroup>
'@

$NameSnippet = '<PFFGroup name="connector::TotalAgility" title="Send to TotalAgility" />'

function Edit-Utf16Xml {
    param([string]$Path, [scriptblock]$Transform)

    $reader = New-Object System.IO.StreamReader($Path, $true)
    try { $text = $reader.ReadToEnd(); $enc = $reader.CurrentEncoding } finally { $reader.Dispose() }

    $new = & $Transform $text
    if ($new -eq $text) { return $false }

    if (-not (Test-Path "$Path.bak")) { Copy-Item $Path "$Path.bak" }
    [System.IO.File]::WriteAllText($Path, $new, $enc)
    return $true
}

function Add-PublishModeEntry {
    param([string]$Path)
    Edit-Utf16Xml $Path {
        param($text)
        if ($text -match 'connector::TotalAgility') { return $text }
        $rx = [regex]'(<toolbar\s+name="connectors"[^>]*>)'
        if (-not $rx.IsMatch($text)) { throw "No connectors toolbar found in $Path" }
        $rx.Replace($text, ('$1' + "`r`n" + $PublishModeSnippet), 1)
    }
}

function Add-NameEntry {
    param([string]$Path)
    Edit-Utf16Xml $Path {
        param($text)
        if ($text -match 'connector::TotalAgility') { return $text }
        $rx = [regex]'(<toolbar\s+name="connectors"[^>]*/>)'
        if (-not $rx.IsMatch($text)) { throw "No connectors toolbar title found in $Path" }
        $rx.Replace($text, ('$1' + "`r`n    " + $NameSnippet), 1)
    }
}

function Remove-TotalAgilityEntries {
    param([string]$Path)
    Edit-Utf16Xml $Path {
        param($text)
        $t = [regex]::Replace($text, '\s*<PFFGroup name="connector::TotalAgility"[^>]*?/>', '')
        [regex]::Replace($t, '\s*<PFFGroup name="connector::TotalAgility"[^>]*>.*?</PFFGroup>', '', 'Singleline')
    }
}

function Test-Admin {
    ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole(
        [Security.Principal.WindowsBuiltInRole]::Administrator)
}

# Only run the installer when executed, not when dot-sourced for testing
if ($MyInvocation.InvocationName -eq '.') { return }

if (-not (Test-Admin)) { throw "Run this script from an elevated (Administrator) PowerShell." }
if (Get-Process PowerPDF -ErrorAction SilentlyContinue) { throw "Close Power PDF first." }

$connectors = Join-Path $PowerPdfRoot "bin\Connectors"
$resource = Join-Path $PowerPdfRoot "Resource\PowerPDF"
$dll = Join-Path $connectors "PPDF.TotalAgility.Connector.dll"
if (-not (Test-Path $connectors)) { throw "Connectors folder not found: $connectors" }

$fw = if ($PowerPdfRoot -like '*(x86)*') { 'Framework' } else { 'Framework64' }
$regasm = Join-Path $env:WINDIR "Microsoft.NET\$fw\v4.0.30319\regasm.exe"

if ($Uninstall) {
    if (Test-Path $dll) { & $regasm /unregister $dll }
    Remove-TotalAgilityEntries (Join-Path $resource "UILayout\Publish Mode.xml") | Out-Null
    Get-ChildItem $resource -Recurse -Filter NameAndTitle.xml | ForEach-Object { Remove-TotalAgilityEntries $_.FullName | Out-Null }
    foreach ($item in "PPDF.TotalAgility.Connector.dll", "de", "zh-TW", "runtimes", "Microsoft.Web.WebView2.Core.dll", "Microsoft.Web.WebView2.WinForms.dll") {
        $p = Join-Path $connectors $item
        if (Test-Path $p) { Remove-Item $p -Recurse -Force }
    }
    Write-Host "Removed. (Newtonsoft.Json.dll was left in place; other connectors may use it.)"
}
else {
    if (-not (Test-Path (Join-Path $SourceDir "PPDF.TotalAgility.Connector.dll"))) {
        throw "PPDF.TotalAgility.Connector.dll not found in $SourceDir"
    }

    Write-Host "Copying files to $connectors"
    Get-ChildItem $SourceDir -Exclude "install.ps1" | Copy-Item -Destination $connectors -Recurse -Force

    Write-Host "Registering with $regasm"
    & $regasm $dll /codebase
    if ($LASTEXITCODE -ne 0) { throw "regasm failed (exit code $LASTEXITCODE)" }

    $pm = Join-Path $resource "UILayout\Publish Mode.xml"
    Write-Host ("Publish Mode.xml: " + $(if (Add-PublishModeEntry $pm) { "updated" } else { "already up to date" }))

    foreach ($f in Get-ChildItem $resource -Recurse -Filter NameAndTitle.xml) {
        Write-Host ($f.FullName.Substring($resource.Length + 1) + ": " + $(if (Add-NameEntry $f.FullName) { "updated" } else { "already up to date" }))
    }
}

$cache = Join-Path $env:APPDATA "Kofax\PDF\PowerPDF\UILayout\Publish.xml"
if (Test-Path $cache) { Remove-Item $cache -Force; Write-Host "Deleted cached $cache" }
Write-Host "Done. Start Power PDF."
