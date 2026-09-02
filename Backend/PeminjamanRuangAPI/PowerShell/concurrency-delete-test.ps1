$token6 = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI2IiwiZW1haWwiOiJhZG1pbi50ZXN0QGdtYWlsLmNvbSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWUiOiJBa3VuIEFkbWluIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiQURNSU4iLCJleHAiOjE3ODgyNDM1OTAsImlzcyI6IlBlbWluamFtYW5SdWFuZ0FQSSIsImF1ZCI6IlBlbWluamFtYW5SdWFuZ0NsaWVudCJ9.DFcsep2keX8qOHil8okAF0QWIm_YrN7pOHjR36hhN6Q"
$token22 = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIyMiIsImVtYWlsIjoiYWRtaW4uaHR0cHNAZ21haWwuY29tIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6IkFkbWluIEh0dHBzIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiQURNSU4iLCJleHAiOjE3ODgyNDM1NjksImlzcyI6IlBlbWluamFtYW5SdWFuZ0FQSSIsImF1ZCI6IlBlbWluamFtYW5SdWFuZ0NsaWVudCJ9.HF-iKfv1E4UeFbuvysLPBivySz2XM8X1tBO-GTm81qM"

Write-Host "Menjalankan dua DELETE request secara bersamaan..."

$job1 = Start-Job -ScriptBlock {
    param($token)

    curl.exe -k -s `
        -X DELETE `
        "https://localhost:5074/api/User/22" `
        -H "Authorization: Bearer $token" `
        -w "`nHTTP_STATUS:%{http_code}"
} -ArgumentList $token6

$job2 = Start-Job -ScriptBlock {
    param($token)

    curl.exe -k -s `
        -X DELETE `
        "https://localhost:5074/api/User/6" `
        -H "Authorization: Bearer $token" `
        -w "`nHTTP_STATUS:%{http_code}"
} -ArgumentList $token22

Wait-Job $job1, $job2 | Out-Null

Write-Host ""
Write-Host "=== Admin 6 -> DELETE Admin 22 ==="
Receive-Job $job1

Write-Host ""
Write-Host "=== Admin 22 -> DELETE Admin 6 ==="
Receive-Job $job2

Remove-Job $job1, $job2