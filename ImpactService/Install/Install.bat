
@ECHO OFF

SET /P ElasticUrl=Elasticsearch URL:
SET /P CustomerCode=Customer code:
SET /P IsPg=Is PostgreSQL customer code? (y/n)

if /i %IsPg% == y (set LiveSuff="_products_pg") else (set LiveSuff="_products")
if /i %IsPg% == y (set StageSuff="_products_stage_pg") else (set StageSuff="_products_stage")

if not exist "C:\QA" mkdir C:\QA
if not exist "C:\QA\Logs" mkdir C:\QA\Logs

SET ThisScriptsDirectory=%~dp0
SET PowerShellScriptPath=%ThisScriptsDirectory%\Install\Install.ps1

PowerShell -NoProfile -ExecutionPolicy Bypass -Command "& {Start-Process PowerShell -ArgumentList '-NoProfile -ExecutionPolicy Bypass -File """"%PowerShellScriptPath%"""" -port 8033 -logPath """"C:\QA\Logs"""" -elasticBaseAddress """"%ElasticUrl%"""" -liveIndexName """"%CustomerCode%%LiveSuff%"""" -stageIndexName """"%CustomerCode%%StageSuff%""""' -Verb RunAs}";
