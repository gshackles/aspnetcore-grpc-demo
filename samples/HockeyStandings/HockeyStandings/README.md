# Hockey Standings

This is a simple gRPC service that exposes some operations for checking the current NHL standings for different teams. It includes examples of how to make unary calls as well as client-side streaming, server-side streaming, and bi-directional streaming.

## Examples

Examples of making these calls can be found in the [`HockeyStandingsClient`](./HockeyStandingsClient/Program.cs) app.

### Unary Calls

The [`GetTeamRecord`](./HockeyStandingsService/Services/HockeyStandingsService.cs#L15) operation demonstrates a simple unary call:

``` protobuf
rpc GetTeamRecord (GetTeamRecordRequest) returns (TeamRecord);
```

This is demonstrated by the [`MakeUnaryCalls`](./HockeyStandingsClient/Program.cs#L26) method in the demo client app.

### Server Streaming Calls

The [`GetAllTeamRecords`](./HockeyStandingsService/Services/HockeyStandingsService.cs#L26) operation demonstrates a server-side streaming call:

``` protobuf
rpc GetAllTeamRecords (GetAllTeamRecordsRequest) returns (stream TeamRecord);
```

This is demonstrated by the [`MakeServerStreamingCall`](./HockeyStandingsClient/Program.cs#L42) method in the demo client app. The server-side implementation has a small delay in between returning each item, to show that the client is receiving items incrementally, rather than having to wait for the entire operation to complete.

### Bi-Directional Streaming Calls

The [`GetTeamRecords`](./HockeyStandingsService/Services/HockeyStandingsService.cs#L40) operation demonstrates a bi-directional streaming call:

``` protobuf
rpc GetTeamRecords (stream GetTeamRecordRequest) returns (stream TeamRecord);
```

This is demonstrated by the [`MakeBidirectionalStreamingCall`](./HockeyStandingsClient/Program.cs#L50) method in the demo client app.

### Cancellation

The [`MakeStreamingCallWithCancellation`](./HockeyStandingsClient/Program.cs#L77) method in the demo client app shows how to use a `CancellationToken` in order to cancel an operation in progress.