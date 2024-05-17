echo Stopping web application on all ports
taskkill /F /IM dotnet.exe 

echo Stop nats server
taskkill /F /IM nats-server.exe

