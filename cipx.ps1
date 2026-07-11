Write-Host "正在创建 ClassIsland 插件包..." -ForegroundColor Cyan
taskkill /im ClassIsland.Desktop.exe /f
dotnet publish -c Release -p:CreateCipx=true
if ($LASTEXITCODE -eq 0) {
    Write-Host "ClassIsland 插件包创建成功! 位置: $PWD\AgentIsland\cipx" -ForegroundColor Green
    Write-Host "请参考 https://docs.classisland.tech/dev/plugins/publishing.html 来发布插件。" -ForegroundColor Yellow
} else {
    Write-Host "ClassIsland 插件包创建失败。" -ForegroundColor Red
}