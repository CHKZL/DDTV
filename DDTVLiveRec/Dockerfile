FROM mcr.microsoft.com/dotnet/sdk:5.0-focal as builder

RUN apt update \
    && apt install git -y \
    && apt-get clean -y && rm -rf /var/lib/apt/lists/*
WORKDIR /src
COPY Auxiliary Auxiliary
COPY DDTVLiveRec DDTVLiveRec
COPY DDTVLiveRecWebServer DDTVLiveRecWebServer
RUN cd DDTVLiveRec && dotnet publish -c Release /p:DebugType=None

FROM mcr.microsoft.com/dotnet/aspnet:5.0
WORKDIR /DDTVLiveRec
RUN ln -sf /usr/share/zoneinfo/Asia/Shanghai /etc/localtime && echo 'Asia/Shanghai' >/etc/timezone
RUN apt update \
    && apt install ffmpeg -y \
    && apt-get clean -y && rm -rf /var/lib/apt/lists/*
COPY --from=builder /src/DDTVLiveRec/bin/Release/net5.0/publish/ /DDTVLiveRec/
EXPOSE 11419
VOLUME /DDTVLiveRec/tmp
VOLUME /DDTVLiveRec/BiliUser.ini
VOLUME /DDTVLiveRec/DDTVLiveRec.dll.config
VOLUME /DDTVLiveRec/RoomListConfig.json
ENTRYPOINT ["dotnet", "DDTVLiveRec.dll"]