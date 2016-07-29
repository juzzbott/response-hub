cd %~dp0

:: Create a directory called ResponseHubConfigs
:: If you change the name of the folder here you must also update the ResponseHubConfigs.proj file
IF NOT "%1"=="ResponseHubConfigs" (CALL ResponseHubTransforms.bat ResponseHubConfigs
EXIT /b 0
)

:: Build up the output structure
IF EXIST %1 RMDIR /S /Q %1 
MKDIR %1\Debug
MKDIR %1\Test
MKDIR %1\Staging
MKDIR %1\Release

:: Transform and copy Web.config for each environment
:: Debug
copy /y "..\..\src\ResponseHub.UI\Web.config" Web.config
copy /y "..\..\src\ResponseHub.UI\Web.Debug.config" Web.Debug.config
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\Msbuild.exe "ResponseHubTransforms.proj" /t:Debug
move /Y  "Web.config" .\%1\Debug\Web.config

:: Test
copy /y "..\..\src\ResponseHub.UI\Web.config" Web.config
copy /y "..\..\src\ResponseHub.UI\Web.Test.config" Web.Test.config
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\Msbuild.exe "ResponseHubTransforms.proj" /t:Test
move /Y  "Web.config" .\%1\Test\Web.config

:: Staging
copy /y "..\..\src\ResponseHub.UI\Web.config" Web.config
copy /y "..\..\src\ResponseHub.UI\Web.Staging.config" Web.Staging.config
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\Msbuild.exe "ResponseHubTransforms.proj" /t:Staging
move /Y  "Web.config" .\%1\Staging\Web.config

:: Production
copy /y "..\..\src\ResponseHub.UI\Web.config" Web.config
copy /y "..\..\src\ResponseHub.UI\Web.Release.config" Web.Release.config
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\Msbuild.exe "ResponseHubTransforms.proj" /t:Release
move /Y  "Web.config" .\%1\Release\Web.config

::========================================================
:: final tidy up
if exist "*.config" del "*.config"