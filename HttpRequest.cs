using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Reflection;

namespace RestApiHelper
{
    // enum to determine type of request
    public enum RequestType
    {
        GET,
        POST
    };
    // create the class when you need to make an http request
    public class HttpRequest
	{
        // http request fields
        string path;
		string userAgent;
		//CookieCollection cookies;
		private int timeout;
        private HttpClient _client;

		// contructor for an http request using Uri as string
		public HttpRequest(string path, string userAgent, int timeout, HttpClient client)
		{
			// set Uri and agent
			this.path = path;
			this.userAgent = userAgent;
			this.timeout = timeout;
            
            _client = client;
		}
        		
		// prepare for response 
		string PrepareRequest( RequestType type, object param = null)
		{
            string result = "Error";

			// prepare request based on type
            if (type == RequestType.GET)
			{
                var getP = param == null ? "" : objectToGET(param);

                var task = _client.GetStringAsync(path + getP);
				task.Wait();

                result = task.Result;

			}
			else if (type == RequestType.POST)
			{
				var strData = JsonConvert.SerializeObject(param);
				
				var task = _client.PostAsync(path, new StringContent(strData, Encoding.UTF8, "application/json" ));
				task.Wait();
                task.Result.EnsureSuccessStatusCode();
				var task1 = task.Result.Content.ReadAsStringAsync();
                task1.Wait();
                result = task1.Result;				
			}
            			
			return result;
		}
        		
		// will perform request
		public T SendRequest<T>( RequestType requestType, object Object = null) where T : class
		{
            var response = PrepareRequest(requestType, Object);

			// if reponse was successful then get response contents
			if (response != "Error")
			{
				// get contents as string
				string strContents = response;

				// if string is empty send empty JObject
				if (!string.IsNullOrEmpty(strContents))
				{
					// attempt to deserialize string to JObject
					try
					{
                        var result = JsonConvert.DeserializeObject<T>(strContents);
                        return result;
					}
					catch (JsonException e)
					{
						// rethrow exception
						throw e;
					}
				}
			}
			else // throw an exception with status code
				throw new WebException($"Error");

			// error
			return null;
		}

		// run send request method async
		public Task<T> SendRequestAsync<T>( RequestType requestType, object Object = null) where T : class
		{
			// have this run on as a different task
			return Task.Run(() =>
			{
				return SendRequest<T>( requestType, Object);
			});
		}

		//Convert an object to url GET request format
		private static string objectToGET(object obj)
		{
			string result = "?";

			var type = obj.GetType();
            var properties = type.GetTypeInfo().DeclaredProperties;

			foreach (var property in properties)
			{
				if (property.CanRead)
				{
					result += property.Name.ToLower();
					result += "=";
					var value = property.GetValue(obj, null);
					if (value == null)
						result += "null";
					else
						result += value.ToString().ToLower();
					result += "&";
				}
			}

			return result;

		}
	}
}
