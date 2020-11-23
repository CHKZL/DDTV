using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Auxiliary
{
    /// <summary>
    /// 请求处理器接口。
    /// 一个请求处理器接受某一表示获取对象特征的标识符（ID），并通过请求API返回该对象。
    /// </summary>
    /// <typeparam name="T">返回对象的类型</typeparam>
    public interface IRequestHandler<T>
    {
        T Accept(string id);
    }

    /// <summary>
    /// 实现请求处理器接口的基类。
    /// 包含一部分所有请求处理器都通用的代码，并暴露一些钩子函数交由子类实现。
    /// </summary>
    /// <typeparam name="T">返回对象的类型</typeparam>
    public abstract class BaseRequestHandler<T> : IRequestHandler<T>
    {
        protected IAPIRequest _request;
        protected JObject _responseJObject;
        protected string _id;

        /// <summary>
        /// 返回请求API对象所需的WebClient。由子类重载。
        /// 可以通过引用已创建的WebClient对象来降低内存消耗。
        /// </summary>
        /// <returns></returns>
        protected abstract WebClient getWebClient();

        /// <summary>
        /// 由标识符（ID）构建通用API请求对象的过程。由子类重载。
        /// </summary>
        /// <param name="id">唯一表示返回对象的标识符（ID），类似字典中的键值。</param>
        /// <returns></returns>
        protected abstract IAPIRequest buildRequest(string id);

        private JObject _getResponseJObject()
        {
            byte[] responseBytes;
            try
            {
                responseBytes = getWebClient().DownloadData(_request.Url);
            }
            catch (Exception e)
            {
                InfoLog.InfoPrintf($"[{_id}] {_request.ExceptionString}: {e.Message}", 
                    InfoLog.InfoClass.Debug);
                return null;
            }
            var responseString = Encoding.UTF8.GetString(responseBytes);
            var jsonObject = JObject.Parse(responseString);
            return jsonObject;
        }

        
        /// <summary>
        /// 从返回的JSON对象得到结果所需的处理过程。由子类重载。
        /// </summary>
        /// <param name="responseJObject">API返回的JSON对象。可能为空，代表API访问失败。</param>
        /// <returns></returns>
        protected abstract T responseToResult(JObject responseJObject);

        public T Accept(string id)
        {
            _id = id;
            _request = buildRequest(_id);
            _responseJObject = _getResponseJObject();
            return responseToResult(_responseJObject);
        }
    }

    public class CachedObject<T>
    {
        private string _id;
        private IRequestHandler<T> _handler;

        public bool IsCached { get => _value != null; }
        public DateTime LastUpdateTime { get; protected set; }
        public int TimeoutSec { get; set; }
        public bool IsTimedOut { get => (DateTime.Now - LastUpdateTime).TotalSeconds > TimeoutSec; }

        private T _value;

        public T Get()
        {
            if (IsCached && !IsTimedOut) return _value;
            else return ForceUpdate();
        }

        public T ForceUpdate()
        {
            T result = _handler.Accept(_id);
            _value = result;
            LastUpdateTime = DateTime.Now;
            return result;
        }

        public CachedObject(IRequestHandler<T> handler, string id)
        {
            _id = id;
            _handler = handler;
            TimeoutSec = int.MaxValue;
        }
    }

    public class CachedObjectCollection<T>
    {
        private Dictionary<string, CachedObject<T>> _objects 
            = new Dictionary<string, CachedObject<T>>();

        private IRequestHandler<T> _handler;
        private int _timeoutSec = int.MaxValue;

        public T Get(string id)
        {
            if (_objects.ContainsKey(id))
            {
                return _objects[id].Get();
            }
            else
            {
                var obj = new CachedObject<T>(_handler, id);
                obj.TimeoutSec = _timeoutSec;
                _objects.Add(id, obj);
                DataCache.CacheCount++;
                return obj.Get();
            }
        }

        public CachedObjectCollection(IRequestHandler<T> handler)
        {
            _handler = handler;
        }

        public CachedObjectCollection(IRequestHandler<T> handler, int timeoutSec)
        {
            _handler = handler;
            _timeoutSec = timeoutSec;
        }
    }

    public interface IAPIRequest
    {
        string Url { get; }
        string ExceptionString { get; }
    }

    public class BiliAPIRequest : IAPIRequest
    {
        public string BaseUrl { get; set; }
        public Dictionary<string, string> Args { get;set; }
        public string Url 
        {
            get
            {
                var result = BaseUrl;
                if (BaseUrl.Contains("?") && Args.Count > 0) result += "&";
                if (!BaseUrl.Contains("?") && Args.Count > 0) result += "?";
                foreach (var arg in Args)
                {
                    result += arg.Key + "=" + arg.Value;
                }
                return result;
            }
        }
        public string ExceptionString { get; set; }
    }
}
