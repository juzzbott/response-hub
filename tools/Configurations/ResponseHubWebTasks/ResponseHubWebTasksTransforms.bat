cd %~dp0

:: Create a directory called ResponseHubConfigs
:: If you change the name of the folder here you must also update the ResponseHubConfigs.proj file
IF NOT "%1"=="ResponseHubWebTasksConfigs" (CALL ResponseHubWebTasksTransforms.bat ResponseHubWebTasksConfigs
EXIT /b 0
)

:: Build up the output structure
IF EXIST %1 RMDIR /S /Q %1 
MKDIR %1\Debug
MKDIR %1\Test
MKDIR %1\Staging
MKDIR %1\Release

:: Transform and copy App.config for each environment
:: Debug
copy /y "..\..\..\src\Tasks\ResponseHub.WebTasks\App.config" App.config
copy /y "..\..\..\src\Tasks\ResponseHub.WebTasks\App.Debug.config" App.Debug.config
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\Msbuild.exe "ResponseHubWebTasksTransforms.proj" /t:Debug
move /Y  "App.config" .\%1\Debug\App.config

:: Test
copy /y "..\..\..\src\Tasks\ResponseHub.WebTasks\App.config" App.config
copy /y "..\..\..\src\Tasks\ResponseHub.WebTasks\App.Test.config" App.Test.config
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\Msbuild.exe "ResponseHubWebTasksTransforms.proj" /t:Test
move /Y  "App.config" .\%1\Test\App.config

:: Staging
copy /y "..\..\..\src\Tasks\ResponseHub.WebTasks\App.config" App.config
copy /y "..\..\..\src\Tasks\ResponseHub.WebTasks\App.Staging.config" App.Staging.config
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\Msbuild.exe "ResponseHubWebTasksTransforms.proj" /t:Staging
move /Y  "App.config" .\%1\Staging\App.config

:: Production
copy /y "..\..\..\src\Tasks\ResponseHub.WebTasks\App.config" App.config
copy /y "..\..\..\src\Tasks\ResponseHub.WebTasks\App.Release.config" App.Release.config
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\Msbuild.exe "ResponseHubWebTasksTransforms.proj" /t:Release
move /Y  "App.config" .\%1\Release\App.config

::========================================================
:: final tidy up
if exist "*.config" del "*.config"