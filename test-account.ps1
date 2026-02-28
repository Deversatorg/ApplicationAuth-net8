$ErrorActionPreference = "Stop"

$baseUrl = "http://localhost:1310/api/v1"

Write-Host "--- TEST: Register User ---"
$regBody = @{
    email           = "newuser@test.com"
    password        = "Welcome1!"
    confirmPassword = "Welcome1!"
} | ConvertTo-Json
try {
    $regResponse = Invoke-RestMethod -Uri "$baseUrl/users" -Method Post -Body $regBody -ContentType "application/json"
    Write-Host "Register Success:"
    $regResponse | ConvertTo-Json -Depth 5
}
catch {
    Write-Host "Register Failed: $($_.Exception.Message)"
    if ($_.ErrorDetails) { Write-Host $_.ErrorDetails.Message }
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        Write-Host "Response Body: $($reader.ReadToEnd())"
    }
}

Write-Host "`n--- TEST: Login User ---"
$loginBody = @{
    email    = "newuser@test.com"
    password = "Welcome1!"
} | ConvertTo-Json
try {
    $loginResponse = Invoke-RestMethod -Uri "$baseUrl/sessions" -Method Post -Body $loginBody -ContentType "application/json"
    $script:accessToken = $loginResponse.content.accessToken
    $script:refreshToken = $loginResponse.content.refreshToken
    Write-Host "Login Success. AccessToken retrieved."
}
catch {
    Write-Host "Login Failed: $($_.Exception.Message)"
    if ($_.ErrorDetails) { Write-Host $_.ErrorDetails.Message }
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        Write-Host "Response Body: $($reader.ReadToEnd())"
    }
}
