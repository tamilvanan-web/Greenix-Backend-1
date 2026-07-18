# Stage 1: Build compilation
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy all project definition files to restore dependencies
COPY ["Greenix.API/Greenix.API.csproj", "Greenix.API/"]
COPY ["Greenix.Application/Greenix.Application.csproj", "Greenix.Application/"]
COPY ["Greenix.Domain/Greenix.Domain.csproj", "Greenix.Domain/"]
COPY ["Greenix.Infrastructure/Greenix.Infrastructure.csproj", "Greenix.Infrastructure/"]
COPY ["Greenix.Persistence/Greenix.Persistence.csproj", "Greenix.Persistence/"]
COPY ["Greenix.Shared/Greenix.Shared.csproj", "Greenix.Shared/"]

RUN dotnet restore "Greenix.API/Greenix.API.csproj"

# Copy source tree and compile
COPY . .
WORKDIR "/src/Greenix.API"
RUN dotnet build "Greenix.API.csproj" -c Release -o /app/build

# Stage 2: Publish compiled assembly binaries
FROM build AS publish
RUN dotnet publish "Greenix.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Setup lightweight ASP.NET Core Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 8080
EXPOSE 8081
ENTRYPOINT ["dotnet", "Greenix.API.dll"]
