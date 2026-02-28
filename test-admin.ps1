$ErrorActionPreference = "Stop"

$baseUrl = "http://localhost:1310/api/v1"

Write-Host "--- TEST: Login as Admin ---"
$loginBody = @{
    email    = "admin@test.com"
    password = "Welcome1!"
} | ConvertTo-Json
try {
    $loginResponse = Invoke-RestMethod -Uri "$baseUrl/admin-sessions" -Method Post -Body $loginBody -ContentType "application/json"
    $script:accessToken = $loginResponse.data.token.accessToken
    Write-Host "Login Success."
}
catch {
    Write-Host "Login Failed: $($_.Exception.Message)"
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        Write-Host "Response Body: $($reader.ReadToEnd())"
    }
}

Write-Host "`n--- TEST: Get All Users ---"
try {
    $headers = @{ Authorization = "Bearer $($script:accessToken)" }
    $usersResponse = Invoke-RestMethod -Uri "$baseUrl/admin-users" -Method Get -Headers $headers
    Write-Host "GetAllUsers Success: Retrieved $($usersResponse.data.Count) users."
}
catch {
    Write-Host "GetAllUsers Failed: $($_.Exception.Message)"
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        Write-Host "Response Body: $($reader.ReadToEnd())"
    }
}
