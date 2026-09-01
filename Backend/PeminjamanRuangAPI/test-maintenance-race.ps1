param(
    [Parameter(Mandatory = $true)]
    [int]$RoomId,

    [Parameter(Mandatory = $true)]
    [string]$Token
)

$uri = "https://localhost:5074/api/Maintenance"

$body = @{
    roomId = $RoomId
    maintenanceCategory = "Race Test"
    priorityLevel = "MEDIUM"
    facilitiesServiced = $null
    documentation = "Step 2.9 concurrent maintenance test"
    description = "Step 2.9 concurrent maintenance test"
    startDate = "2026-09-02"
    endDate = "2026-09-03"
} | ConvertTo-Json -Compress

$jobScript = {
    param(
        $RequestName,
        $Uri,
        $Token,
        $Body
    )

    $tempFile = Join-Path $env:TEMP "maintenance-race-$RequestName-$([guid]::NewGuid()).json"

    try {
        Set-Content `
            -Path $tempFile `
            -Value $Body `
            -Encoding UTF8

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
    catch {
        [PSCustomObject]@{
            Request    = $RequestName
            StatusCode = 0
            Body       = $_.Exception.Message
        }
    }
    finally {
        if (Test-Path $tempFile) {
            Remove-Item $tempFile -Force
        }
    }
}

$job1 = Start-Job `
    -ScriptBlock $jobScript `
    -ArgumentList "A", $uri, $Token, $body

$job2 = Start-Job `
    -ScriptBlock $jobScript `
    -ArgumentList "B", $uri, $Token, $body

Wait-Job $job1, $job2 | Out-Null

Receive-Job $job1
Receive-Job $job2

Remove-Job $job1, $job2