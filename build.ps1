Write-Host "正在编译..." -ForegroundColor Cyan
taskkill /im ClassIsland.Desktop.exe /f
dotnet build -c Debug
if ($LASTEXITCODE -eq 0) {
    Write-Host "编译成功，正在启动..." -ForegroundColor Green
    Write-Host "提示：此版本为调试版本，请不要将此版本打包发布。" -ForegroundColor Yellow
    # 请将下面的路径替换为你本地的 ClassIsland.exe 路径。
    & D:\path\to\classisland\v1\debug\binary\ClassIsland.exe -epp $PWD\AgentIsland\bin\Debug\net8.0-windows
}
else {
    Write-Host "编译失败。" -ForegroundColor Red
}