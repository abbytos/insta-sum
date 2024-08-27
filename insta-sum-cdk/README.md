# Insta-Sum CDK

Insta-Sum CDK is the backend infrastructure for the Insta-Sum Chrome extension. This backend is built using AWS CDK in C# and is responsible for managing and deploying AWS resources to support the text summarization capabilities provided by the Chrome extension.

## Technologies Used
- **AWS CDK:** Infrastructure as code tool for defining AWS resources using a high-level programming language.
- **AWS Lambda:** For running the summarization function triggered by API Gateway.
- **API Gateway:** Exposes the Lambda function to the frontend for handling text summarization requests.
- **Secrets Manager:** Stores sensitive API keys such as OpenAI API Key.
- **SSM Parameter Store:** Stores the ARN for the OpenAI Secret.
- **S3:** Hosts static files if required.
- **C# (.NET Core):** Language used for the AWS CDK stack.

## Project Structure
This repository contains the AWS CDK project for deploying the backend infrastructure for Insta-Sum.

```bash
Insta-Sum-CDK/
├── bin/
│   └── insta-sum-cdk.ts            # Entry point for defining and deploying the CDK stack
├── infra/
│   ├── insta-sum-cdk-stack.ts      # The main CDK stack definition that includes resources like Lambda, API Gateway, and Secrets Manager
│   └── ssm-secret-handler.ts       # Helper class for retrieving secrets from SSM Parameter Store
├── lib/
│   └── insta-sum-cdk.tsproj        # C# project file for the CDK stack
├── cdk.json                        # Configuration file for AWS CDK specifying the app's entry point
├── package.json                    # Node.js package file for managing CDK dependencies
└── README.md                       # Documentation for the Insta-Sum CDK backend



## AWS Resources
The following AWS resources are provisioned by the CDK stack:

- **Lambda Function:** Handles the summarization logic using OpenAI API.
- **API Gateway:** Exposes the Lambda function as a REST API for the frontend to interact with.
- **Secrets Manager:** Stores sensitive information such as OpenAI API Key securely.
- **SSM Parameter Store:** Stores the ARN for the OpenAI secret to be retrieved by the stack.
- **S3 Bucket (optional):** For hosting any static assets if necessary for the backend.

## Project Setup

### Prerequisites
- **AWS Account**: Ensure you have an AWS account with the necessary permissions to deploy CDK stacks.
- **AWS CLI**: Ensure the AWS CLI is installed and configured with your credentials.
- **AWS CDK CLI**: Install the AWS CDK CLI using `npm install -g aws-cdk`.
- **.NET Core SDK**: Ensure the .NET Core SDK is installed for compiling and running the CDK project.

### Install Dependencies

To install the necessary dependencies for CDK and the .NET project:

```
npm install
dotnet restore
```

### Bootstrapping AWS CDK
Before deploying the stack, ensure your AWS environment is bootstrapped:

```
cdk bootstrap aws://{account-id}/{region}
```
Replace {account-id} and {region} with your AWS account ID and the desired region.

## #Deploying the Stack
To deploy the backend infrastructure, run the following command:

```
cdk deploy 
```
The cdk deploy command will provision all the necessary AWS resources for Insta-Sum.

### Fetching the Secret ARN
The OpenAI Secret ARN is stored in AWS Systems Manager (SSM) Parameter Store. You can update the ARN by setting the parameter:

```
aws ssm put-parameter --name "/InstaSum/OpenAI/SecretArn" --value "{arn}" --type String --region {region} --overwrite
```
Replace {arn} with the actual ARN of the OpenAI secret and {region} with your AWS region.

### Destroying the Stack
To clean up and remove all AWS resources provisioned by the CDK stack:

```
cdk destroy
```

## Usage
Once the stack is deployed, it will expose an API endpoint that the Insta-Sum frontend will use to send text for summarization. The Lambda function will handle the request, call the OpenAI API, and return the summarized text to the Chrome extension.

Ensure the OpenAI API key is properly stored in AWS Secrets Manager and referenced by the backend.

## Customize Configuration
For further customization, you can modify the insta-sum-cdk-stack.ts file to adjust the AWS resources, Lambda behavior, or API Gateway configurations.

## Useful CDK Commands
cdk ls: List all stacks in your CDK project.
cdk synth: Emits the synthesized CloudFormation template for the stack.
cdk diff: Compares the deployed stack with the local state to show what changes will be made.
cdk deploy: Deploy this stack to your default AWS account/region.
