# Build Script para RPRO
# Executa: .\build.ps1

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  RPRO Build Script" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

$ErrorActionPreference = "Stop"

# Limpar builds anteriores
Write-Host "`n[1/4] Limpando builds anteriores..." -ForegroundColor Yellow
dotnet clean -c Release

# Restaurar pacotes
Write-Host "`n[2/4] Restaurando pacotes NuGet..." -ForegroundColor Yellow
dotnet restore

# Compilar solução
Write-Host "`n[3/4] Compilando solução..." -ForegroundColor Yellow
dotnet build -c Release --no-restore

# Publicar
Write-Host "`n[4/4] Publicando aplicação..." -ForegroundColor Yellow
dotnet publish src/RPRO.App/RPRO.App.csproj `
    -c Release `
    -r win-x64 `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:PublishReadyToRun=true `
    -o ./publish

Write-Host "`n========================================" -ForegroundColor Green
Write-Host "  Build concluído com sucesso!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host "`nExecutável gerado em: ./publish/Cortez.exe" -ForegroundColor White

# Mostrar tamanho do arquivo
$exePath = "./publish/Cortez.exe"
if (Test-Path $exePath) {
    $size = (Get-Item $exePath).Length / 1MB
    Write-Host "Tamanho: $([math]::Round($size, 2)) MB" -ForegroundColor White
}

Write-Host "`nPressione qualquer tecla para sair..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")