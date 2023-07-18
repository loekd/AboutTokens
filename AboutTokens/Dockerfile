FROM mcr.microsoft.com/dotnet/aspnet:8.0-preview AS base
RUN apt update && apt install clang gcc lld musl-dev -y
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0-preview AS build
RUN apt update && apt install clang gcc lld musl-dev -y
WORKDIR /src
COPY ["AboutTokens/AboutTokens.csproj", "AboutTokens/"]
RUN dotnet restore "AboutTokens/AboutTokens.csproj"
COPY . .
WORKDIR "/src/AboutTokens"
RUN dotnet build "AboutTokens.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AboutTokens.csproj" -c Release -o /app/publish -r linux-musl-x64

FROM nginx:alpine AS final
WORKDIR /app
COPY --from=publish /app/publish/wwwroot .
EXPOSE 80
#ENTRYPOINT ["dotnet", "AboutTokens.dll"]