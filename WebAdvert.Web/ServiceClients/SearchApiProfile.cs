using AutoMapper;
using WebAdvert.Web.Models.Home;
using WebAdvert.Web.Models;

namespace WebAdvert.Web.ServiceClients
{
    public class SearchApiProfile : Profile
    {
        public SearchApiProfile()
        {
            CreateMap<AdvertType, SearchViewModel>().ReverseMap();
        }
    }
}