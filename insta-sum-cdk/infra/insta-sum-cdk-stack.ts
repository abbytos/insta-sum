import { Construct } from 'constructs';
import * as cdk from 'aws-cdk-lib';
import * as lambda from 'aws-cdk-lib/aws-lambda';
import * as apigateway from 'aws-cdk-lib/aws-apigateway';
import * as s3 from 'aws-cdk-lib/aws-s3';
import * as cloudfront from 'aws-cdk-lib/aws-cloudfront';
import * as iam from 'aws-cdk-lib/aws-iam';

export class InstaSumCdkStack extends cdk.Stack {
  private readonly lambdaRole: iam.Role;
  private secretArn: string;

  constructor(scope: Construct, id: string, props?: cdk.StackProps) {
    super(scope, id, props);

    this.secretArn = this.node.tryGetContext('secretArn');
    if (!this.secretArn) {
      throw new Error('Context variable "secretArn" is required.');
    }

    this.lambdaRole = this.createLambdaExecutionRole();
    const bucket = this.createS3Bucket();
    this.createCloudFrontDistribution(bucket);
    const lambdaFunction = this.createLambdaFunction();
    const api = this.createApiGateway(lambdaFunction);
    this.createApiUrlOutput(api);
    this.setBucketPolicy(bucket);
  }

  /**
   * Creates and configures the Lambda execution role.
   * @returns {iam.Role} The Lambda execution role.
   */
  private createLambdaExecutionRole(): iam.Role {
    const role = new iam.Role(this, 'LambdaExecutionRole', {
      assumedBy: new iam.ServicePrincipal('lambda.amazonaws.com'),
    });

    role.addManagedPolicy(iam.ManagedPolicy.fromAwsManagedPolicyName('service-role/AWSLambdaBasicExecutionRole'));
    role.addToPolicy(new iam.PolicyStatement({
      actions: ['secretsmanager:GetSecretValue'],
      resources: [this.secretArn],
    }));

    return role;
  }

  /**
   * Creates an S3 bucket for the project.
   * @returns {s3.Bucket} The created S3 bucket.
   */
  private createS3Bucket(): s3.Bucket {
    return new s3.Bucket(this, 'InstaSumBucket', {
      removalPolicy: cdk.RemovalPolicy.DESTROY,
      autoDeleteObjects: true,
    });
  }

  /**
   * Creates a CloudFront distribution for the S3 bucket.
   * @param {s3.Bucket} bucket - The S3 bucket to use as the origin.
   */
  private createCloudFrontDistribution(bucket: s3.Bucket): void {
    new cloudfront.CloudFrontWebDistribution(this, 'Distribution', {
      originConfigs: [
        {
          s3OriginSource: {
            s3BucketSource: bucket,
          },
          behaviors: [{ isDefaultBehavior: true }],
        },
      ],
    });
  }

  /**
   * Creates a Lambda function for the application.
   * @returns {lambda.IFunction} The created Lambda function.
   */
  private createLambdaFunction(): lambda.IFunction {
    return new lambda.Function(this, 'InstaSumFunction', {
      runtime: lambda.Runtime.DOTNET_6,
      code: lambda.Code.fromAsset('lambda/SummarizeLambdaHandler/publish'),
      handler: 'SummarizeLambdaHandler::SummarizeLambdaHandler.LambdaEntryPoint::FunctionHandler',
      environment: {
        'OPENAI_API_KEY': this.secretArn,
      },
      timeout: cdk.Duration.seconds(30),
      role: this.lambdaRole,
    });
  }

  /**
   * Creates an API Gateway with Lambda integration.
   * @param {lambda.IFunction} lambdaFunction - The Lambda function to integrate with the API.
   * @returns {apigateway.RestApi} The created API Gateway.
   */
  private createApiGateway(lambdaFunction: lambda.IFunction): apigateway.RestApi {
    const api = new apigateway.RestApi(this, 'InstaSumApi', {
      restApiName: 'InstaSumService',
      deployOptions: {
        stageName: 'prod',
      },
    });

    const lambdaIntegration = new apigateway.LambdaIntegration(lambdaFunction);

    // Define API resources and methods
    const summaryResource = api.root.addResource('summary');
    const keyHighlightsResource = api.root.addResource('key-highlights');
    const importantWordsResource = api.root.addResource('important-words');

    summaryResource.addMethod('POST', lambdaIntegration);
    keyHighlightsResource.addMethod('POST', lambdaIntegration);
    importantWordsResource.addMethod('POST', lambdaIntegration);

    return api;
  }

  /**
   * Outputs the API Gateway URL.
   * @param {apigateway.RestApi} api - The API Gateway to output the URL for.
   */
  private createApiUrlOutput(api: apigateway.RestApi): void {
    new cdk.CfnOutput(this, 'ApiUrlSummary', {
      value: `https://${api.restApiId}.execute-api.${this.region}.amazonaws.com/prod/summary`,
      description: 'The URL of the API Gateway endpoint for /summary',
    });

    new cdk.CfnOutput(this, 'ApiUrlKeyHighlights', {
      value: `https://${api.restApiId}.execute-api.${this.region}.amazonaws.com/prod/key-highlights`,
      description: 'The URL of the API Gateway endpoint for /key-highlights',
    });

    new cdk.CfnOutput(this, 'ApiUrlImportantWords', {
      value: `https://${api.restApiId}.execute-api.${this.region}.amazonaws.com/prod/important-words`,
      description: 'The URL of the API Gateway endpoint for /important-words',
    });
  }

  /**
   * Sets the policy for the S3 bucket to allow specific actions.
   * @param {s3.Bucket} bucket - The S3 bucket to set the policy for.
   */
  private setBucketPolicy(bucket: s3.Bucket): void {
    const bucketPolicy = new iam.PolicyStatement({
      actions: ['s3:GetObject', 's3:PutObject', 's3:DeleteObject', 's3:ListBucket'],
      resources: [`${bucket.bucketArn}/*`, bucket.bucketArn],
      effect: iam.Effect.ALLOW,
      principals: [new iam.AccountRootPrincipal()],
    });
    bucket.addToResourcePolicy(bucketPolicy);
  }
}
