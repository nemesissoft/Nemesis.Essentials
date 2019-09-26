param (
  [string]$basePath = "",
  [string]$releaseNotes = ""
)

function Update-Package([string]$packagesFileName, [string] $newReleaseNotes) 
{
    $nuspecTemplate = [xml](cat $packagesFileName)
    $metadata = $nuspecTemplate.package.metadata
    
    if($metadata.releaseNotes) {
        $metadata.releaseNotes = "$newReleaseNotes"
    }
    else {
        $newNode = $metadata.OwnerDocument.CreateElement("releaseNotes")
        $newNode.InnerText = "$newReleaseNotes"
        $metadata = $metadata.AppendChild($newNode)
    }        

    $nuspecTemplate.Save((get-item $packagesFileName))
}

Write-Output ("Attempt to change release notes to: " + $releaseNotes)
$files = Get-ChildItem -Path "$basePath" -Filter *.nuspec -Recurse

if ($files.Count -ne 0)
{
    foreach($file in $files)
    {
        Write-Output ("PATCHING " + $file.FullName)
        
        Update-Package -packagesFileName $file.FullName -newReleaseNotes "$releaseNotes"
    }            
}