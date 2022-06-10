FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 5183
EXPOSE 7183

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Telegram/Telegram.csproj", "Telegram/"]
COPY ["Shared/Shared.csproj", "Shared/"]
RUN dotnet restore "Telegram/Telegram.csproj"
COPY . .
WORKDIR "/src/Telegram"
RUN dotnet build "Telegram.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Telegram.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Telegram.dll"]
