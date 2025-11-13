$folder = "Models"
$model = "ggml-base.bin"
$url = "https://huggingface.co/ggerganov/whisper.cpp/resolve/main/ggml-base.bin"

if (!(Test-Path $folder)) { New-Item -ItemType Directory -Path $folder }

Write-Output "Descargando modelo Whisper base..."
Invoke-WebRequest -Uri $url -OutFile "$folder/$model"

Write-Output "Modelo descargado correctamente."
