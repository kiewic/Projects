# There are only three actions that can be taken:
# * Skip (do nothing)
# * Replicate (keep picture in both folders)
# * Delete (move picture to Recycle Bin)

$global:skipResult = [System.Windows.Forms.DialogResult]::Ignore;
$global:replicateResult = [System.Windows.Forms.DialogResult]::Yes;
$global:deleteResult = [System.Windows.Forms.DialogResult]::No;


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
    }

    Write-Host $leftFullName -ForegroundColor $foregroundColor

    if (!$leftResult -or !$rightResult)
    {
        $result = ShowForm $leftFullName $rightFullName
        ApplyAction $result $leftFullName $rightFullName
    }
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
    $flowLayoutPanel = New-Object System.Windows.Forms.FlowLayoutPanel
    $leftPictureBox = New-Object System.Windows.Forms.PictureBox
    $rightPictureBox = New-Object System.Windows.Forms.PictureBox
    $skipButton = New-Object System.Windows.Forms.Button
    $replicateButton = New-Object System.Windows.Forms.Button
    $deleteButton = New-Object System.Windows.Forms.Button

    $skipButton.Height = 40
    $replicateButton.Height = 40
    $deleteButton.Height = 40
    $skipButton.Text = "Skip"
    $replicateButton.Text = "Replicate"
    $deleteButton.Text = "Delete"

    $skipButton.Add_Click({ $form.DialogResult = $global:skipResult })
    $replicateButton.Add_Click({ $form.DialogResult = $global:replicateResult })
    $deleteButton.Add_Click({ $form.DialogResult = $global:deleteResult })

    $flowLayoutPanel.BackColor = [System.Drawing.Color]::Azure
    $splitContainer.BackColor = [System.Drawing.Color]::Azure

    $flowLayoutPanel.Anchor = [System.Windows.Forms.AnchorStyles]::Bottom.value__ + `
        [System.Windows.Forms.AnchorStyles]::Left.value__ + `
        [System.Windows.Forms.AnchorStyles]::Right
    $splitContainer.Anchor = [System.Windows.Forms.AnchorStyles]::Top.value__ + `
        [System.Windows.Forms.AnchorStyles]::Bottom.value__ + `
        [System.Windows.Forms.AnchorStyles]::Left + `
        [System.Windows.Forms.AnchorStyles]::Right

    $splitContainer.Size = New-Object System.Drawing.Size(284, 215)
    $flowLayoutPanel.Size = New-Object System.Drawing.Size(284, 47)
    $flowLayoutPanel.Location = New-Object System.Drawing.Point(0, 215)

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

    $flowLayoutPanel.Controls.Add($skipButton)
    $flowLayoutPanel.Controls.Add($replicateButton)
    $flowLayoutPanel.Controls.Add($deleteButton)

    $splitContainer.Panel1.Controls.Add($leftPictureBox)
    $splitContainer.Panel2.Controls.Add($rightPictureBox)

    $form.Controls.Add($flowLayoutPanel)
    $form.Controls.Add($splitContainer)

    return $form.ShowDialog()
}

Function ApplyAction([System.Windows.Forms.DialogResult]$result, [string]$leftFullName, [string]$rightFullName)
{
    switch ($result)
    {
        $global:replicateResult
        {
            Write-Host "Replicate" -ForegroundColor Green
        }
        $global:deleteResult
        {
            Write-Host "Delete" -ForegroundColor Red
        }
        default
        {
            Write-Host "Skip"
        }
    }
}

$directoryA = "C:\users\Gilberto\Desktop\FolderA"
$directoryB = "C:\users\Gilberto\Desktop\FolderB"
CompareDirs $directoryA $directoryB


 