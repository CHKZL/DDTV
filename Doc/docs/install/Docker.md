# DDTVLiveRec安装教程（Docker）
:::tip 提示
具体参数请按照自己的需求进行修改
:::
## 构建Docker镜像

```bash
docker build -f DDTVLiveRec/Dockerfile -t ddtv:latest .
```

## 运行Docker

```bash
docker run -d \
    --restart always \
    -p 11419:11419 \
    -v ${CONFIG_DIR}/BiliUser.ini:/DDTVLiveRec/BiliUser.ini \
    -v ${CONFIG_DIR}/DDTVLiveRec.dll.config:/DDTVLiveRec/DDTVLiveRec.dll.config \
    -v ${CONFIG_DIR}/RoomListConfig.json:/DDTVLiveRec/RoomListConfig.json \
    -v ${DOWNLOAD_DIR}:/DDTVLiveRec/tmp \
    --name ddtv \
    ddtv:latest
```