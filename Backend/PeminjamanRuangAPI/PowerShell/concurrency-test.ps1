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

    $tempFile =
        Join-Path $env:TEMP `
            "admin-demote-$TargetAdminId-$([guid]::NewGuid()).json"

    try {
        # Ambil data target agar field profile tetap sama.
        $targetJson = & curl.exe `
            -k `
            -s `
            "$BaseUrl/$TargetAdminId" `
            -H "Authorization: Bearer $ActorToken"

        $target =
            ($targetJson -join "`n") |
                ConvertFrom-Json

        $body = @{
            fullName = $target.fullName
            phoneNumber = $target.phoneNumber
            departmentId = $target.departmentId
            role = "USER"
            isActive = $true
        } | ConvertTo-Json

        Set-Content `
            -Path $tempFile `
            -Value $body `
            -Encoding UTF8

        $result = & curl.exe `
            -k `
            -s `
            -w "`nHTTP_STATUS:%{http_code}" `
            -X PUT `
            "$BaseUrl/$TargetAdminId" `
            -H "Authorization: Bearer $ActorToken" `
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
    finally {
        if (Test-Path $tempFile) {
            Remove-Item $tempFile -Force
        }
    }
}

Write-Host "Menjalankan concurrent admin demotion..."

$jobA = Start-Job `
    -ScriptBlock $jobScript `
    -ArgumentList `
        $TokenA,
        $AdminIdB,
        $baseUrl,
        "Admin $AdminIdA -> demote Admin $AdminIdB"

$jobB = Start-Job `
    -ScriptBlock $jobScript `
    -ArgumentList `
        $TokenB,
        $AdminIdA,
        $baseUrl,
        "Admin $AdminIdB -> demote Admin $AdminIdA"

Wait-Job $jobA, $jobB | Out-Null

Receive-Job $jobA
Receive-Job $jobB

Remove-Job $jobA, $jobB