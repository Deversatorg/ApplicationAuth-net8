$ErrorActionPreference = "Stop"

$baseUrl = "http://localhost:1310/api/v1"

Write-Host "--- TEST: Telegram POST Test (String) ---"
$testText = '"Hello Telegram Test!"'
try {
    $res1 = Invoke-RestMethod -Uri "$baseUrl/telegram/test" -Method Post -Body $testText -ContentType "application/json"
    Write-Host "POST Test Success: $($res1 | ConvertTo-Json -Depth 5 -Compress)"
}
catch {
    Write-Host "POST Test Failed: $($_.Exception.Message)"
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        Write-Host "Response Body: $($reader.ReadToEnd())"
    }
}

Write-Host "`n--- TEST: Telegram POST Normal ---"
$telegramBody = @{
    userToken = "userToken123"
    message   = "Hello from full model"
    isBot     = $false
} | ConvertTo-Json
try {
    $res2 = Invoke-RestMethod -Uri "$baseUrl/telegram" -Method Post -Body $telegramBody -ContentType "application/json"
    Write-Host "POST Normal Success: $($res2 | ConvertTo-Json -Depth 5 -Compress)"
}
catch {
    Write-Host "POST Normal Failed: $($_.Exception.Message)"
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        Write-Host "Response Body: $($reader.ReadToEnd())"
    }
}

Write-Host "`n--- TEST: Telegram GET Messages ---"
try {
    $res3 = Invoke-RestMethod -Uri "$baseUrl/telegram/userToken123/messages" -Method Get
    Write-Host "GET Messages Success: Retrieved $($res3.data.Count) messages."
}
catch {
    Write-Host "GET Messages Failed: $($_.Exception.Message)"
}
