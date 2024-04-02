using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Lambda.APIGatewayEvents;

namespace Lambda;

public class RollActivity
{
    private Table rollTable;
    private static string USERNAME_PARAM = "username";

    public RollActivity(Table rollTable) {
        this.rollTable = rollTable;
    }

    public async Task<APIGatewayProxyResponse> Handle(APIGatewayProxyRequest request)
    {
        if (request.QueryStringParameters == null || !request.QueryStringParameters.ContainsKey(USERNAME_PARAM)) {
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Body = "No username provided!"
            };
        }
        string username = request.QueryStringParameters[USERNAME_PARAM];

        // Generate random score 1 to 1 million
        LeaderboardEntry entry = new LeaderboardEntry 
        {
            Username = username,
            Score = new Random().Next(1, 1000000)
        };
        await rollTable.PutItemAsync(entry.ToDocument());

        return new APIGatewayProxyResponse
        {
            StatusCode = (int)HttpStatusCode.OK,
            Body = $"Rolled {entry.Score} for {entry.Username}!"
        };
    }
}