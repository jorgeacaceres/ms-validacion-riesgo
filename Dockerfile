FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY *.sln .
COPY ms-validacion-riesgo.csproj .
RUN dotnet restore ms-validacion-riesgo.csproj
COPY . .
RUN dotnet publish ms-validacion-riesgo.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80
ENTRYPOINT ["dotnet", "ms-validacion-riesgo.dll"]