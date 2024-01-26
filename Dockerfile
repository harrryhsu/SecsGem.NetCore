FROM mcr.microsoft.com/dotnet/sdk:6.0.418-focal
WORKDIR /app

COPY . ./
RUN dotnet restore
RUN dotnet build -c Release --no-restore
RUN dotnet test SecsGem.NetCore.Test -c Release --no-restore
RUN dotnet pack -c Release --no-restore --no-build -o /sln/artifacts 

ENTRYPOINT ["dotnet", "nuget", "push", "/sln/artifacts/*.nupkg", "--source", "https://api.nuget.org/v3/index.json"]