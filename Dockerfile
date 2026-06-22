    
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["RoadSafety-backend.Presentation/RoadSafety-backend.Presentation.csproj", "RoadSafety-backend.Presentation/"]
COPY ["RoadSafety-backend.Application/RoadSafety-backend.Application.csproj", "RoadSafety-backend.Application/"]
COPY ["RoadSafety-backend.Infrastructure/RoadSafety-backend.Infrastructure.csproj", "RoadSafety-backend.Infrastructure/"]
COPY ["RoadSafety-backend.Domain/RoadSafety-backend.Domain.csproj", "RoadSafety-backend.Domain/"]

RUN dotnet restore "RoadSafety-backend.Presentation/RoadSafety-backend.Presentation.csproj"

COPY . .
RUN dotnet publish "RoadSafety-backend.Presentation/RoadSafety-backend.Presentation.csproj" \
    --configuration Release \
    --no-restore \
    --output /app/publish

FROM runtime AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "RoadSafety-backend.Presentation.dll"]
