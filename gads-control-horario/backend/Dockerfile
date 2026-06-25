FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY *.sln .
COPY ControlHorario.Domain/*.csproj ControlHorario.Domain/
COPY ControlHorario.Application/*.csproj ControlHorario.Application/
COPY ControlHorario.Infrastructure/*.csproj ControlHorario.Infrastructure/
COPY ControlHorario.Api/*.csproj ControlHorario.Api/
RUN dotnet restore
COPY . .
RUN dotnet publish ControlHorario.Api -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "ControlHorario.Api.dll"]
