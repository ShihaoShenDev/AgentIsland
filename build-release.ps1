Write-Host "正在编译..." -ForegroundColor Cyan
taskkill /im ClassIsland.Desktop.exe /f
dotnet build -c Release
if ($LASTEXITCODE -eq 0) {
    Write-Host "编译成功，正在启动..." -ForegroundColor Green
    Write-Host "提示：此版本为发布版本，可以放心使用 ClassIsland 内部的打包功能打包并发布。" -ForegroundColor Yellow
    & $env:ClassIsland_DebugBinaryDirectory\ClassIsland.Desktop.exe -epp $PWD\AgentIsland\bin\Release\net8.0-windows
}
else {
    Write-Host "编译失败。" -ForegroundColor Red
}