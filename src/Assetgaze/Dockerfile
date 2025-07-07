# Stage 1: Build the application
# We use the official .NET 9 SDK image which contains all the tools needed to build your project.
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR /app

# Copy the .csproj file and restore dependencies first.
# This is a clever optimization. As long as your .csproj doesn't change, Docker will use a cached layer here, speeding up future builds.
COPY src/Assetgaze/*.csproj ./Assetgaze/
RUN dotnet restore ./Assetgaze/Assetgaze.csproj

# Copy the rest of the source code for all projects
COPY src/. ./
WORKDIR /app/Assetgaze

# Build and publish the release version of your API project.
# The --no-restore flag is used because we already restored dependencies in the previous step.
RUN dotnet publish -c Release -o /app/publish --no-restore

# Stage 2: Create the final, lightweight runtime image
# We use the much smaller ASP.NET runtime image, which is optimized for running apps, not building them.
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS final
WORKDIR /app

# Copy the published output from the 'build' stage into this final image.
COPY --from=build /app/publish .

# Define the user to run the app. Running as a non-root user is a security best practice.
# .NET 9 images come with a pre-configured 'app' user.
USER app

# Expose the port the app will run on. We'll use port 8080 for HTTP internally.
EXPOSE 8080

# Entrypoint defines the command that will run when the container starts.
# This executes your compiled application DLL.
ENTRYPOINT ["dotnet", "Assetgaze.dll"]