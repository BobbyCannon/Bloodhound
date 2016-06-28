param (
    [Parameter()]
    [switch] $RandomOrder = $false,
    [Parameter()]
    [string] $Filter,
    [Parameter()]
    [switch] $Loop,
    [Parameter()]
    [switch] $ListTests,
    [Parameter()]
    [string] $Configuration = "Release"
)

$scriptPath = Split-Path (Get-Variable MyInvocation).Value.MyCommand.Path 

if (Test-Path "$scriptPath\Bloodhound.IntegrationTests\bin\$Configuration\Bloodhound.IntegrationTests.dll" -PathType Leaf) {
    Import-Module $scriptPath\Bloodhound.IntegrationTests\bin\$Configuration\Bloodhound.IntegrationTests.dll
}

function RunLoop() {
    param (
        [Parameter(ValueFromPipeline = $true, Position = 1)]
        [System.Int32] $Count
    )

    $watch = [System.Diagnostics.Stopwatch]::StartNew()
    $tests = Get-Command -Module Bloodhound.IntegrationTests
    $testCount = 0;
    
    if ($RandomOrder) {
        $tests = $tests | Get-Random -Count $tests.Count
    }

    foreach ($test in $tests) 
    {
        try
        {
            $testNames = Invoke-Expression "$test"

            if ($Filter.Length -gt 0) {
                $testNames = $testNames | where { $_.Contains($Filter) }
            }

            if ($RandomOrder) {
                $testNames = $testNames | Get-Random -Count $testNames.Count
            }

            foreach ($testName in $testNames) {
                Write-Host "$test.$testName ..." -NoNewline
                $testCount++
                
                if ($ListTests) {
                    Write-Host " "
                } else {
                    & "$test" -Name $testName -Verbose:$PSBoundParameters.ContainsKey("Verbose")
                    Write-Host " Passed" -ForegroundColor Green
                }
            }
        }
        catch [System.Exception]
        {
            Write-Host
            Write-Host " Failed:" $_.Exception.Message -ForegroundColor Red
            Write-Host " @ " $_.Exception.StackTrace -ForegroundColor Red

            if ($_.Exception.InnerException) {
                Write-Host " Failed:" $_.Exception.InnerException.Message -ForegroundColor Red
                Write-Host " @ " $_.Exception.InnerException.StackTrace -ForegroundColor Red
            }

            exit -1
        }
    }

    Write-Host "Loop $Count ($testCount) completed in" $watch.Elapsed -ForegroundColor Yellow
}

$count = 1

RunLoop $count

while ($Loop) {
    $count += 1
    RunLoop $count
}

exit $LASTEXITCODE