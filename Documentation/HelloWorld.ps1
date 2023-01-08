Set-Location $PSScriptRoot

$counter = 1

while ($true)
{
    $counter += 1
    Write-Host "Hello worldalon Nr. $counter"

    "[$(Get-Date)]Hello worldalon Nr. $counter" | Add-Content log.txt

    Start-Sleep -Seconds 5

    if ($counter -eq 10) {
        throw "Boom!"
    }
}