param(
    [Parameter(Mandatory = $true)]
    [int]$AdminIdA,

    [Parameter(Mandatory = $true)]
    [string]$TokenA,

    [Parameter(Mandatory = $true)]
    [int]$AdminIdB,

    [Parameter(Mandatory = $true)]
    [string]$TokenB
)

$baseUrl = "https://localhost:5074/api/User"

$jobScript = {
    param(
        $ActorToken,
        $TargetAdminId,
        $BaseUrl,
        $RequestName
    )

    try {
        $result = & curl.exe `
            -k `
            -s `
            -w "`nHTTP_STATUS:%{http_code}" `
            -X DELETE `
            "$BaseUrl/$TargetAdminId" `
            -H "Authorization: Bearer $ActorToken"

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
            Request = $RequestName
            TargetAdminId = $TargetAdminId
            StatusCode = $statusCode
            Body = $responseBody
        }
    }
    catch {
        [PSCustomObject]@{
            Request = $RequestName
            TargetAdminId = $TargetAdminId
            StatusCode = 0
            Body = $_.Exception.Message
        }
    }
}

Write-Host "Menjalankan concurrent admin delete..."

$jobA = Start-Job `
    -ScriptBlock $jobScript `
    -ArgumentList `
        $TokenA,
        $AdminIdB,
        $baseUrl,
        "Admin $AdminIdA -> delete Admin $AdminIdB"

$jobB = Start-Job `
    -ScriptBlock $jobScript `
    -ArgumentList `
        $TokenB,
        $AdminIdA,
        $baseUrl,
        "Admin $AdminIdB -> delete Admin $AdminIdA"

Wait-Job $jobA, $jobB | Out-Null

Receive-Job $jobA
Receive-Job $jobB

Remove-Job $jobA, $jobB