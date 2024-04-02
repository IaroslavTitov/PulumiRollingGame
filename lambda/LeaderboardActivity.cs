using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Lambda.APIGatewayEvents;

namespace Lambda;

public class LeaderboardActivity
{
    private Table rollTable;

    public LeaderboardActivity(Table rollTable) {
        this.rollTable = rollTable;
    }

    public async Task<APIGatewayProxyResponse> Handle(APIGatewayProxyRequest request)
    {
        ScanOperationConfig config = new ScanOperationConfig();
        Search search = rollTable.Scan(config);
        
        List<LeaderboardEntry> entries = new List<LeaderboardEntry>();
        do
        {
            var foundItems = await search.GetNextSetAsync();
            entries.AddRange(foundItems.Select(LeaderboardEntry.FromDocument));
        } while (!search.IsDone);

        return new APIGatewayProxyResponse
        {
            StatusCode = (int)HttpStatusCode.OK,
            Body = String.Join("\n",
                entries.OrderByDescending(entry => entry.Score).Select(
                    entry => entry.Username + " - " + entry.Score
                )
            )
        };
    }
}