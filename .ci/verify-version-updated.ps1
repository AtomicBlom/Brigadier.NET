$fileToCheck = '.ci/azure-pipelines.yml'

$head=$(git rev-parse HEAD)
$parents=$(git rev-list --parents -n 1 $head)
$p1,$p2,$p3=$parents.split(' ')
If ($p1 = $head)
{
  $parent1=$p2
  $parent2=$p3
}
ElseIf ($p2 = $head)
{
  $parent1=$p1
  $parent2=$p3
}
Else
{
  $parent1=$p1
  $parent2=$p2
}
$outp1=$(git diff $head $parent1 --name-only)
$outp2=$(git diff $head $parent2 --name-only)

If ((-not ($outp1 -contains $fileToCheck)) -and (-not ($outp2 -contains $fileToCheck)))
{
  Write-Host  "$("##vso[task.logissue type=error;]") $("Pipelines has not updated $fileToCheck")"
  exit 1
}
