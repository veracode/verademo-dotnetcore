# For Security Labs we need both the application and DB running within the same container.
# It's far easier to use the SQL Server base image and install the ASP.NET Core 3.1 SDK
# on top than the other way around. We are using the full SDK rather than
# aspnetcore-runtime-3.1 to enable re-compilation within the lab.
#
# https://hub.docker.com/_/microsoft-mssql-server
# This is Ubuntu 16.04.7 LTS
FROM mcr.microsoft.com/mssql/server:2017-latest-ubuntu AS runtime

# Configure Sql Server
ENV ACCEPT_EULA=Y
ENV MSSQL_PID=Express
ENV SA_PASSWORD=SuperSecurePassw0rd!
COPY db/* /var/opt/mssql/data/
RUN ln -s /opt/mssql-tools/bin/sqlcmd /bin/ && chmod +x /var/opt/mssql/data/configure.sh && /var/opt/mssql/data/configure.sh

# Install the ASP.NET Core 3.1 SDK as per
# Also install the fortune-mod fortune game
# https://docs.microsoft.com/en-us/dotnet/core/install/linux-ubuntu
RUN apt-get update \
    && apt-get -y dist-upgrade \
    && apt-get -y install apt-transport-https dotnet-sdk-3.1 fortune-mod iputils-ping \
    && ln -s /usr/games/fortune /bin/ \
    && rm -rf /var/lib/apt/lists/*
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1

COPY entrypoint.sh /

WORKDIR /app
COPY app /app

ENV ASPNETCORE_URLS="http://+:8080"

# Compile
RUN dotnet build

ENTRYPOINT ["/entrypoint.sh"]
CMD ["-c"]