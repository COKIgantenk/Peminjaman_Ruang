param(
    [Parameter(Mandatory = $true)]
    [string]$Token,

    [int]$RoomId = 4,

    [string]$BookingDate = "2026-09-10",

    [string]$StartTime = "14:00:00",

    [string]$EndTime = "15:00:00"
)

$uri = "https://localhost:5074/api/Booking"

$jobScript = {
    param(
        $RequestName,
        $Uri,
        $Token,
        $RoomId,
        $BookingDate,
        $StartTime,
        $EndTime
    )

    $body = @{
        roomId = $RoomId
        bookingDate = $BookingDate
        startTime = $StartTime
        endTime = $EndTime
        numPeople = 2
        title = "Race Create $RequestName"
        requesterName = "User IT"
        requesterDivision = "IT"
        description = "Step 2.9 concurrent create test"
    } | ConvertTo-Json

    $tempFile =
        Join-Path $env:TEMP "booking-create-$RequestName-$([guid]::NewGuid()).json"

    try {
        Set-Content `
            -Path $tempFile `
            -Value $body `
            -Encoding utf8

        $result = & curl.exe `
            -k `
            -s `
            -w "`nHTTP_STATUS:%{http_code}" `
            -X POST `
            $Uri `
            -H "Authorization: Bearer $Token" `
            -H "Content-Type: application/json" `
            --data-binary "@$tempFile"

        $resultText = $result -join "`n"

        $statusMatch =
            [regex]::Match(
                $resultText,
                "HTTP_STATUS:(\d{3})"
            )

        $statusCode =
            if ($statusMatch.Success) {
                [int]$statusMatch.Groups[1].Value
            }
            else {
                0
            }

        $responseBody =
            $resultText `
                -replace "`nHTTP_STATUS:\d{3}$", ""

        [PSCustomObject]@{
            Request    = $RequestName
            StatusCode = $statusCode
            Body       = $responseBody
        }
    }
    finally {
        if (Test-Path $tempFile) {
            Remove-Item $tempFile -Force
        }
    }
}

$jobA = Start-Job `
    -ScriptBlock $jobScript `
    -ArgumentList `
    "A", $uri, $Token, `
    $RoomId, $BookingDate, $StartTime, $EndTime

$jobB = Start-Job `
    -ScriptBlock $jobScript `
    -ArgumentList `
    "B", $uri, $Token, `
    $RoomId, $BookingDate, $StartTime, $EndTime

Wait-Job $jobA, $jobB | Out-Null

Receive-Job $jobA
Receive-Job $jobB

Remove-Job $jobA, $jobB