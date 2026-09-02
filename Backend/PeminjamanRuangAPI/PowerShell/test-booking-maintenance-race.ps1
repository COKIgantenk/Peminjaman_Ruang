param(
    [Parameter(Mandatory = $true)]
    [string]$UserToken,

    [Parameter(Mandatory = $true)]
    [string]$AdminToken
)

$bookingUrl =
    "https://localhost:5074/api/Booking"

$maintenanceUrl =
    "https://localhost:5074/api/Maintenance"

$bookingJob = Start-Job -ScriptBlock {
    param($Url, $Token)

    $body = @{
        roomId = 4
        bookingDate = "2026-09-06"
        startTime = "10:00:00"
        endTime = "11:00:00"
        numPeople = 2
        title = "Cross Race Booking"
        requesterName = "User IT"
        requesterDivision = "IT"
        description = "Step 2.9 booking maintenance race"
    } | ConvertTo-Json

    $file =
        Join-Path $env:TEMP "booking-race-$([guid]::NewGuid()).json"

    try {
        Set-Content $file $body -Encoding utf8

        $result = & curl.exe `
            -k `
            -s `
            -w "`nHTTP_STATUS:%{http_code}" `
            -X POST `
            $Url `
            -H "Authorization: Bearer $Token" `
            -H "Content-Type: application/json" `
            --data-binary "@$file"

        $result -join "`n"
    }
    finally {
        if (Test-Path $file) {
            Remove-Item $file -Force
        }
    }
} -ArgumentList $bookingUrl, $UserToken

$maintenanceJob = Start-Job -ScriptBlock {
    param($Url, $Token)

    $body = @{
        roomId = 4
        maintenanceCategory = "GENERAL"
        priorityLevel = "MEDIUM"
        facilitiesServiced = $null
        documentation = $null
        description = "Step 2.9 booking maintenance race"
        startDate = "2026-09-06"
        endDate = "2026-09-06"
    } | ConvertTo-Json

    $file =
        Join-Path $env:TEMP "maintenance-race-$([guid]::NewGuid()).json"

    try {
        Set-Content $file $body -Encoding utf8

        $result = & curl.exe `
            -k `
            -s `
            -w "`nHTTP_STATUS:%{http_code}" `
            -X POST `
            $Url `
            -H "Authorization: Bearer $Token" `
            -H "Content-Type: application/json" `
            --data-binary "@$file"

        $result -join "`n"
    }
    finally {
        if (Test-Path $file) {
            Remove-Item $file -Force
        }
    }
} -ArgumentList $maintenanceUrl, $AdminToken

Wait-Job $bookingJob, $maintenanceJob | Out-Null

Write-Host "`n=== BOOKING ==="
Receive-Job $bookingJob

Write-Host "`n=== MAINTENANCE ==="
Receive-Job $maintenanceJob

Remove-Job $bookingJob, $maintenanceJob