$files = Get-Item "C:\Binaries\Bloodhound\*.nupkg"

foreach ($file in $files) {
	& "nuget.exe" push $file.FullName
}