FROM mcr.microsoft.com/dotnet/sdk:5.0-focal as builder

RUN apt update \
    && apt install --no-install-recommends -y git npm \
    && apt-get clean -y && rm -rf /var/lib/apt/lists/*
WORKDIR /src
COPY Auxiliary Auxiliary
COPY DDTVLiveRec DDTVLiveRec
COPY DDTVLiveRecWebServer DDTVLiveRecWebServer
COPY WebUI WebUI
RUN cd /src/DDTVLiveRec && dotnet publish -c Release /p:DebugType=None \
    && cd /src/WebUI && npm install && npm run build \
    && cp -r dist/* /src/DDTVLiveRec/bin/Release/net5.0/publish/

FROM mcr.microsoft.com/dotnet/aspnet:5.0
WORKDIR /DDTVLiveRec
RUN ln -sf /usr/share/zoneinfo/Asia/Shanghai /etc/localtime && echo 'Asia/Shanghai' >/etc/timezone
RUN apt update \
    && apt install --no-install-recommends -y ffmpeg libgdiplus \
    && apt-get clean -y && rm -rf /var/lib/apt/lists/*
COPY --from=builder /src/DDTVLiveRec/bin/Release/net5.0/publish/ /DDTVLiveRec/
EXPOSE 11419
VOLUME /DDTVLiveRec/tmp
ENTRYPOINT ["dotnet", "DDTVLiveRec.dll"]
