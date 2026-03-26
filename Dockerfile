FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY task1/task1.csproj task1/
COPY task1.Application/task1.Application.csproj task1.Application/
COPY task1.DataLayer/task1.DataLayer.csproj task1.DataLayer/
RUN dotnet restore task1/task1.csproj

COPY task1/ task1/
COPY task1.Application/ task1.Application/
COPY task1.DataLayer/ task1.DataLayer/

WORKDIR /src/task1
RUN dotnet publish task1.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "task1.dll"]
