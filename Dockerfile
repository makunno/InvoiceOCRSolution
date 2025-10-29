FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

COPY InvoiceOCRSolution.sln ./
COPY InvoiceOCR.Api/ ./InvoiceOCR.Api/
COPY InvoiceOCR.Core/ ./InvoiceOCR.Core/
COPY InvoiceOCR.Data/ ./InvoiceOCR.Data/

RUN dotnet restore

COPY . ./
RUN dotnet publish InvoiceOCR.Api/InvoiceOCR.Api.csproj -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build-env /app/out .

ENTRYPOINT ["dotnet", "InvoiceOCR.Api.dll"]
