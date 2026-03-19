# OrderSystem Docker setup

This repository now includes Dockerfiles for the runnable services plus compose files for shared, development, and production-oriented setups.

## Included services

- `api` - ASP.NET Core API
- `dashboard` - Blazor WebAssembly dashboard served by nginx
- `worker-processor`
- `worker-notifier`
- `worker-audit`
- `rabbitmq`

The API uses a persistent SQLite database stored in the `orders-data` named volume.

## Compose files

- `docker-compose.yml` - shared base configuration
- `docker-compose.override.yml` - development settings loaded automatically by `docker compose`
- `docker-compose.prod.yml` - standalone production-oriented configuration with named images

## Start the development stack

```powershell
docker compose up --build
```

## Validate the development config

```powershell
docker compose config
```

## Start the production-oriented stack

Build and tag the images first:

```powershell
docker compose build
docker tag ordersystem/api:dev ordersystem/api:latest
docker tag ordersystem/dashboard:dev ordersystem/dashboard:latest
docker tag ordersystem/worker-processor:dev ordersystem/worker-processor:latest
docker tag ordersystem/worker-notifier:dev ordersystem/worker-notifier:latest
docker tag ordersystem/worker-audit:dev ordersystem/worker-audit:latest
```

Then start the production-oriented stack:

```powershell
docker compose -f docker-compose.prod.yml up -d
```

## Useful URLs

- Dashboard: `http://localhost:8081`
- API: `http://localhost:8080`
- RabbitMQ Management: `http://localhost:15672`

## Health endpoints

- API health: `http://localhost:8080/health`
- Dashboard health: `http://localhost:8081/healthz`

RabbitMQ default credentials:

- Username: `guest`
- Password: `guest`

## Stop the stack

```powershell
docker compose down
```

## Stop the stack and remove volumes

```powershell
docker compose down --volumes
```

## Stop the production-oriented stack

```powershell
docker compose -f docker-compose.prod.yml down
```

