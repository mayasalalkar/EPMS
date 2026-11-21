FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY Epms.sln ./
COPY Epms.Api/Epms.Api.csproj Epms.Api/
COPY Epms.Tests/Epms.Tests.csproj Epms.Tests/
RUN dotnet restore Epms.Api/Epms.Api.csproj
COPY . .
RUN dotnet publish Epms.Api/Epms.Api.csproj -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Epms.Api.dll"]
