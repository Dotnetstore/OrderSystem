FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Directory.Build.props", "./"]
COPY ["src/OrderSystem.Workers/OrderSystem.Worker.Notifier/OrderSystem.Worker.Notifier.csproj", "src/OrderSystem.Workers/OrderSystem.Worker.Notifier/"]
COPY ["src/OrderSystem.Infrastructure/OrderSystem.Infrastructure.csproj", "src/OrderSystem.Infrastructure/"]
COPY ["src/OrderSystem.Application/OrderSystem.Application.csproj", "src/OrderSystem.Application/"]
COPY ["src/OrderSystem.Domain/OrderSystem.Domain.csproj", "src/OrderSystem.Domain/"]
RUN dotnet restore "src/OrderSystem.Workers/OrderSystem.Worker.Notifier/OrderSystem.Worker.Notifier.csproj"

COPY . .
RUN dotnet publish "src/OrderSystem.Workers/OrderSystem.Worker.Notifier/OrderSystem.Worker.Notifier.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "OrderSystem.Worker.Notifier.dll"]
