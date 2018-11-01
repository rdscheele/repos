FROM microsoft/dotnet:2.1-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 64480
EXPOSE 44393

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY ["FrontEndApplication/FrontEndApplication.csproj", "FrontEndApplication/"]
RUN dotnet restore "FrontEndApplication/FrontEndApplication.csproj"
COPY . .
WORKDIR "/src/FrontEndApplication"
RUN dotnet build "FrontEndApplication.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "FrontEndApplication.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "FrontEndApplication.dll"]