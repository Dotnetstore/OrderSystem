FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Directory.Build.props", "./"]
COPY ["src/OrderSystem.Dashboard/OrderSystem.Dashboard.csproj", "src/OrderSystem.Dashboard/"]
RUN dotnet restore "src/OrderSystem.Dashboard/OrderSystem.Dashboard.csproj"

COPY . .
RUN dotnet publish "src/OrderSystem.Dashboard/OrderSystem.Dashboard.csproj" -c Release -o /app/publish

FROM nginx:1.27-alpine AS final
WORKDIR /usr/share/nginx/html

COPY ["src/OrderSystem.Dashboard/nginx.conf", "/etc/nginx/conf.d/default.conf"]
COPY --from=build /app/publish/wwwroot .

EXPOSE 80
