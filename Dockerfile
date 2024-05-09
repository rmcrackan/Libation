# Dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 as build-env

COPY Source /Source
RUN dotnet publish -c Release -o /Source/bin/Publish/Linux-chardonnay /Source/LibationCli/LibationCli.csproj -p:PublishProfile=/Source/LibationCli/Properties/PublishProfiles/LinuxProfile.pubxml
COPY Docker/liberate.sh /Source/bin/Publish/Linux-chardonnay


FROM mcr.microsoft.com/dotnet/runtime:8.0

ENV SLEEP_TIME "30m"

# Sets the character set that will be used for folder and filenames when liberating
ENV LANG C.UTF-8
ENV LC_ALL C.UTF-8

RUN mkdir /db /config /data

COPY --from=build-env /Source/bin/Publish/Linux-chardonnay /libation


CMD ["./libation/liberate.sh"]
