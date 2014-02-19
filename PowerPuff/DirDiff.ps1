# There are only three actions that can be taken:
# * Skip (do nothing)
# * Replicate (keep picture in both folders)
# * Delete (move picture to Recycle Bin)

param (
    [string]$directoryA = "C:\users\Gilberto\Desktop\FolderA",
    [string]$directoryB = "C:\users\Gilberto\Desktop\FolderB"
)

# Make sure to load System.Windows.Forms.dll assembly.
Add-Type -AssemblyName System.Windows.Forms

$global:skipResult = [System.Windows.Forms.DialogResult]::Cancel;
$global:replicateResult = [System.Windows.Forms.DialogResult]::Yes;
$global:deleteResult = [System.Windows.Forms.DialogResult]::No;


Function CompareDirs([string]$leftDir, [string]$rightDir)
{
    CompareDirsCore $leftDir $rightDir $false
    CompareDirsCore $rightDir $leftDir $true
}

Function CompareDirsCore([string]$leftDir, [string]$rightDir, [bool]$inverted)
{
    $files = Get-ChildItem -Path $leftDir
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

            $rightResult = Test-Path($rightFullName);
        }

        if ($leftResult -and $rightResult)
        {
            if (!$inverted)
            {
                ShowDiff $leftResult $rightResult $leftFullName $rightFullName
            }
            else
            {
                # Do nothing. A "Y Y" result for this files was already reported
                # when comparing files left to right.
            }
        }
        else
        {
            if (!$inverted)
            {
                $result = ShowDiff $leftResult $rightResult $leftFullName $rightFullName
            }
            else
            {
                $result = ShowDiff $rightResult $leftResult $rightFullName $leftFullName
            }
            ApplyFileAction $result $leftFullName $rightFullName
        }
    }
}


Function ShowDiff([bool]$leftResult, [bool]$rightResult, [string]$leftFullName, [string]$rightFullName)
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
        return ShowForm $leftFullName $rightFullName
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
    $splitContainer.SplitterDistance = $splitContainer.Width / 2

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


    Try{
        if (Test-Path($leftFullName))
        {
            $form.Text = $leftFullName
            $leftPictureBox.Image = [System.Drawing.Image]::FromFile($leftFullName)
        }
        if (Test-Path($rightFullName))
        {
            $form.Text = $rightFullName
            $rightPictureBox.Image = [System.Drawing.Image]::FromFile($rightFullName)
        }
    }
    Catch [System.OutOfMemoryException]
    {
        Write-Host "Wrong image format."
    }
    Catch [System.IO.FileNotFoundException]
    {
        Write-Host "Maybe a directory."
    }

    $flowLayoutPanel.Controls.Add($skipButton)
    $flowLayoutPanel.Controls.Add($replicateButton)
    $flowLayoutPanel.Controls.Add($deleteButton)

    $splitContainer.Panel1.Controls.Add($leftPictureBox)
    $splitContainer.Panel2.Controls.Add($rightPictureBox)

    $form.WindowState = [System.Windows.Forms.FormWindowState]::Maximized
    $form.Controls.Add($flowLayoutPanel)
    $form.Controls.Add($splitContainer)

    $result = $form.ShowDialog()

    # Release files, so we can delete them if that is the case.
    if ($leftPictureBox.Image -ne $null)
    {
        $leftPictureBox.Image.Dispose()
    }
    if ($rightPictureBox.Image -ne $null)
    {
        $rightPictureBox.Image.Dispose()
    }
    $form.Dispose()

    return $result
}

Function ApplyFileAction([System.Windows.Forms.DialogResult]$result, [string]$leftFullName, [string]$rightFullName)
{
    switch ($result)
    {
        $global:replicateResult
        {
            Write-Host "Replicate" -ForegroundColor Green
            Copy-Item $leftFullName $rightFullName
        }
        $global:deleteResult
        {
            Write-Host "Delete" -ForegroundColor Red
            MoveFileToRecycleBin $leftFullName
        }
        default
        {
            Write-Host "Skip"
        }
    }
}

function MoveFileToRecycleBin([string]$fileFullName) 
{
    $file = New-Object System.IO.FileInfo $fileFullName
    $shell = New-Object -comobject "Shell.Application"
    $folder = $shell.Namespace($file.DirectoryName)
    $item = $folder.ParseName($file.Name)

    $item.InvokeVerb("delete")
}

CompareDirs $directoryA $directoryB


 