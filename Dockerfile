FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["PharmacyProjectApi/PharmacyProjectApi.csproj", "PharmacyProjectApi/"]
COPY ["Pharmacy.Application/Pharmacy.Application.csproj", "Pharmacy.Application/"]
COPY ["Pharmacy.Domain/Pharmacy.Domain.csproj", "Pharmacy.Domain/"]
COPY ["Pharmacy.Infrastructure/Pharmacy.Infrastructure.csproj", "Pharmacy.Infrastructure/"]
RUN dotnet restore "PharmacyProjectApi/PharmacyProjectApi.csproj"
COPY . .
WORKDIR "/src/PharmacyProjectApi"
RUN dotnet build "PharmacyProjectApi.csproj" -c Release -o /app/build

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/build .
ENTRYPOINT ["dotnet", "PharmacyProjectApi.dll"]
