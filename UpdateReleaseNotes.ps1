param (
  [string]$basePath = "",
  [string]$releaseNotes = ""
)

#<PackageReleaseNotes>RELEASE_NOTES_PLACEHOLDER<!--starts with "RELEASE_NOTES_PLACEHOLDER" == this will be patched by AppVeyor--></PackageReleaseNotes>
function Update-ReleaseNotes([string]$csProjFileName, [string] $newReleaseNotes) 
{
    $csProjDoc = [xml](Get-Content $csProjFileName)
    $releaseNotesNode = $csProjDoc.SelectSingleNode("Project/PropertyGroup/PackageReleaseNotes")
        
    if($releaseNotesNode.InnerText -clike "RELEASE_NOTES_PLACEHOLDER*") 
    {
        Write-Output ("PATCHING '" + $csProjFileName + "' with '" + $newReleaseNotes + "'")
        $releaseNotesNode.InnerText = "$newReleaseNotes"
        $csProjDoc.Save($csProjFileName)
    }    
}

try {
    $files = Get-ChildItem -Path "$basePath" -Filter *.csproj -Recurse
    if ($files.Count -ne 0)
    {
        foreach($file in $files)
        {   
            Update-ReleaseNotes -csProjFileName $file.FullName -newReleaseNotes "$releaseNotes"
        }            
    }
}
catch {
    Write-Error "An error occurred during patching files"
    Write-Error $_
    Exit
}