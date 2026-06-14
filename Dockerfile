FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Mindflow-backend/Mindflow-backend.csproj", "Mindflow-backend/"]
RUN dotnet restore "Mindflow-backend/Mindflow-backend.csproj"

COPY . .
RUN dotnet publish "Mindflow-backend/Mindflow-backend.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Mindflow-backend.dll"]
