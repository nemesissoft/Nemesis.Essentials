<#
    .SYNOPSIS
      Obtain relese notes for given tag in repository from Github API
    .OUTPUTS
      Tuple containing short and long release description
    .EXAMPLE
     $result = .\GetGithubReleaseNotes.ps1 -tagName "v1.0.38" -repoOwner "nemesissoft" -repoName "Nemesis.Essentials"
     $shortDesc = $result.Item1
     $longDesc = $result.Item2
#>

param (  
  [Parameter(Mandatory=$True)] [Alias("tn")] [string]$tagName,
  [Parameter(Mandatory=$True)] [Alias("ro")] [string]$repoOwner,
  [Parameter(Mandatory=$True)] [Alias("rn")] [string]$repoName
)

try 
{
    $uri = "https://api.github.com/repos/" + $repoOwner + "/"+ $repoName + "/releases/tags/" + $tagName
    $webContent = Invoke-WebRequest -Uri $uri

    $json = $webContent.Content | ConvertFrom-Json  | Select name, body

    $shortDesc = "Release " + $tagName
    $longDesc  = "Release " + $tagName

    if($json.name){
        $shortDesc = $shortDesc + " - "  + $json.name
        $longDesc  = $longDesc  + "`r`n" + $json.name
    }
    if($json.body){
        $longDesc  = $longDesc  + "`r`n" + $json.body
    }
    return [System.Tuple]::Create($shortDesc, $longDesc)
}
catch {
    Write-Error "An error occurred during obtaining release notes"
    Write-Error $_
    return null
}