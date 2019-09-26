param (  
  [string]$tagName = "",
  [string]$repoOwner = "",
  [string]$repoName = ""
)

$releaseNotes = "Release " + $tagName

try 
{
    #https://api.github.com/repos/nemesissoft/Nemesis.Essentials/releases/tags/v1.0.31
    $uri = "https://api.github.com/repos/" + $repoOwner + "/"+ $repoName + "/releases/tags/" + $tagName
    $webContent = Invoke-WebRequest -Uri $uri
    $json = $webContent | ConvertFrom-Json  | Select name, body

    if($json.name){
        $releaseNotes = $releaseNotes + ":`r`n" + $json.name
    }
    if($json.body){
        $releaseNotes = $releaseNotes + "`r`n" + $json.body
    }
    return $releaseNotes
}
catch {
    Write-Error "An error occurred during obtaining release notes"
    Write-Error $_
    return ""
}