# List all methods in the Moza SDK
Add-Type -Path "$PSScriptRoot\..\lib\MozaSDK\MOZA_API_CSharp.dll"

$methods = [mozaAPI.mozaAPI].GetMethods() |
    Where-Object { $_.DeclaringType.Name -eq 'mozaAPI' } |
    Select-Object -ExpandProperty Name |
    Sort-Object -Unique

Write-Host "Available SDK Methods:" -ForegroundColor Cyan
foreach ($method in $methods) {
    Write-Host "  $method"
}
