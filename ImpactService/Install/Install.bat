
@ECHO OFF

SET /P ElasticsearchHost= Elasticsearch:

if not exist "C:\QA" mkdir C:\QA
if not exist "C:\QA\Logs" mkdir C:\QA\Logs

SET ThisScriptsDirectory=%~dp0
SET PowerShellScriptPath=%ThisScriptsDirectory%\Install\InstallImpact.ps1

PowerShell -NoProfile -ExecutionPolicy Bypass -Command "& {Start-Process PowerShell -ArgumentList '-NoProfile -ExecutionPolicy Bypass -File """"%PowerShellScriptPath%"""" -port 8033 -logPath """"C:\QA\Logs"""" -elasticBaseAddress """"%ElasticsearchHost%"""" -liveIndexName """"%CustomerCode%_products"""" -stageIndexName """"%CustomerCode%_products_stage""""' -Verb RunAs}";
