FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Directory.Build.props", "./"]
COPY ["src/OrderSystem.Api/OrderSystem.Api.csproj", "src/OrderSystem.Api/"]
COPY ["src/OrderSystem.Application/OrderSystem.Application.csproj", "src/OrderSystem.Application/"]
COPY ["src/OrderSystem.Infrastructure/OrderSystem.Infrastructure.csproj", "src/OrderSystem.Infrastructure/"]
COPY ["src/OrderSystem.Domain/OrderSystem.Domain.csproj", "src/OrderSystem.Domain/"]
RUN dotnet restore "src/OrderSystem.Api/OrderSystem.Api.csproj"

COPY . .
RUN dotnet publish "src/OrderSystem.Api/OrderSystem.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

RUN apt-get update \
	&& apt-get install -y --no-install-recommends wget \
	&& rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "OrderSystem.Api.dll"]
