# Dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env

COPY Source /Source
RUN dotnet publish -c Release -o /Source/bin/Publish/Linux-chardonnay /Source/LibationCli/LibationCli.csproj -p:PublishProfile=/Source/LibationCli/Properties/PublishProfiles/LinuxProfile.pubxml
COPY Docker/liberate.sh /Source/bin/Publish/Linux-chardonnay


FROM mcr.microsoft.com/dotnet/runtime:8.0
ARG USER_UID=1001


ENV SLEEP_TIME=30m

# Set the character set that will be used for folder and filenames when liberating
ENV LANG=C.UTF-8
ENV LC_ALL=C.UTF-8

RUN apt-get update && apt-get -y upgrade && \
    mkdir /db /config /data

COPY --from=build-env /Source/bin/Publish/Linux-chardonnay /libation
COPY Docker/appsettings.json /libation/

USER ${USER_UID}

CMD ["./libation/liberate.sh"]
