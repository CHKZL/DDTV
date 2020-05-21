using Auxiliary;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using Newtonsoft.Json.Linq;

namespace AuxiliaryTests
{
    [TestClass]
    public class TestCacheSystem
    {
        [TestMethod]
        public void TestRealAPI()
        {
            var wc = new WebClient();
            wc.Headers.Add("Accept: */*");
            wc.Headers.Add("User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/81.0.4044.138 Safari/537.36");
            wc.Headers.Add("Accept-Language: zh-CN,zh;q=0.8,en;q=0.6,ja;q=0.4");
            IAPIRequest r = new APIRequest()
            {
                Url = "https://api.live.bilibili.com/room/v1/Room/get_info?id=1",
                WebClient = wc
            };

            Pipeline<IAPIRequest, string> pipeline = new Pipeline<IAPIRequest, string>();
            pipeline.AddStep(new APIRequestToResponseJObjectPipe());
            pipeline.AddStep(new ResponseJObjectToRoomUidPipe());

            string result = pipeline.Accept(r);
            Assert.AreEqual(result, "9617619");
        }

        [TestMethod]
        public void TestCachedObject()
        {
            var wc = new WebClient();
            wc.Headers.Add("Accept: */*");
            wc.Headers.Add("User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/81.0.4044.138 Safari/537.36");
            wc.Headers.Add("Accept-Language: zh-CN,zh;q=0.8,en;q=0.6,ja;q=0.4");
            IAPIRequest r = new BiliAPIRequest()
            {
                BaseUrl = "https://api.live.bilibili.com/room/v1/Room/get_info",
                Args = new Dictionary<string, string>
                {
                    { "id", "1" }
                },
                WebClient = wc
            };

            Pipeline<IAPIRequest, string> pipeline = new Pipeline<IAPIRequest, string>();
            pipeline.AddStep(new APIRequestToResponseJObjectPipe());
            pipeline.AddStep(new ResponseJObjectToRoomUidPipe());

            var cachedUid = new CachedObject<string>(r, pipeline);
            Assert.AreEqual(cachedUid.IsCached, false);
            var testOne = cachedUid.Get();
            Assert.AreEqual(testOne, "9617619");
            Assert.AreEqual(cachedUid.IsCached, true);
            DateTime updateTime = cachedUid.LastUpdateTime;
            var testTwo = cachedUid.Get();
            Assert.AreEqual(testTwo, "9617619");
            Assert.AreEqual(updateTime, cachedUid.LastUpdateTime);
            Assert.AreSame(testOne, testTwo);
            Thread.Sleep(500);
            var testThree = cachedUid.ForceUpdate();
            Assert.AreEqual(testThree, "9617619");
            Assert.AreNotEqual(updateTime, cachedUid.LastUpdateTime);
        }
    }

    public class ResponseJObjectToRoomUidPipe : IPipe
    {
        public object Invoke(object input)
        {
            JObject response = (JObject)input;

            var uid = response["data"]["uid"].ToString();
            return uid;
        }
    }
}
