FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Directory.Build.props", "./"]
COPY ["src/OrderSystem.Workers/OrderSystem.Worker.Audit/OrderSystem.Worker.Audit.csproj", "src/OrderSystem.Workers/OrderSystem.Worker.Audit/"]
RUN dotnet restore "src/OrderSystem.Workers/OrderSystem.Worker.Audit/OrderSystem.Worker.Audit.csproj"

COPY . .
RUN dotnet publish "src/OrderSystem.Workers/OrderSystem.Worker.Audit/OrderSystem.Worker.Audit.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "OrderSystem.Worker.Audit.dll"]
