$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$project = Join-Path $root 'Glyphore\Glyphore.csproj'
$version = '6.0.0'
Write-Host "Building Glyphore $version..." -ForegroundColor Cyan

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
  Write-Host 'No encuentro dotnet. Abre Visual Studio Installer > Modify e instala .NET desktop development.' -ForegroundColor Yellow
  exit 1
}

dotnet --version | Out-Host
$publish = Join-Path $root 'publish\win-x64'
$dist = Join-Path $root 'dist'
if (Test-Path $publish) { Remove-Item $publish -Recurse -Force }
New-Item -ItemType Directory -Path $publish -Force | Out-Null
New-Item -ItemType Directory -Path $dist -Force | Out-Null

dotnet publish $project -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:DebugType=None -p:DebugSymbols=false -o $publish
if ($LASTEXITCODE -ne 0) { throw 'dotnet publish failed' }

$pdb = Join-Path $publish 'Glyphore.pdb'
if (Test-Path $pdb) { Remove-Item $pdb -Force }
$exe = Join-Path $publish 'Glyphore.exe'
if (-not (Test-Path $exe)) { throw "No se genero $exe" }

# Validate the published single-file in a clean directory that contains only Glyphore.exe.
# The EXE creates the About -> Third-Party Licenses forms off-screen, opens the full
# license viewer and verifies that the complete embedded Apache-2.0 text is available.
$smokeRoot = Join-Path ([IO.Path]::GetTempPath()) ("Glyphore-license-smoke-" + [Guid]::NewGuid().ToString('N'))
try {
  New-Item -ItemType Directory -Path $smokeRoot -Force | Out-Null
  $smokeExe = Join-Path $smokeRoot 'Glyphore.exe'
  Copy-Item $exe $smokeExe -Force

  $beforeFiles = @(Get-ChildItem $smokeRoot -File -Force)
  if ($beforeFiles.Count -ne 1 -or $beforeFiles[0].Name -ne 'Glyphore.exe') {
    throw 'Single-file license smoke test directory contains unexpected files before launch.'
  }

  Write-Host 'Validating embedded third-party licenses from Glyphore.exe only...' -ForegroundColor Cyan
  $smoke = Start-Process -FilePath $smokeExe -ArgumentList '--self-test-third-party-licenses' -WorkingDirectory $smokeRoot -Wait -PassThru
  if ($smoke.ExitCode -ne 0) {
    $diag = Join-Path $smokeRoot 'license-smoke-error.txt'
    if (Test-Path $diag) {
      Write-Host 'Embedded-license smoke-test diagnostic:' -ForegroundColor Red
      Get-Content $diag | Write-Host
    }
    throw "Embedded third-party license smoke test failed with exit code $($smoke.ExitCode)."
  }

  $afterFiles = @(Get-ChildItem $smokeRoot -File -Force | Where-Object { $_.Name -ne 'Glyphore.exe' })
  if ($afterFiles.Count -ne 0) {
    throw "The license UI unexpectedly depended on or created companion files: $($afterFiles.Name -join ', ')"
  }
  Write-Host 'Embedded license UI smoke test: OK (single EXE, offline resource, full text scrollable).' -ForegroundColor Green

  Write-Host 'Validating ASCII Title charset/letter-spacing/default/save-load invariants...' -ForegroundColor Cyan
  $titleSmoke = Start-Process -FilePath $smokeExe -ArgumentList '--self-test-title-pipeline' -WorkingDirectory $smokeRoot -Wait -PassThru
  if ($titleSmoke.ExitCode -ne 0) {
    $diag = Join-Path $smokeRoot 'title-pipeline-smoke-error.txt'
    if (Test-Path $diag) {
      Write-Host 'ASCII Title smoke-test diagnostic:' -ForegroundColor Red
      Get-Content $diag | Write-Host
    }
    throw "ASCII Title pipeline smoke test failed with exit code $($titleSmoke.ExitCode)."
  }
  Write-Host 'ASCII Title pipeline smoke test: OK (charset, letter spacing, custom-color defaults and scene save/load).' -ForegroundColor Green

  Write-Host 'Validating Discord local RPC framing, partial reads, activity payloads and state priority...' -ForegroundColor Cyan
  $discordSmoke = Start-Process -FilePath $smokeExe -ArgumentList '--self-test-discord-rpc' -WorkingDirectory $smokeRoot -Wait -PassThru
  if ($discordSmoke.ExitCode -ne 0) {
    $diag = Join-Path $smokeRoot 'discord-rpc-smoke-error.txt'
    if (Test-Path $diag) {
      Write-Host 'Discord RPC smoke-test diagnostic:' -ForegroundColor Red
      Get-Content $diag | Write-Host
    }
    throw "Discord Rich Presence smoke test failed with exit code $($discordSmoke.ExitCode)."
  }
  Write-Host 'Discord RPC smoke test: OK (handshake/framing, partial reads, SET_ACTIVITY/Clear Activity, priority and reconnect cadence).' -ForegroundColor Green

  Write-Host 'Validating native window frame, monitor maximize and detached-window lifecycle...' -ForegroundColor Cyan
  $windowSmoke = Start-Process -FilePath $smokeExe -ArgumentList '--self-test-detached-windows' -WorkingDirectory $smokeRoot -Wait -PassThru
  if ($windowSmoke.ExitCode -ne 0) {
    $diag = Join-Path $smokeRoot 'detached-window-smoke-error.txt'
    if (Test-Path $diag) {
      Write-Host 'Detached-window smoke-test diagnostic:' -ForegroundColor Red
      Get-Content $diag | Write-Host
    }
    throw "Detached-window smoke test failed with exit code $($windowSmoke.ExitCode)."
  }
  Write-Host 'Window smoke test: OK (native Windows frame, themed wheel scrolling, min/max/restore, monitor work area and ownerless detached lifecycle).' -ForegroundColor Green
}
finally {
  if (Test-Path $smokeRoot) { Remove-Item $smokeRoot -Recurse -Force -ErrorAction SilentlyContinue }
}

$releaseZip = Join-Path $dist "Glyphore-$version-win-x64.zip"
if (Test-Path $releaseZip) { Remove-Item $releaseZip -Force }
Compress-Archive -Path $exe -DestinationPath $releaseZip -CompressionLevel Optimal

Write-Host "`nEXE para GitHub Releases:" -ForegroundColor Green
Write-Host "  $exe" -ForegroundColor White
Write-Host "ZIP binario opcional:" -ForegroundColor Green
Write-Host "  $releaseZip" -ForegroundColor White
