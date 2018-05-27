using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using RestApiHelper;
using System.Net.Http;
using System.Threading.Tasks;

namespace RestApiHelper
{
    public class Endpoint<T, P> where T : class where P : class
    {
        protected RequestType requestType;
        protected HttpClient httpClient;
        protected int timeout;
        protected string endpointUrl;
        protected string userAgent;
        protected string bearelToken = null;

        public Endpoint(
            RequestType requestType, 
            HttpClient httpClient,
            int timeout,
            string endpointUrl, 
            string userAgent)
        {            
            this.requestType = requestType;
            this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            this.endpointUrl = endpointUrl ?? throw new ArgumentNullException(nameof(endpointUrl));
            this.userAgent = userAgent ?? throw new ArgumentNullException(nameof(userAgent));
            this.timeout = timeout;
        }

        public void SetBearerToken(string token)
        {
            bearelToken = token;
        }

        //Send the request and return the result a single object result
        public async Task<T>GetSingleData( P parameters )
        {
            var request = new HttpRequest(endpointUrl, userAgent, timeout, httpClient, bearelToken);
            var result = await request.SendRequestAsync<T>(requestType, parameters);
            return result;
        }

        //Send the request and return the result multiple objects result
        public async Task<T[]>GetMultipleData( P parameters )
        {
            var request = new HttpRequest(endpointUrl, userAgent, timeout, httpClient, bearelToken);
            var result = await request.SendRequestAsync<T[]>(requestType, parameters);
            return result;
        }
    }
}
