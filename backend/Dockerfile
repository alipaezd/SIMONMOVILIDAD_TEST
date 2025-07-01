# Stage 1: build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["Simon.Movilidad.Api.csproj", "./"]
RUN dotnet restore "Simon.Movilidad.Api.csproj"
RUN dotnet add package BCrypt.Net-Next --version 4.0.2

COPY . .
RUN dotnet publish -c Release -o /app
RUN npm install -g newman

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app ./
ENTRYPOINT ["dotnet", "Simon.Movilidad.Api.dll"]
