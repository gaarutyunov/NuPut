# NuPut

.NET Core 3.1 Console application that can be used to push packages to your NuGet.Server (e.g. Nexus)

### Clone project

```bash
git clone https://github.com/gaarutyunov/NuPut.git
```

### Enter project directory

```bash
cd NuPut/NuPut
```

### Pack project

```bash
dotnet pack
```

### Install as global tool

```bash
dotnet tool install --global --add-source ./nupkg nuput
```

### Create Automator app and use it inside Finder

1. Run Automator, select new Application
2. Select Utilities -> Double click ‘Run AppleScript’
3. Paste script:
```
on run {input, parameters}
	tell application "Finder"
		set dir_path to quoted form of (POSIX path of (folder of the front window as alias))
	end tell
	CD_to(dir_path)
end run

on CD_to(theDir)
	tell application "iTerm"
		activate
		set win to (create window with default profile)
		set sesh to (current session of win)
		tell sesh to write text "nuput -h <yourHostUrl> -d " & theDir
	end tell
end CD_to
```
4. Replace <yourHostUrl in script>
5. Save as 'NuPut.app' somewhere out of the way
6. Drag and drop the 'NuPut.app' onto the Finder toolbar while holding down the Option and Command keys to place the launch icon on Finder
7. To prevent two windows from opening when you click the launch icon when iTerm2 is not already running, set iTerm2 -> Preferences -> Startup to "Don't Open Any Windows"
8. If you want to replace the generic Automator icon, copy nuput.icns image from repository with \<Cmd\>\<C\>, then \<Cmd\>\<I\> on NuPut.app, select the application icon in the top left corner of the window and \<Cmd\>\<V\>.