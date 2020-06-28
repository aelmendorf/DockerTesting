#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY *.sln .
COPY WeatherStream/*.csproj ./WeatherStream/
RUN dotnet restore

COPY WeatherStream/. ./WeatherStream.
WORKDIR /source/WeatherStream
RUN dotnet publish -c release -0 /app --no-restore

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster
WORKDIR /app
COPY --from=build /app ./
ENTRYPOINT ["dotnet", "WeatherStream.dll"]



#FROM build AS publish
#RUN dotnet publish "TokenGen.csproj" -c Release -o /app/publish
#
#FROM base AS final
#WORKDIR /app
#COPY --from=publish /app/publish .
#ENTRYPOINT ["dotnet", "TokenGen.dll"]