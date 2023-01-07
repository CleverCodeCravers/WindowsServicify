$counter = 1

while ($true)
{
    $counter += 1
    Write-Host "Hello worldalon Nr. $counter"
    Start-Sleep -Seconds 5

    if ($counter -eq 10) {
        throw "Boom!"
    }
}