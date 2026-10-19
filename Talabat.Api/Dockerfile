FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Talabat.sln ./
COPY Talabat.Api/Talabat.Api.csproj Talabat.Api/
COPY Talabat.Application/Talabat.Application.csproj Talabat.Application/
COPY Talabat.Domain/Talabat.Domain.csproj Talabat.Domain/
COPY Talabat.Infrastructure/Talabat.Infrastructure.csproj Talabat.Infrastructure/
RUN dotnet restore Talabat.Api/Talabat.Api.csproj

COPY . .
RUN dotnet publish Talabat.Api/Talabat.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://127.0.0.1:5080
EXPOSE 5080
ENTRYPOINT ["dotnet", "Talabat.Api.dll"]
