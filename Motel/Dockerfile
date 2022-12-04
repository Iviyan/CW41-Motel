FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Motel/Motel.csproj", "Motel/"]
RUN dotnet restore "Motel/Motel.csproj"
COPY . .
WORKDIR "/src/Motel"
RUN dotnet build "Motel.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Motel.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Motel.dll"]
