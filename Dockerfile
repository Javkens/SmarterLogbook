FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Smarter Logbook/Smarter Logbook.csproj", "Smarter Logbook/"]
RUN dotnet restore "Smarter Logbook/Smarter Logbook.csproj"

COPY . .
WORKDIR "/src/Smarter Logbook"
RUN dotnet publish "Smarter Logbook.csproj" --configuration Release --no-restore --output /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Smarter Logbook.dll"]
