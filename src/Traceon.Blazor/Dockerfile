# -- Build stage -----------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /repo

# Copy Directory.Build.props files
COPY src/Traceon.Blazor/Directory.Build.props src/Traceon.Blazor/
COPY src/Traceon.Api/Traceon.Api/src/Directory.Build.props src/Traceon.Api/Traceon.Api/src/

# Copy project files for restore
COPY src/Traceon.Blazor/Traceon.Blazor/Traceon.Blazor.csproj src/Traceon.Blazor/Traceon.Blazor/
COPY src/Traceon.Api/Traceon.Api/src/Traceon.Contracts/Traceon.Contracts.csproj src/Traceon.Api/Traceon.Api/src/Traceon.Contracts/

RUN dotnet restore src/Traceon.Blazor/Traceon.Blazor/Traceon.Blazor.csproj

# Copy all source and publish
COPY src/Traceon.Blazor/Traceon.Blazor/ src/Traceon.Blazor/Traceon.Blazor/
COPY src/Traceon.Api/Traceon.Api/src/Traceon.Contracts/ src/Traceon.Api/Traceon.Api/src/Traceon.Contracts/

RUN dotnet publish src/Traceon.Blazor/Traceon.Blazor/Traceon.Blazor.csproj -c Release -o /app/publish --no-restore

# -- Serve with Nginx ------------------------------------------
FROM nginx:alpine AS runtime

RUN rm -rf /usr/share/nginx/html/*

COPY --from=build /app/publish/wwwroot /usr/share/nginx/html
COPY src/Traceon.Blazor/nginx.conf /etc/nginx/nginx.conf

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
