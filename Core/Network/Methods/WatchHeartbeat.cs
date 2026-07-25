using Core.Account;
using Core.LogModule;
using Core.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;

namespace Core.Network.Methods
{
    /// <summary>
    /// B站直播观看心跳(x25Kn协议)API。
    /// 网页播放器通过 live-trace.bilibili.com 的 E(进入)/X(周期心跳) 接口上报观看行为，
    /// 是B站结算"观看时长"(粉丝勋章亲密度等)的依据；裸拉流的VLC播放器不会触发，由此类补充上报。
    /// </summary>
    public class WatchHeartbeat
    {
        /// <summary>
        /// live-trace域名(心跳专用，不走_LiveDomainName配置)
        /// </summary>
        private const string TraceDomain = "https://live-trace.bilibili.com";

        /// <summary>
        /// 心跳请求的共享HttpClient：心跳为低频请求(约每分钟1次)，共享实例避免反复建立连接
        /// </summary>
        private static readonly HttpClient _traceClient = new HttpClient(new HttpClientHandler { AllowAutoRedirect = true })
        {
            Timeout = TimeSpan.FromSeconds(10)
        };

        /// <summary>
        /// E/X响应中的心跳secret三要素，每次响应刷新并递推给下一次X请求
        /// </summary>
        public class HeartbeatSecret
        {
            /// <summary>服务器时间戳(秒)，下一次X请求的ets字段</summary>
            public long Timestamp { get; set; }
            /// <summary>心跳间隔(秒)，下一次X请求的time字段及实际等待间隔</summary>
            public int HeartbeatInterval { get; set; }
            /// <summary>HMAC密钥(benchmark)</summary>
            public string SecretKey { get; set; }
            /// <summary>链式HMAC算法索引：0=MD5 1=SHA1 2=SHA256 3=SHA224 4=SHA512 5=SHA384</summary>
            public int[] SecretRule { get; set; }
        }

        /// <summary>
        /// 获取房间分区信息(心跳id参数需要parent_area_id/area_id)
        /// </summary>
        /// <param name="roomId">真实房间号(长号)</param>
        /// <returns>成功返回(parent_area_id, area_id)，失败返回null</returns>
        public static (long ParentAreaId, long AreaId)? GetRoomAreaInfo(long roomId)
        {
            try
            {
                string body = Get.GetBody($"{Config.Core_RunConfig._LiveDomainName}/xlive/web-room/v1/index/getInfoByRoom?room_id={roomId}", true);
                if (string.IsNullOrEmpty(body))
                {
                    return null;
                }
                using var doc = JsonDocument.Parse(body);
                var root = doc.RootElement;
                if (root.GetProperty("code").GetInt32() != 0)
                {
                    return null;
                }
                var roomInfo = root.GetProperty("data").GetProperty("room_info");
                return (roomInfo.GetProperty("parent_area_id").GetInt64(), roomInfo.GetProperty("area_id").GetInt64());
            }
            catch (Exception ex)
            {
                Log.Warn(nameof(WatchHeartbeat), $"获取房间[{roomId}]分区信息失败:{ex.Message}", null, false);
                return null;
            }
        }

        /// <summary>
        /// 构造心跳device参数(JSON数组字符串)。device[0]为buvid3：扫码登录的Cookie中没有buvid3，
        /// 先尝试从Cookie解析，没有则生成一个标准格式buvid；device[1]为随机uuid
        /// </summary>
        public static string BuildDeviceJson(AccountInformation account)
        {
            string buvid = null;
            if (!string.IsNullOrEmpty(account?.strCookies))
            {
                foreach (string kv in account.strCookies.Split(';'))
                {
                    string pair = kv.Trim();
                    if (pair.StartsWith("buvid3="))
                    {
                        buvid = pair.Substring("buvid3=".Length);
                        break;
                    }
                }
            }
            if (string.IsNullOrEmpty(buvid))
            {
                buvid = AccountBuvid.CreateUniqueIdentifier();
            }
            return $"[\"{buvid}\",\"{Guid.NewGuid()}\"]";
        }

        /// <summary>
        /// 构造心跳id参数：[parent_area_id, area_id, 0, room_id]
        /// </summary>
        public static string BuildIdJson(long parentAreaId, long areaId, long roomId)
        {
            return $"[{parentAreaId},{areaId},0,{roomId}]";
        }

        /// <summary>
        /// 发送E心跳(进入房间)，成功返回secret三要素，失败返回null
        /// </summary>
        public static HeartbeatSecret SendE(string idJson, string deviceJson, long anchorUid, string ua, AccountInformation account)
        {
            try
            {
                var form = new Dictionary<string, string>
                {
                    ["id"] = idJson,
                    ["device"] = deviceJson,
                    ["ts"] = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString(),
                    ["is_patch"] = "0",
                    ["heart_beat"] = "[]",
                    ["ua"] = ua,
                    ["csrf_token"] = account.CsrfToken,
                    ["csrf"] = account.CsrfToken,
                    ["visit_id"] = "",
                    ["ruid"] = anchorUid.ToString(),
                };
                string body = PostTrace($"{TraceDomain}/xlive/data-interface/v1/x25Kn/E", form, ua, account);
                return ParseSecret(body, "E");
            }
            catch (Exception ex)
            {
                Log.Warn(nameof(WatchHeartbeat), $"观看心跳E请求异常:{ex.Message}", null, false);
                return null;
            }
        }

        /// <summary>
        /// 发送X心跳(周期观看上报)，基于上一次响应的secret计算s签名；成功返回刷新后的secret，失败返回null
        /// </summary>
        public static HeartbeatSecret SendX(string idJson, string deviceJson, long anchorUid, string ua, AccountInformation account, HeartbeatSecret prev)
        {
            try
            {
                long ts = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                //t字段的JSON属性顺序必须与网页播放器一致，服务器按此顺序重算校验s
                string tJson = $"{{\"id\":{idJson},\"device\":{deviceJson},\"ets\":{prev.Timestamp},\"benchmark\":\"{prev.SecretKey}\",\"time\":{prev.HeartbeatInterval},\"ts\":{ts},\"ua\":{JsonSerializer.Serialize(ua)}}}";
                string s = HmacChain.Compute(tJson, prev.SecretKey, prev.SecretRule);
                var form = new Dictionary<string, string>
                {
                    ["id"] = idJson,
                    ["device"] = deviceJson,
                    ["ets"] = prev.Timestamp.ToString(),
                    ["benchmark"] = prev.SecretKey,
                    ["time"] = prev.HeartbeatInterval.ToString(),
                    ["ts"] = ts.ToString(),
                    ["ua"] = ua,
                    ["is_patch"] = "0",
                    ["heart_beat"] = "[]",
                    ["csrf_token"] = account.CsrfToken,
                    ["csrf"] = account.CsrfToken,
                    ["visit_id"] = "",
                    ["ruid"] = anchorUid.ToString(),
                    ["s"] = s,
                };
                string body = PostTrace($"{TraceDomain}/xlive/data-interface/v1/x25Kn/X", form, ua, account);
                return ParseSecret(body, "X");
            }
            catch (Exception ex)
            {
                Log.Warn(nameof(WatchHeartbeat), $"观看心跳X请求异常:{ex.Message}", null, false);
                return null;
            }
        }

        /// <summary>
        /// 解析E/X响应：code==0时提取secret三要素，否则记日志并返回null
        /// </summary>
        private static HeartbeatSecret ParseSecret(string body, string stage)
        {
            if (string.IsNullOrEmpty(body))
            {
                Log.Warn(nameof(WatchHeartbeat), $"观看心跳{stage}响应为空", null, false);
                return null;
            }
            try
            {
                using var doc = JsonDocument.Parse(body);
                var root = doc.RootElement;
                int code = root.GetProperty("code").GetInt32();
                if (code != 0)
                {
                    string message = root.TryGetProperty("message", out var m) ? m.GetString() : "";
                    Log.Warn(nameof(WatchHeartbeat), $"观看心跳{stage}被服务器拒绝,code:{code},message:{message}", null, false);
                    return null;
                }
                var data = root.GetProperty("data");
                var secret = new HeartbeatSecret
                {
                    Timestamp = data.GetProperty("timestamp").GetInt64(),
                    HeartbeatInterval = data.GetProperty("heartbeat_interval").GetInt32(),
                    SecretKey = data.GetProperty("secret_key").GetString(),
                    SecretRule = data.GetProperty("secret_rule").EnumerateArray().Select(x => x.GetInt32()).ToArray(),
                };
                //间隔兜底：服务器返回异常值时按默认60秒处理，防止死循环或请求风暴
                if (secret.HeartbeatInterval <= 0)
                {
                    secret.HeartbeatInterval = 60;
                }
                return secret;
            }
            catch (Exception ex)
            {
                Log.Warn(nameof(WatchHeartbeat), $"观看心跳{stage}响应解析失败:{ex.Message}", null, false);
                return null;
            }
        }

        /// <summary>
        /// 向live-trace发送表单POST。不复用Post.PostBody：其实现存在不设置Content-Type、
        /// 表单值不做URL编码的历史行为，UA中的分号等字符会破坏表单解析
        /// </summary>
        private static string PostTrace(string url, Dictionary<string, string> form, string ua, AccountInformation account)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Content = new FormUrlEncodedContent(form);
            if (!string.IsNullOrEmpty(ua))
            {
                request.Headers.TryAddWithoutValidation("User-Agent", ua);
            }
            request.Headers.TryAddWithoutValidation("Referer", "https://live.bilibili.com/");
            if (account != null && !string.IsNullOrEmpty(account.strCookies))
            {
                request.Headers.TryAddWithoutValidation("Cookie", account.strCookies);
            }
            using var response = _traceClient.Send(request);
            response.EnsureSuccessStatusCode();
            return response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        }
    }
}
