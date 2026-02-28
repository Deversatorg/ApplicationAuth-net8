$ErrorActionPreference = "Stop"

$baseUrl = "http://localhost:1310/api/v1/sessions"

Write-Host "--- TEST: Google Login with Fake Token ---"
$googleBody = @{
    idToken = "fake.jwt.token.123"
} | ConvertTo-Json

try {
    $res = Invoke-RestMethod -Uri "$baseUrl/google" -Method Post -Body $googleBody -ContentType "application/json"
    Write-Host "Google Auth succeeded unexpectedly."
}
catch {
    Write-Host "Google Auth Failed Successfully! Expected behavior for invalid token."
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        Write-Host "Response Body: $($reader.ReadToEnd())"
    }
}

Write-Host "`n--- TEST: Apple Login with Fake Token ---"
$appleBody = @{
    identityToken = "fake.jwt.token.123"
    givenName     = "Test"
    familyName    = "User"
} | ConvertTo-Json

try {
    $res = Invoke-RestMethod -Uri "$baseUrl/apple" -Method Post -Body $appleBody -ContentType "application/json"
    Write-Host "Apple Auth succeeded unexpectedly."
}
catch {
    Write-Host "Apple Auth Failed Successfully! Expected behavior for invalid token."
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        Write-Host "Response Body: $($reader.ReadToEnd())"
    }
}
