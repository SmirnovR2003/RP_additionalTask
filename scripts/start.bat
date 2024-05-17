echo Start Nats server
start "" /D "../" nats-server.exe

cd ../Messenger
start "" dotnet run 0
start "" dotnet run 1
start "" dotnet run 2