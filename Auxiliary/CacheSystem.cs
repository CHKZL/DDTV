using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Auxiliary
{
    public class Pipeline<TIn, TOut>
    {
        private List<IPipe> _steps = new List<IPipe>();

        public void AddStep(IPipe step)
        {
            _steps.Add(step);
        }

        public TOut Accept(TIn initialObject)
        {
            object input = initialObject;
            object output = null;
            foreach (var s in _steps)
            {
                output = s.Invoke(input);
                input = output;
            }
            return (TOut)output;
        }
    }

    public class CachedObject<T>
    {
        private IAPIRequest _apiRequest;
        private Pipeline<IAPIRequest, T> _pipeline;

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
            var result = _pipeline.Accept(_apiRequest);
            _value = result;
            LastUpdateTime = DateTime.Now;
            return result;
        }

        public CachedObject(IAPIRequest apiRequest, Pipeline<IAPIRequest, T> pipeline)
        {
            _apiRequest = apiRequest;
            _pipeline = pipeline;
            TimeoutSec = int.MaxValue;
        }
    }

    public class CachedObjectCollection<T>
    {
        private Dictionary<string, CachedObject<T>> _objects = new Dictionary<string, CachedObject<T>>();

        private IAPIRequestBuilder _builder;
        private Pipeline<IAPIRequest, T> _pipeline;
        private int _timeoutSec = int.MaxValue;

        public T Get(string name)
        {
            if (_objects.ContainsKey(name))
            {
                return _objects[name].Get();
            }
            else
            {
                var obj = new CachedObject<T>(_builder.Build(name), _pipeline);
                obj.TimeoutSec = _timeoutSec;
                _objects.Add(name, obj);
                DataCache.BilibiliApiCount++;
                return obj.Get();
            }
        }

        public CachedObjectCollection(IAPIRequestBuilder builder, Pipeline<IAPIRequest, T> pipeline)
        {
            _builder = builder;
            _pipeline = pipeline;
        }

        public CachedObjectCollection(IAPIRequestBuilder builder, Pipeline<IAPIRequest, T> pipeline, int timeoutSec)
        {
            _builder = builder;
            _pipeline = pipeline;
            _timeoutSec = timeoutSec;
        }
    }

    public interface IAPIRequestBuilder
    {
        IAPIRequest Build(string args);
    }

    public enum APIPlatforms
    {
        Bilibili,
        Youtube
    }

    public interface IAPIRequest
    {
        APIPlatforms Platform { get; }
        string Url { get; }
        int ID { get; }
        WebClient WebClient { get; }
        string ExceptionString { get; }
    }

    public class APIRequest : IAPIRequest
    {
        public APIPlatforms Platform { get; set; }
        public string Url { get; set; }
        public int ID { get; set; }
        public WebClient WebClient { get; set; }
        public string ExceptionString { get; set; }
    }

    public class BiliAPIRequest : IAPIRequest
    {
        public APIPlatforms Platform { get; set; }
        public string BaseUrl { get; set; }
        public int ID 
        { 
            get
            {
                if (int.TryParse(Args.Values.FirstOrDefault(), out int result)) return result;
                else return -1;
            }
        }
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
        public WebClient WebClient { get; set; }
        public string ExceptionString { get; set; }
    }

    public interface IPipe
    {
        object Invoke(object input);
    }

    public class APIRequestToResponseJObjectPipe : IPipe
    {
        public object Invoke(object input)
        {
            var request = (IAPIRequest)input;
            byte[] responseBytes;
            try
            {
                responseBytes = request.WebClient.DownloadData(request.Url);
            }
            catch (Exception e)
            {
                InfoLog.InfoPrintf(request.ID + request.ExceptionString + ": " + 
                    e.Message, InfoLog.InfoClass.Debug);
                return null;
            }
            var responseString = Encoding.UTF8.GetString(responseBytes);
            var jsonObject = JObject.Parse(responseString);
            return jsonObject;
        }
    }

    public class APIRequestToResponseJObjectWithArgPipe : IPipe
    {
        public object Invoke(object input)
        {
            var request = (IAPIRequest)input;
            byte[] responseBytes;
            try
            {
                responseBytes = request.WebClient.DownloadData(request.Url);
            }
            catch (Exception e)
            {
                InfoLog.InfoPrintf(request.ID + request.ExceptionString + ": " +
                    e.Message, InfoLog.InfoClass.Debug);
                return null;
            }
            var responseString = Encoding.UTF8.GetString(responseBytes);
            var jsonObject = JObject.Parse(responseString);
            jsonObject.Add("@@", request.ID);
            return jsonObject;
        }
    }
}
