﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using GameMarketAPIServer.Configuration;
using Newtonsoft.Json;
using GameMarketAPIServer.Models;
using Microsoft.Extensions.Options;
using GameMarketAPIServer.Utilities;
using Microsoft.Extensions.Logging;

namespace GameMarketAPIServer.Services
{
    public interface IAPIManager
    {
        Task<string> CallAPIAsync(int apiCall, string paramaters = "");
    }

    public abstract class APIManager<TSchema> : IAPIManager where TSchema : ISchema
    {
        protected readonly IHttpClientFactory clientFactory;
        protected readonly HttpClient httpClient;
        protected readonly TSchema schema;
        protected readonly MainSettings settings;
        protected readonly ILogger<APIManager<TSchema>> logger;

        public const string errorURLNoParam = "Error, missing paramater";
        public const string errorURLCallDoesntExist = "Error, API call does not exist";
        public const string httpRequestFail = "requestFailed";
        public const string checkHeaders = "CheckForHeaders";
        public const string noPayload = "No PayloadGiven";


        protected bool running = false;
        protected CancellationTokenSource mainCTS = new CancellationTokenSource();
        protected Task mainTask;

        protected APIManager( IOptions<MainSettings> settings, ILogger<APIManager<TSchema>> apiLogger,
            TSchema schema, IHttpClientFactory httpClientFactory)
        {
            // this.settings = settings;
            this.settings = settings.Value;
            httpClient = httpClientFactory.CreateClient();
            CreateDefaultHeaders(settings);
            this.logger = apiLogger;

            this.schema = schema;
        }
        public virtual void Start()
        {
            running = true;

            logger.LogTrace($"{schema.GetName()} is starting");
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
            else if (url == errorURLNoParam)
            {
                return false;
            }
            else if (url == errorURLCallDoesntExist)
            {
                logger.LogDebug("Error");
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
                    logger.LogInformation("\n" + url);
                }


                HttpRequestMessage request;
                string payload = PostPayload(apiCall, paramaters);
                if (payload == noPayload)
                    request = new HttpRequestMessage(HttpMethod.Get, url);
                else
                {
                    if (settings.outputHTTP) { logger.LogDebug(payload + "\n"); }
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
                logger.LogError(ex.ToString());
                return httpRequestFail;
            }

        }
        //public abstract Task<(bool,List<ITableData>)> ParseJsonAsyncOld(int apiCall, string json);

        public abstract Task<(bool, ICollection<ITable>?)> ParseJsonAsync(int apiCall, string json);



        protected abstract void CreateDefaultHeaders(IOptions<MainSettings> settings);
        protected abstract void AddAdditionalHeaders(HttpRequestMessage request, int apiCall, string paramaters, string payload);
        protected abstract void HandleHttpClientHeaders(HttpHeaders headers, int apiCall, string paramaters);

    }
}
