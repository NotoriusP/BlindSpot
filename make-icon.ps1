param([string]$Src = "UI\Asset\Tray_icon.png", [string]$Out = "UI\Asset\Tray_icon.ico")

# Windows wants a .ico for the executable's embedded icon; the source art is a
# 1024x1024 PNG with alpha. Build a multi-size icon so the shell can pick the
# best fit for the taskbar, Alt+Tab and the file explorer.

Add-Type -AssemblyName System.Drawing

$srcPath = (Resolve-Path $Src).Path
$srcBmp = New-Object System.Drawing.Bitmap $srcPath
$srcImg = [System.Drawing.Image]$srcBmp
$sizes = @(256, 64, 48, 32, 24, 16)

$ms = New-Object System.IO.MemoryStream
$bw = New-Object System.IO.BinaryWriter $ms

# ICONDIR
$bw.Write([UInt16]0)          # reserved
$bw.Write([UInt16]1)          # type: icon
$bw.Write([UInt16]$sizes.Count)

# Pre-render every size into its own PNG-encoded entry.
$entries = @()
$offset = 6 + 16 * $sizes.Count
foreach ($s in $sizes) {
  $bmp = New-Object System.Drawing.Bitmap $s, $s
  $g = [System.Drawing.Graphics]::FromImage($bmp)
  $g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
  $g.PixelOffsetMode   = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
  $g.SmoothingMode     = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality
  $g.Clear([System.Drawing.Color]::Transparent)
  $g.DrawImage($srcImg, 0, 0, $s, $s)
  $g.Dispose()

  $pngMs = New-Object System.IO.MemoryStream
  $bmp.Save($pngMs, [System.Drawing.Imaging.ImageFormat]::Png)
  $bmp.Dispose()
  $data = $pngMs.ToArray()
  $pngMs.Dispose()

  $entries += ,@{ size = $s; data = $data }

  # ICONDIRENTRY
  $bw.Write([Byte]$(if ($s -ge 256) { 0 } else { $s }))   # width (0 = 256)
  $bw.Write([Byte]$(if ($s -ge 256) { 0 } else { $s }))   # height
  $bw.Write([Byte]0)    # colors in palette
  $bw.Write([Byte]0)    # reserved
  $bw.Write([UInt16]1)  # color planes
  $bw.Write([UInt16]32) # bits per pixel
  $bw.Write([UInt32]$data.Length)
  $bw.Write([UInt32]$offset)
  $offset += $data.Length
}

foreach ($e in $entries) { $bw.Write($e.data) }
$bw.Flush()

$outPath = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($Out)
[System.IO.File]::WriteAllBytes($outPath, $ms.ToArray())
$bw.Dispose(); $ms.Dispose(); $srcBmp.Dispose()

"$Out written ($((Get-Item $Out).Length) bytes, $($sizes.Count) sizes)"
