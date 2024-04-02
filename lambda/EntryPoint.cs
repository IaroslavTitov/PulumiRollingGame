using System;
using System.Net;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Lambda;

public class EntryPoint
{
    private AmazonDynamoDBClient ddbClient = new AmazonDynamoDBClient();
    private Table rollTable;
    private RollActivity rollActivity;
    private LeaderboardActivity leaderboardActivity;

    public EntryPoint() {
        rollTable = Table.LoadTable(ddbClient, Environment.GetEnvironmentVariable("RollTable"));
        rollActivity = new RollActivity(rollTable);
        leaderboardActivity = new LeaderboardActivity(rollTable);
    }

    public async Task<APIGatewayProxyResponse> Handle(APIGatewayProxyRequest request)
    {
        return request.HttpMethod switch
        {
            "GET" => await leaderboardActivity.Handle(request),
            "POST" => await rollActivity.Handle(request),
            _ => new APIGatewayProxyResponse {
                StatusCode = (int)HttpStatusCode.NotFound
            }
        };
    }
}