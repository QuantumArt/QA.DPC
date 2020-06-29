
@ECHO OFF

SET /P CustomerCode= CustomerCode:
SET /P DatabaseServer= Db server:
SET /P DatabaseLogin= Db login:
SET /P DatabasePassword= Db password:
SET /P ElasticsearchHost= Elasticsearch:
SET /P BackendPort= Backend port:

if not exist "C:\QA\" mkdir C:\QA
if not exist "C:\QA\Logs" mkdir C:\QA\Logs
SET ThisScriptsDirectory=%~dp0
SET PowerShellScriptPath=%ThisScriptsDirectory%\Install\InstallConsolidationCatalog.ps1

for /r %%a in (*.ps1) do (echo.>%%a:Zone.Identifier)
PowerShell -NoProfile -ExecutionPolicy Bypass -Command "& {Start-Process PowerShell -ArgumentList '-NoProfile -ExecutionPolicy Bypass -File """"%PowerShellScriptPath%"""" -dbType 1  -databaseServer """"%DatabaseServer%"""" -login """"%DatabaseLogin%"""" -password """"%DatabasePassword%""""  -customerCode """"%CustomerCode%"""" -installRoot """"C:\QA"""" -elasticsearchHost """"%ElasticsearchHost%"""" -backendPort """"%BackendPort%"""" -logPath """"C:\QA\Logs"""" ' -Verb RunAs}";
