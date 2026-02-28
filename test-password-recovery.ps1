$ErrorActionPreference = "Stop"

$baseUrl = "http://localhost:1310/api/v1/sessions"
$testEmail = "admin@admin.com"

Write-Host "--- TEST: Forgot Password ---"
$forgotBody = @{
    email = $testEmail
} | ConvertTo-Json

try {
    $res = Invoke-RestMethod -Uri "$baseUrl/forgot-password" -Method Post -Body $forgotBody -ContentType "application/json"
    Write-Host "Forgot Password call succeeded! Response: $($res.Content)"
}
catch {
    Write-Host "Forgot Password Failed!"
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        Write-Host "Response Body: $($reader.ReadToEnd())"
    }
}

Write-Host "`n--- TEST: Reset Password (with invalid fake code) ---"
$resetBody = @{
    email       = $testEmail
    code        = "123456"
    newPassword = "NewValidPassword123!"
} | ConvertTo-Json

try {
    $res = Invoke-RestMethod -Uri "$baseUrl/reset-password" -Method Post -Body $resetBody -ContentType "application/json"
    Write-Host "Reset Password succeeded unexpectedly."
}
catch {
    Write-Host "Reset Password Failed Successfully! Expected behavior for fake code."
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        Write-Host "Response Body: $($reader.ReadToEnd())"
    }
}
