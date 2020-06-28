#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY *.sln .
COPY TokenGen/*.csproj ./TokenGen/
RUN dotnet restore

COPY TokenGen/. ./TokenGen.
WORKDIR /source/TokenGen
RUN dotnet publish -c release -0 /app --no-restore

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster
WORKDIR /app
COPY --from=build /app ./
ENTRYPOINT ["dotnet", "TokenGen.dll"]



#FROM build AS publish
#RUN dotnet publish "TokenGen.csproj" -c Release -o /app/publish
#
#FROM base AS final
#WORKDIR /app
#COPY --from=publish /app/publish .
#ENTRYPOINT ["dotnet", "TokenGen.dll"]