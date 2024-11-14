# Dockerfile
FROM --platform=${BUILDPLATFORM} mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG TARGETARCH

COPY Source /Source
RUN dotnet publish \
    /Source/LibationCli/LibationCli.csproj \
    --arch ${TARGETARCH} \
    --configuration Release \
    --output /Source/bin/Publish/Linux-chardonnay \
    -p:PublishProfile=/Source/LibationCli/Properties/PublishProfiles/LinuxProfile.pubxml

FROM mcr.microsoft.com/dotnet/runtime:8.0
ARG USER_UID=1001
ARG USER_GID=1001

# Set the character set that will be used for folder and filenames when liberating
ENV LANG=C.UTF-8
ENV LC_ALL=C.UTF-8

ENV SLEEP_TIME=-1
ENV LIBATION_CONFIG_INTERNAL=/config-internal
ENV LIBATION_CONFIG_DIR=/config
ENV LIBATION_DB_DIR=/db
ENV LIBATION_DB_FILE=
ENV LIBATION_CREATE_DB=true
ENV LIBATION_BOOKS_DIR=/data


RUN apt-get update && apt-get -y upgrade && \
    apt-get install -y jq && \
    mkdir -m777 ${LIBATION_CONFIG_INTERNAL} ${LIBATION_BOOKS_DIR}

COPY --from=build /Source/bin/Publish/Linux-chardonnay /libation
COPY Docker/* /libation

USER ${USER_UID}:${USER_GID}

CMD ["/libation/liberate.sh"]
