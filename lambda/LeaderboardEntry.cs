using System;
using System.Collections.Generic;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;

namespace Lambda;
public struct LeaderboardEntry
{
    public string Username;
    public int Score;

    public Document ToDocument()
    {       
        var dictionary = new Dictionary<string, AttributeValue>
        {
            {"Username", new AttributeValue {S = Username}},
            {"Score", new AttributeValue {N = Score.ToString()}}
        };
        return Document.FromAttributeMap(dictionary);
    }

    public static LeaderboardEntry FromDocument(Document doc) {
        return new LeaderboardEntry 
        {
            Username = doc["Username"],
            Score = Int32.Parse(doc["Score"]),
        };
    }
}


