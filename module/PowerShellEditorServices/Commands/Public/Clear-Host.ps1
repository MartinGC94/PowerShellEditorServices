# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

Microsoft.PowerShell.Management\Get-Item function:Clear-Host | Microsoft.PowerShell.Management\Set-Item function:__clearhost

function Clear-Host {
    [Alias('cls')]
    param()

    __clearhost
    if ($host.Runspace.LanguageMode -eq [System.Management.Automation.PSLanguageMode]::FullLanguage) {
        $psEditor.Window.Terminal.Clear()
    }
}

if (!$IsMacOS -or $IsLinux) {
    Microsoft.PowerShell.Utility\Set-Alias -Name clear -Value Clear-Host -Option AllScope
}
