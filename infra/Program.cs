using System.Collections.Generic;
using System.Text.Json;
using Pulumi;
using Pulumi.Aws.Iam;
using Aws = Pulumi.Aws;
using AwsApiGateway = Pulumi.AwsApiGateway;

return await Deployment.RunAsync(() => 
{
    var rollTable = new Aws.DynamoDB.Table("RollTable", new() {
        Name = "RollTable",
        BillingMode = "PAY_PER_REQUEST",
        HashKey = "Username",
        RangeKey = "Score",
        Attributes = new() {
            new Aws.DynamoDB.Inputs.TableAttributeArgs()
            {
                Name = "Username",
                Type = "S"
            },
            new Aws.DynamoDB.Inputs.TableAttributeArgs 
            {
                Name = "Score",
                Type = "N"
            }
        }
    });

    var dynamoAccessPolicy = new Policy("dynamo_access_policy", new()
    {
        Name = "dynamo_access_policy",
        PolicyDocument = rollTable.Arn.Apply(arn => JsonSerializer.Serialize(new Dictionary<string, object?>
        {
            ["Version"] = "2012-10-17",
            ["Statement"] = new[]
            {
                new Dictionary<string, object?>
                {
                    ["Action"] = new[]
                    {
				        "dynamodb:DescribeTable",
				        "dynamodb:GetItem",
				        "dynamodb:PutItem",
				        "dynamodb:Scan"
                    },
                    ["Effect"] = "Allow",
                    ["Resource"] = arn,
                },
            },
        }))
    });

    var lambdaRole = new Role($"lambdaRole", new()
    {
        AssumeRolePolicy = JsonSerializer.Serialize(new Dictionary<string, object?>
        {
            ["Version"] = "2012-10-17",
            ["Statement"] = new[]
            {
                new Dictionary<string, object?>
                {
                    ["Action"] = "sts:AssumeRole",
                    ["Effect"] = "Allow",
                    ["Principal"] = new Dictionary<string, object?>
                    {
                        ["Service"] = "lambda.amazonaws.com",
                    },
                }
            },
        }),
        ManagedPolicyArns = dynamoAccessPolicy.Arn.Apply(arn => new[]
        {
            "arn:aws:iam::aws:policy/service-role/AWSLambdaBasicExecutionRole",
            arn,
        }),
    });

    var fn = new Aws.Lambda.Function("fn", new()
    {
        Runtime = "dotnet8",
        Handler = "Lambda::Lambda.EntryPoint::Handle",
        Role = lambdaRole.Arn,
        Code = new FileArchive("../lambda/bin/Release/net8.0/publish"),
        Environment = new Aws.Lambda.Inputs.FunctionEnvironmentArgs
        {
            Variables = rollTable.Name.Apply
            (
                name => new Dictionary<string, string> 
                {
                    { "RollTable", name }
                }
            ) 
        }
    });

    var api = new AwsApiGateway.RestAPI("api", new()
    {
        Routes =
        {
            new AwsApiGateway.Inputs.RouteArgs
            {
                Path = "/",
                Method = AwsApiGateway.Method.GET,
                EventHandler = fn,
            },
            new AwsApiGateway.Inputs.RouteArgs
            {
                Path = "/",
                Method = AwsApiGateway.Method.POST,
                EventHandler = fn,
            },
        },
    });

    return new Dictionary<string, object?>
    {
        ["url"] = api.Url,
    };
});

