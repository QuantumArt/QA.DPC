
@ECHO OFF

SET /P CustomerCode= CustomerCode:
SET /P DatabaseServer= Db server:
SET /P ElasticsearchHost= Elasticsearch:
SET /P BackendPort= Backend port:

if not exist "C:\QA\" mkdir C:\QA
SET ThisScriptsDirectory=%~dp0
SET PowerShellScriptPath=%ThisScriptsDirectory%\Install\InstallConsolidationCatalog.ps1

PowerShell -NoProfile -ExecutionPolicy Bypass -Command "& {Start-Process PowerShell -ArgumentList '-NoProfile -ExecutionPolicy Bypass -File """"%PowerShellScriptPath%"""" -databaseServer """"%DatabaseServer%"""" -customerCode """"%CustomerCode%"""" -installRoot """"C:\QA"""" -elasticsearchHost """"%ElasticsearchHost%"""" -backendPort """"%BackendPort%"""" -logPath """%ThisScriptsDirectory%Install.log""" ' -Verb RunAs}";
