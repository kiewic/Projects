$jsonArray = [IO.File]::ReadAllText(".\\Desktop\\logs.json") | ConvertFrom-Json
$jsonArray.length
$counter1 = 0
$counter2 = 0
$siufConter = 0
$likeConter = 0
foreach ($event in $jsonArray)
{
    if ($event.name -like "*WaasPing*")
    {
        Write-Host $event.name -ForegroundColor red
    }
    else
    {
        $dependencyName = $event.data.baseData.dependencyName
        $targetUri = $event.data.baseData.targetUri
        $eventString = ($event | ConvertTo-Json)

        #Write-Host $event.name -ForegroundColor green
        if ($dependencyName -ne $null -and $dependencyName -like 'http*' -and $dependencyName -like '*2322035*')
        {
            Write-Output "(1) $($event.time) $dependencyName"
            Write-Output ($event | ConvertTo-Json)
            $counter1++
        }
        elseif ($targetUri -ne $null -and $targetUri -like 'http*' -and $targetUri -like '*2322035*')
        {
            Write-Output "(2) $($event.time) $targetUri"
            Write-Output ($event | ConvertTo-Json)
            $counter2++
        }
        elseif ($eventString -like '*onesettingsprod*')
        {
            $siufConter++
        }
        elseif ($eventString -like '*https:*')
        {
            #Write-Output $eventString
            #Write-Host ($event | ConvertTo-Json)
            #$innerException = $event.data.innerExceptionTree | ConvertFrom-Json
            #Write-Host $innerException  -ForegroundColor cyan
            $likeConter++
        }
        else
        {
        }
    }
}
$counter1
$counter2
$siufConter
$likeConter


