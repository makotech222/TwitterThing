# TwitterThing
.NET 7 implementation for processing twitter stream api tweets.

## Pre-requisites
1. Visual Studio
2. .NET 7 SDK
3. Twitter API Bearer Token

## Building
1. Open .sln file in Visual Studio and Build

## Running
1. Provide the Twitter API Bearer Token using either User Environment Variable: "TwitterBearerToken", or editing the appsettings.json variable, "TwitterBearerToken".
2. Navigate to "http://localhost:5143/" to generate the statistics that are currently available in the api.


## Developers Note
This application is running on the .NET 7 Web API framework with MinimalAPI paradigm. Entry point of app is in Program.cs, where app is configured and endpoints are created. The long running task for streaming from Twitter is also started here and run in the background. The app exposes the '/' endpoint to provide user with JSON-formatted object of the statistics requested.

The twitter stream is opened with a normal async HTTPClient request, and then processed with a System.IO.Pipelines implementation of a reader/writer. The stream is read line-by-line and pushed into a ConcurrentQueue. When the endpoint is accessed, the queue is processed and statistics are generated from this.

Currently, statistics are stored in memory, so as the program runs the memory usage will slowly rise over time as the Hashtag dictionary is filled.