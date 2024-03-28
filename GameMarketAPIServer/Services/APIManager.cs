using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using GameMarketAPIServer.Configuration;
using Newtonsoft.Json;
using GameMarketAPIServer.Models;
using Microsoft.Extensions.Options;

namespace GameMarketAPIServer.Services
{
    public interface IAPIManager
    {
        Task<string> CallAPIAsync(int apiCall, string paramaters = "");
    }

    public abstract class APIManager : IAPIManager
    {
        protected readonly HttpClient httpClient;
        protected readonly IDataBaseManager dbManager;
        protected readonly MainSettings settings;

        public const string errorURLNoParam = "Error, missing paramater";
        public const string errorURLCallDoesntExist = "Error, API call does not exist";
        public const string httpRequestFail = "requestFailed";
        public const string checkHeaders = "CheckForHeaders";
        public const string noPayload = "No PayloadGiven";
        public string managerName;


        protected bool running = false;
        protected CancellationTokenSource mainCTS = new CancellationTokenSource();
        protected Task mainTask;

        protected APIManager(IDataBaseManager dbManager, IOptions<MainSettings> settings, string managerName)
        {
            // this.settings = settings;
            this.settings = settings.Value;
            this.dbManager = dbManager;
            this.managerName = managerName;
            httpClient = new HttpClient();
            CreateDefaultHeaders(settings);

        }
        public virtual void Start()
        {
            running = true;
            Console.WriteLine(managerName + " is starting");
            mainTask = Task.Run(RunAsync);
        }
        public virtual void Stop() 
        {
            running = false;
            mainTask?.Wait();
        }
        protected abstract Task RunAsync();



        public abstract string ToStringAsync(int apiCall);
        public abstract string GetUrlAsync(int apiCall, string paramaters = "");

        public bool ValidURL(string url)
        {
            if (url == null) 
            { 
                return false; 
            }
            else if (url.Length == 0) 
            { 
                return false;
            }
            else if  (url == errorURLNoParam) 
            { 
                return false; 
            }
            else if (url == errorURLCallDoesntExist) 
            {
                Console.WriteLine("Error");
                return false; 
            }

            return true;

        }

        protected virtual string PostPayload(int apiCall, string paramaters)
        {
            return noPayload;
        }

        public virtual async Task<string> CallAPIAsync(int apiCall, string paramaters = "")
        {
            try
            {
                string url = GetUrlAsync(apiCall, paramaters);

                if (!ValidURL(url))
                {
                    return url;
                }
                if (settings.outputHTTP)
                {
                    Console.WriteLine("\n" + url);
                }


                HttpRequestMessage request;
                string payload = PostPayload(apiCall, paramaters);
                if (payload == noPayload)
                     request = new HttpRequestMessage(HttpMethod.Get, url);
                else
                {
                    if (settings.outputHTTP) { Console.WriteLine(payload + "\n"); }
                    request = new HttpRequestMessage(HttpMethod.Post, url)
                    {
                        Content = new StringContent(payload, System.Text.Encoding.UTF8, "application/json")
                    };
                }
                AddAdditionalHeaders(request, apiCall, paramaters, payload);

                using var response = await httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                HandleHttpClientHeaders(response.Headers, apiCall, paramaters);

                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return httpRequestFail;
            }

        }
        public abstract Task<(bool,List<TableData>)> ParseJsonAsync(int apiCall, string json);



        protected abstract void CreateDefaultHeaders(IOptions<MainSettings>settings);
        protected abstract void AddAdditionalHeaders(HttpRequestMessage request, int apiCall, string paramaters, string payload);
        protected abstract void HandleHttpClientHeaders(HttpHeaders headers, int apiCall, string paramaters);

        
    }
}
