using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using WebAdvert.Web.Models;
using WebAdvert.Web.Models.Home;
using WebAdvert.Web.ServiceClients;

namespace WebAdvert.Web.Controllers
{
    //[Authorize]
    public class HomeController : Controller
    {
        private readonly ISearchApiClient _searchApiClient;
        private readonly IAdvertApiClient _advertApiClient;
        private readonly IMapper _mapper;

        public HomeController(IMapper mapper, ISearchApiClient searchApiClient, IAdvertApiClient advertApiClient)
        {
            _mapper = mapper;
            _searchApiClient = searchApiClient;
            _advertApiClient = advertApiClient;
        }

        //[Authorize]
        public async Task<IActionResult> Index()
        {
            //var allAds = await _advertApiClient.GetAllAsync();
            //var allViewModels = allAds.Select(x => _mapper.Map<IndexViewModel>(x));

            //return View(allViewModels);
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Search(string keyword)
        {
            var viewModel = new List<SearchViewModel>();

            var searchResult = await _searchApiClient.Search(keyword);
            searchResult.ForEach(advertDoc =>
            {
                var viewModelItem = _mapper.Map<SearchViewModel>(advertDoc);
                viewModel.Add(viewModelItem);
            });

            return View(viewModel);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
