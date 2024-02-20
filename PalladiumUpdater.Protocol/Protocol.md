# Local data storage

Each app gets an identifier, say "Palladium". Apps are installed to `%LOCALAPPDATA%/{Identifier}/`. The entire app archive is extracted into that folder. An additional file, `Version.txt`, describes which version the app is.

# Updating process

## Version check

The local version is checked at `%LOCALAPPDATA%/{Identifier}/Version.txt`. The remote version is checked by downloading a file at a given URL. The format of both versions is validated, and then an exact string match is performed to check if the version is the same or not.

## Downloading

The archive file URL is given. The archive file is downloaded to `%TEMP%/{Identifier}/pending-archive-download`. Once the download process has finished, it is renamed to `%TEMP%/{Identifier}/archive-{version}` (`archive-v1.0.0` for example). 

## Updating

Once the user has agreed to the update, the archive is decompressed to `%TEMP%/{Identifier}/decompressed/`, and the version file is created at `%TEMP%/{Identifier}/decompressed/Version.txt`. 

For an app updating itself, the copy process is performed by `PalladiumUpdater.exe` which is copied to `%TEMP%/`. This updater will be invoked: `--pid={running-app-pid} --copyfrom="%TEMP%/{Identifier}/decompressed/" --copyto="%LOCALAPPDATA%/{Identifier}/" --postupdate="%LOCALAPPDATA%/{Identifier}/{app.exe}"`.
For an app updating another app, the decompressed data is moved manually to `%LOCALAPPDATA%/{Identifier}/`.

