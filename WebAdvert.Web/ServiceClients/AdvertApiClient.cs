using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using WebAdvert.AdvertApi.Models;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Net.Http.Json;
using Amazon.ServiceDiscovery;
using Amazon.ServiceDiscovery.Model;

namespace WebAdvert.Web.ServiceClients
{
    public class AdvertApiClient : IAdvertApiClient
    {
        private readonly string _baseAddress;
        private readonly IConfiguration _config;
        private readonly HttpClient _client;
        private readonly IMapper _mapper;

        public AdvertApiClient(IConfiguration config, HttpClient client, IMapper mapper)
        {
            _config = config;
            _client = client;
            _mapper = mapper;
            _baseAddress = _config["AdvertApi:BaseUrl"];

            var discoveryService = new AmazonServiceDiscoveryClient();
            var response = discoveryService.DiscoverInstancesAsync(new DiscoverInstancesRequest
            {
                ServiceName = "advertapi-nohealthcheck", // name of service in namespace in cloudmap;
                NamespaceName = "WebAdvertisement"       // name of namespace in cloudmap;
            });
            var instances = response.GetAwaiter().GetResult().Instances; // need to randomize becaise cloudmap does not suppurt LB

            if (instances.Count > 0)
            {
                var ipv4 = instances[0].Attributes["AWS_INSTANCE_IPV4"];
                var port = instances[0].Attributes["AWS_INSTANCE_PORT"];

                _baseAddress = new UriBuilder(Uri.UriSchemeHttp, ipv4, Int32.Parse(port), "api/advert/v1").Path;
            }

        }

        public async Task<AdvertResponse> CreateAsync(CreateAdvertModel model)
        {
            var advertApiModel = _mapper.Map<AdvertModel>(model);

            var jsonModel = JsonConvert.SerializeObject(advertApiModel);
            
            var response = await _client.PostAsync(
                new Uri($"{_baseAddress}/create"),
                new StringContent(jsonModel, Encoding.UTF8, "application/json"));

            var createAdvertResponse = await response.Content.ReadFromJsonAsync<CreateAdvertResponse>();
            var advertResponse = _mapper.Map<AdvertResponse>(createAdvertResponse);

            return advertResponse;
        }

        public async Task<bool> ConfirmAsync(ConfirmAdvertRequest model)
        {
            var advertModel = _mapper.Map<ConfirmAdvertModel>(model);
            
            var jsonModel = JsonConvert.SerializeObject(advertModel);
            
            var response = await _client.PutAsync(
                new Uri($"{_baseAddress}/confirm"),
                new StringContent(jsonModel, Encoding.UTF8, "application/json"));

            return response.StatusCode == HttpStatusCode.OK;
        }

        public async Task<List<Advertisement>> GetAllAsync()
        {
            var apiCallResponse = await _client.GetAsync(new Uri($"{_baseAddress}/all"));
            var allAdvertModels = await apiCallResponse.Content.ReadFromJsonAsync<List<AdvertModel>>();
            return allAdvertModels.Select(x => _mapper.Map<Advertisement>(x)).ToList();
        }

        public async Task<Advertisement> GetAsync(string advertId)
        {
            var apiCallResponse = await _client.GetAsync(new Uri($"{_baseAddress}/{advertId}"));
            var fullAdvert = await apiCallResponse.Content.ReadFromJsonAsync<AdvertModel>();
            return _mapper.Map<Advertisement>(fullAdvert);
        }
    }
}