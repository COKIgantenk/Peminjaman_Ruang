param(
    [Parameter(Mandatory = $true)]
    [int]$BookingIdA,

    [Parameter(Mandatory = $true)]
    [int]$BookingIdB,

    [Parameter(Mandatory = $true)]
    [string]$Token
)

$baseUrl = "https://localhost:5074/api/Booking"

$jobScript = {
    param(
        $RequestName,
        $BookingId,
        $BaseUrl,
        $Token
    )

    try {
        $uri = "$BaseUrl/$BookingId/approve"

        $result = & curl.exe `
            -k `
            -s `
            -w "`nHTTP_STATUS:%{http_code}" `
            -X PUT `
            $uri `
            -H "Authorization: Bearer $Token"

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
            BookingId  = $BookingId
            StatusCode = $statusCode
            Body       = $responseBody
        }
    }
    catch {
        [PSCustomObject]@{
            Request    = $RequestName
            BookingId  = $BookingId
            StatusCode = 0
            Body       = $_.Exception.Message
        }
    }
}

$jobA = Start-Job `
    -ScriptBlock $jobScript `
    -ArgumentList "A", $BookingIdA, $baseUrl, $Token

$jobB = Start-Job `
    -ScriptBlock $jobScript `
    -ArgumentList "B", $BookingIdB, $baseUrl, $Token

Wait-Job $jobA, $jobB | Out-Null

Receive-Job $jobA
Receive-Job $jobB

Remove-Job $jobA, $jobB