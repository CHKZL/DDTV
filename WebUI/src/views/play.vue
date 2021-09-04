<template>
    <video-player
      ref="videoPlayer"
      class="vjs-big-play-centered"
      :options="playerOptions"
      :playsinline="true"
      customEventName="customstatechangedeventname"
      @ready="playerReadied"
    >
      <!-- @play="onPlayerPlay($event)"
      @pause="onPlayerPause($event)"
      @ended="onPlayerEnded($event)"
      @waiting="onPlayerWaiting($event)"
      @playing="onPlayerPlaying($event)"
      @loadeddata="onPlayerLoadeddata($event)"
      @timeupdate="onPlayerTimeupdate($event)"
      @canplay="onPlayerCanplay($event)"
      @canplaythrough="onPlayerCanplaythrough($event)"
      @statechanged="playerStateChanged($event)" -->
    </video-player>
    
</template>

<script>
import {play_str} from "../utils/play_url"
export default {
  data() {
    return {
      name: "",
      path: "",
      playerOptions: {
        playbackRates: [0.5, 1.0, 1.5, 2.0], // 可选的播放速度
        autoplay: false, // 如果为true,浏览器准备好时开始回放。
        muted: false, // 默认情况下将会消除任何音频。
        loop: false, // 是否视频一结束就重新开始。
        preload: "auto", // 建议浏览器在<video>加载元素后是否应该开始下载视频数据。auto浏览器选择最佳行为,立即开始加载视频（如果浏览器支持）
        language: "zh-CN",
        aspectRatio: "16:9", // 将播放器置于流畅模式，并在计算播放器的动态大小时使用该值。值应该代表一个比例 - 用冒号分隔的两个数字（例如"16:9"或"4:3"）
        fluid: true, // 当true时，Video.js player将拥有流体大小。换句话说，它将按比例缩放以适应其容器。
        sources: [
          {
            // type:"rtmp/flv",
            type:
              "video/mp4" ||
              "rtmp/flv" ||
              "video/ogg" ||
              "video/webm" ||
              "video/avi" ||
              "video/flv" ||
              "video/mkv" ||
              "video/mov",
            src: "",
          },
        ],
        poster: "", // 封面地址
        notSupportedMessage: "此视频暂无法播放，请稍后再试",
      },

    };
  },
  mounted() {
    this.name = this.$route.query.name;
    this.path = this.$route.query.path;
    let url = play_str(this.name,this.path)
    console.log(url);
    this.playerOptions.sources[0].src = url;
    this.$refs.videoPlayer.player;
  },
  computed: {
  },
  methods: {
    // listen event
    onPlayerPlay(player) {
      console.log("player play!", player);
    },
    onPlayerPause(player) {
      console.log("player pause!", player);
    },
    // ...player event

    // or listen state event
    playerStateChanged(playerCurrentState) {
      console.log("player current update state", playerCurrentState);
    },

    // player is ready
    playerReadied(player) {
      console.log("the player is readied", player);
      // you can use it to do something...
      // player.[methods]
    },
  },
};
</script>
<style scoped>
/* .video-js .vjs-big-play-button {
  width: 72px;
  height: 72px;
  border-radius: 100%;
  z-index: 100;
  background-color: #ffffff;
  border: solid 1px #979797;
} */
</style>