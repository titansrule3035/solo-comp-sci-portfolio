# Make sure we are in the repo root
Set-Location -Path (git rev-parse --show-toplevel)

# Read each line in .gitignore
Get-Content .gitignore | ForEach-Object {
    $line = $_.Trim()
    if ($line -and -not $line.StartsWith("#")) {
        if (git ls-files --error-unmatch $line 2>$null) {
            Write-Host "Removing cached: $line"
            git rm -r --cached $line
        }
    }
}

Write-Host "Done! Commit changes with:"
Write-Host "git commit -m 'Remove ignored files from repo'"
