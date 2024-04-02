### Summary

This is a sample project I created to learn how to use Pulumi CLI and Pulumi Cloud.
This is just a quick demo, so quality of code is not stellar and there are no tests or anything like that.

The mini-project I came up with is a basic dice rolling game. For infrastructure I picked AWS Serverless, since that’s what I’m very familiar with. I created a Lambda-backed API Gateway and a DynamoDB table to store roll data. There are 2 API endpoints: Roll and Leaderboard. Roll will take a player name, generate a random number and store it in DDB. Leaderboard will return all entries in the table, so that players can check their ranking.

To run this, install pulumi CLI and connect to AWS.
Then run from the root directory:
`dotnet publish`
`pulumi up --yes -s RollingGame -C infra`

Pulumi will then create all the cloud resources and create your own little API game!
