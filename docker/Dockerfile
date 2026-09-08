FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ChallengeNET-main/ChallengeNET/ClyvoCare.API.csproj ChallengeNET/
WORKDIR /src/ChallengeNET
RUN dotnet restore

COPY ChallengeNET-main/ChallengeNET/. .
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

RUN addgroup --system appuser && adduser --system --ingroup appuser appuser
RUN chown -R appuser:appuser /app
USER appuser

ENTRYPOINT ["dotnet", "ClyvoCare.API.dll"]
