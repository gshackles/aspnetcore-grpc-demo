# Rangers Playoff Status Checker

This is a silly demo app, just to demonstrate how .NET Core apps can easily be hosted in a variety of different environments. The app polls the NHL API every second to check on where the Rangers are in the standings, and reports back whether they're currently playoff-bound.

## Steps

Here I wanted to outline the steps I'll follow as part of this talk, showing how to incrementally set up the different hosting fixtures.

### Base Setup

First I'll set up the main application using the worker template:

```
dotnet new worker
```

Next I'll add some improved logging support:

```
dotnet add package Serilog
dotnet add package Serilog.Sinks.File
dotnet add package Serilog.AspNetCore
```

Then I'll update `Program.cs` to use Serilog, and add some extra logging around starting and stopping the service ([see this commit for details](https://github.com/gshackles/aspnetcore-grpc-demo/commit/fc5baccc943a9fc2b0e9e85f7798cd366d6d77cc)). 

Now if you run the app you'll see it log `Worker running at: ...` every second as the worker polls. Finally, I replace the default worker implementation with one that actually checks the standings ([see this commit for details](https://github.com/gshackles/aspnetcore-grpc-demo/commit/1180b057bccefe86cfe6d0b892424af3c65531ee)) and we've now got our functioning app!

### Add Hosting Options

With the service now working as a console application, it's time to add some new hosting options to make it more useful.

#### Windows Service

First we'll host the app as a Windows service. To do so, we'll add in this NuGet package to bring in support for the Windows service lifecycle:

``` 
dotnet add package Microsoft.Extensions.Hosting.WindowsServices
```

Next we'll need to update `Program.cs` to use that lifecycle management. In the `CreateHostBuilder()` method, simply add this to the call chain:

``` csharp
.UseWindowsService()
```

That's actually all that needs to change! Let's go ahead and publish the app to a folder that we can use for the Windows service, just like you'd do for an app you would distribute:

```
dotnet publish -o c:\temp\rangers
```

Now we can create a Windows service from this path, either using the `sc` command or PowerShell like this:

``` powershell
New-Service -Name "Rangers Playoff Status Checker" -BinaryPathName "c:\temp\rangers\RangersPlayoffStatusChecker.exe"
```

Now the service is created, so you can start it up and see it processing and logging! [This commit](https://github.com/gshackles/aspnetcore-grpc-demo/commit/b6e5f2cdc075efd4b986f4be2e95e99a41f62931) can be used to see the changes made here.

#### Linux Service via systemd

From a code perspective, adding systemd support is just like adding Windows service support. First, add in the appropriate NuGet package:

```
dotnet add package Microsoft.Extensions.Hosting.Systemd
```

Next, add this to the call chain in `CreateHostBuilder()`:

``` csharp
.UseSystemd();
```

Now we'll publish a version of the service specifically targeted for Linux:

```
dotnet publish -r linux-x64 -o ~/rangers
```

With that published and in place, we'll create a systemd service that targets it located at `/etc/systemd/system/rangers_status.service`:

```
[Unit]
Description=Rangers playoff status checker

[Service]
Type=notify
ExecStart=/home/greg/rangers/RangersPlayoffStatusChecker

[Install]
WantedBy=multi-user.target
```

After creating that for the first time, run this command to refresh the services:

```
sudo systemctl daemon-reload
```

Now you can start the service:

```
sudo systemctl start rangers_status
```

View the running status of the service, including its output:

```
sudo systemctl status rangers_status
```

And stop the service:

```
sudo systemctl status rangers_status
```

[This commit](https://github.com/gshackles/aspnetcore-grpc-demo/commit/ff3d95334d4f85178dd0c6161c632d8907fbf9db) can be used to see the changes made here.

#### Docker

Finally, we'll look at taking this app and running it via a Docker container which could then get deployed to your container service of choice. You can add the `Dockerfile` by hand, but it's easiest to just let Visual Studio handle it for you by right clicking on the project and selecting `Add > Docker Support`.

After you do that you'll see the option to run it via Docker right from within Visual Studio, or you can create images and run it yourself as well:

```
docker image build -t rangers-checker:1.0 .
docker container run --detach --name rangers rangers-checker:1.0
```

Once it's running you can check its logs to see it in action:

```
docker container logs rangers
```

[This commit](https://github.com/gshackles/aspnetcore-grpc-demo/commit/3112c350dc6a3d17ee497bb25d6f008eef080161) can be used to see the changes made here.