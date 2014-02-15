# There are only three actions that can be taken:
# * Skip (do nothing)
# * Replicate (keep picture in both folders)
# * Delete (move picture to Recycle Bin)



Function CompareDirs([string]$leftDir, [string]$rightDir)
{
    CompareDirsCore $leftDir $rightDir $false
    CompareDirsCore $rightDir $leftDir $true
}

Function CompareDirsCore([string]$leftDir, [string]$rightDir, [bool]$inverted)
{
    $files = Get-ChildItem -File -Recurse $leftDir 
    foreach ($file in $files)
    {
        $leftResult = $true
        $rightResult = $false

        $leftFullName = $file.FullName
        $rightFullName = $null
        if ($leftFullName.Contains($leftDir))
        {
            $relativeName = $leftFullName.Substring($leftDir.Length)
            $rightFullName = $rightDir + $relativeName;

            #Write-Host $rightFullName

            $rightResult = Test-Path($rightFullName);
        }

        if (!$inverted)
        {
            ShowResults $leftResult $rightResult $leftFullName $rightFullName
        }
        elseif ($inverted -and $leftResult -and $rightResult)
        {
            # Do nothing. A "Y Y" result for this files was already reported
            # when comparing files left to right.
        }
        else
        {
            ShowResults $rightResult $leftResult $rightFullName $leftFullName
        }
    }
}


Function ShowResults([bool]$leftResult, [bool]$rightResult, [string]$leftFullName, [string]$rightFullName)
{
    PrintYOrN($leftResult);
    PrintYOrN($rightResult);

    $foregroundColor = "White";
    if (!$leftResult -or !$rightResult)
    {
        $foregroundColor = "Yellow";
        ShowForm $leftFullName $rightFullName
    }

    Write-Host $leftFullName -ForegroundColor $foregroundColor
}

Function PrintYOrN($value)
{
    if ($value)
    {
        Write-Host -NoNewline "Y " -ForegroundColor Green
    }
    else {
        Write-Host -NoNewline "N " -ForegroundColor Yellow
    }
}

Function ShowForm([string]$leftFullName, [string]$rightFullName)
{
    $form = New-Object System.Windows.Forms.Form
    $splitContainer = New-Object System.Windows.Forms.SplitContainer
    $leftPictureBox = New-Object System.Windows.Forms.PictureBox
    $rightPictureBox = New-Object System.Windows.Forms.PictureBox

    $splitContainer.Dock = [System.Windows.Forms.DockStyle]::Fill
    $leftPictureBox.Dock = [System.Windows.Forms.DockStyle]::Fill
    $rightPictureBox.Dock = [System.Windows.Forms.DockStyle]::Fill

    $leftPictureBox.SizeMode = [System.Windows.Forms.PictureBoxSizeMode]::Zoom
    $rightPictureBox.SizeMode = [System.Windows.Forms.PictureBoxSizeMode]::Zoom


    if (Test-Path($leftFullName))
    {
        $leftPictureBox.Image = [System.Drawing.Image]::FromFile($leftFullName)
    }
    if (Test-Path($rightFullName))
    {
        $rightPictureBox.Image = [System.Drawing.Image]::FromFile($rightFullName)
    }

    $splitContainer.Panel1.Controls.Add($leftPictureBox)
    $splitContainer.Panel2.Controls.Add($rightPictureBox)

    $form.Controls.Add($splitContainer)

    [void]$form.ShowDialog()
}

$directoryA = "C:\users\Gilberto\Desktop\FolderA"
$directoryB = "C:\users\Gilberto\Desktop\FolderB"
CompareDirs $directoryA $directoryB


 