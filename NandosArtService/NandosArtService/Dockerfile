FROM microsoft/dotnet:2.1-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY ["NandosArtService.csproj", "NandosArtService/"]
RUN dotnet restore "NandosArtService/NandosArtService.csproj"
WORKDIR /src/NandosArtService
COPY . .
RUN dotnet build "NandosArtService.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "NandosArtService.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "NandosArtService.dll"]