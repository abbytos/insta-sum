@echo off
setlocal

:: Define variables - Replace the placeholders with your specific details
set CDK_PROJECT_DIR=<Your-CDK-Project-Directory>
set STACK_NAME=<Your-Stack-Name>
set AWS_REGION=<Your-AWS-Region>
set SSM_PARAM_NAME=<Your-SSM-Parameter-Name>
set ACCOUNT_ID=<Your-AWS-Account-ID>
set SECRET_NUMBERS=<Your-Secret-Numbers>
set SECRET_ARN=arn:aws:secretsmanager:%AWS_REGION%:%ACCOUNT_ID%:secret:%SECRET_NUMBERS%

:: Navigate to the CDK project directory
cd /d %CDK_PROJECT_DIR%

:: Initialize CDK project (if not already initialized)
if not exist "cdk.json" (
    echo Initializing new CDK project...
    cdk init app --language csharp
)

:: Install necessary CDK dependencies
echo Installing CDK dependencies...
dotnet restore

:: Install AWS CDK CLI (if not already installed)
echo Checking if AWS CDK CLI is installed...
where cdk >nul 2>nul
if errorlevel 1 (
    echo AWS CDK CLI not found. Installing...
    npm install -g aws-cdk
)

:: Configure SSM Parameter Store with secret ARN
echo Setting SSM parameter for Secret ARN...
aws ssm put-parameter --name "%SSM_PARAM_NAME%" --value "%SECRET_ARN%" --type String --region %AWS_REGION% --overwrite

:: Bootstrap the CDK environment (if not already bootstrapped)
echo Bootstrapping CDK environment...
cdk bootstrap aws://%ACCOUNT_ID%/%AWS_REGION% --context secretArn="%SECRET_ARN%"

:: Deploy the CDK stack
echo Deploying CDK stack...
cdk deploy %STACK_NAME% --region %AWS_REGION% --context secretArn="%SECRET_ARN%"

:: Clean up and finish
echo CDK setup complete.
endlocal
pause
