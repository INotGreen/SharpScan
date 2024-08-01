@echo off
REM 设置项目目录
SET PROJECT_DIR=SharpScan.Core

REM 进入项目目录
cd %PROJECT_DIR%

REM 恢复项目依赖
dotnet restore

REM 发布项目，生成适用于 Linux 的单文件可执行文件
dotnet publish -c Release -r linux-x64 --self-contained /p:PublishSingleFile=true /p:EnableCompressionInSingleFile=true /p:SingleFileCompression=Enabled

REM 发布后的文件路径
SET PUBLISH_DIR=%PROJECT_DIR%\..\Bin
REM 提示发布完成
echo 发布完成！文件位于 %PUBLISH_DIR%
pause
