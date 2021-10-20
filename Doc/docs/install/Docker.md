# DDTVLiveRec安装教程（Docker）
:::tip 提示
具体参数请按照自己的需求进行修改
:::
## 构建Docker镜像

```bash
git clone https://github.com/CHKZL/DDTV && cd DDTV
docker build -f DDTVLiveRec/Dockerfile -t ddtv:latest .
```

## 运行Docker

```bash
docker run -itd \
    --restart always \
    -p 11419:11419 \
    -v ${CONFIG_DIR}/BiliUser.ini:/DDTVLiveRec/BiliUser.ini \
    -v ${CONFIG_DIR}/DDTVLiveRec.dll.config:/DDTVLiveRec/DDTVLiveRec.dll.config \
    -v ${CONFIG_DIR}/RoomListConfig.json:/DDTVLiveRec/RoomListConfig.json \
    -v ${DOWNLOAD_DIR}:/DDTVLiveRec/tmp \
    --name ddtv \
    ddtv:latest
```

初次使用，请用阿B手机客户端扫描[http://本设备IP或域名:11419/loginqr]登陆阿B。
之后在[http://本设备IP或域名:11419/#/login]登陆DDTVLiveRec。

## 查看日志

- 查看控制台日志

```bash
docker logs -f ddtv
```

- 查看DDTV输出日志

```bash
docker ps -a
docker cp [ddtv的CONTAINER ID]:/DDTVLiveRec/LOG/DDTVLiveRecLog.out .
cat DDTVLiveRecLog.out
```

:::tip 提示
社区中可用的第三方Docker镜像：https://github.com/CHKZL/DDTV/issues/46
:::