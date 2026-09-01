$token6 = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI2IiwiZW1haWwiOiJhZG1pbi50ZXN0QGdtYWlsLmNvbSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWUiOiJBa3VuIEFkbWluIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiQURNSU4iLCJleHAiOjE3ODgyNDMxOTksImlzcyI6IlBlbWluamFtYW5SdWFuZ0FQSSIsImF1ZCI6IlBlbWluamFtYW5SdWFuZ0NsaWVudCJ9.jFudAQJE7ddhxDyaYVuCe2su3GFmdcXTmaQaQob3BAY"
$token22 = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIyMiIsImVtYWlsIjoiYWRtaW4uaHR0cHNAZ21haWwuY29tIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6IkFkbWluIEh0dHBzIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiQURNSU4iLCJleHAiOjE3ODgyNDMyMTYsImlzcyI6IlBlbWluamFtYW5SdWFuZ0FQSSIsImF1ZCI6IlBlbWluamFtYW5SdWFuZ0NsaWVudCJ9.4i5ubNDckuPc9EH6egYWMqBGd1fJmg6Y0NedBmqdfis"

$projectPath = $PSScriptRoot

$body22Path = Join-Path $projectPath "body22.json"
$body6Path  = Join-Path $projectPath "body6.json"

$body22 = @{
    fullName     = "Admin Https"
    phoneNumber  = "081234567890"
    departmentId = 2
    role         = "USER"
    isActive     = $true
} | ConvertTo-Json

$body6 = @{
    fullName     = "Akun Admin"
    phoneNumber  = "081234567890"
    departmentId = 1
    role         = "USER"
    isActive     = $true
} | ConvertTo-Json

Set-Content -Path $body22Path -Value $body22 -Encoding UTF8
Set-Content -Path $body6Path -Value $body6 -Encoding UTF8

Write-Host "Menjalankan dua request secara bersamaan..."

$job1 = Start-Job -ScriptBlock {
    param($token, $bodyPath)

    curl.exe -k -s `
        -X PUT `
        "https://localhost:5074/api/User/22" `
        -H "Authorization: Bearer $token" `
        -H "Content-Type: application/json" `
        --data-binary "@$bodyPath" `
        -w "`nHTTP_STATUS:%{http_code}"
} -ArgumentList $token6, $body22Path

$job2 = Start-Job -ScriptBlock {
    param($token, $bodyPath)

    curl.exe -k -s `
        -X PUT `
        "https://localhost:5074/api/User/6" `
        -H "Authorization: Bearer $token" `
        -H "Content-Type: application/json" `
        --data-binary "@$bodyPath" `
        -w "`nHTTP_STATUS:%{http_code}"
} -ArgumentList $token22, $body6Path

Wait-Job $job1, $job2 | Out-Null

Write-Host ""
Write-Host "=== Admin 6 -> demote Admin 22 ==="
Receive-Job $job1

Write-Host ""
Write-Host "=== Admin 22 -> demote Admin 6 ==="
Receive-Job $job2

Remove-Job $job1, $job2

Remove-Item $body22Path -ErrorAction SilentlyContinue
Remove-Item $body6Path -ErrorAction SilentlyContinue