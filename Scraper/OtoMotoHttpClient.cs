﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scraper
{
    public interface IOtoMotoHttpClient
    {
        Task<HttpResponseMessage> Get(string url);
    }

    public class OtoMotoHttpClient : IOtoMotoHttpClient
    {
        private readonly HttpClient _httpClient;

        public OtoMotoHttpClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<HttpResponseMessage> Get(string url)
        {
            return await _httpClient.GetAsync(url);
        }
    }
}
