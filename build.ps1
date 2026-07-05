Write-Host "正在编译..." -ForegroundColor Cyan
taskkill /im ClassIsland.Desktop.exe /f
dotnet build
if ($LASTEXITCODE -eq 0) {
    Write-Host "编译成功，正在启动..." -ForegroundColor Green
    Write-Host "提示：此版本为调试版本，请不要将此版本打包发布。" -ForegroundColor Yellow
    & $env:ClassIsland_DebugBinaryDirectory\ClassIsland.Desktop.exe -epp $PWD\bin\Debug\net8.0-windows
} else {
    Write-Host "编译失败。" -ForegroundColor Red
}