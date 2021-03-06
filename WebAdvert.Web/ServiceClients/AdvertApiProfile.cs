using WebAdvert.AdvertApi.Models;
using AutoMapper;
using WebAdvert.Web.Models.AdvertManagement;
using WebAdvert.Web.Models.Home;

namespace WebAdvert.Web.ServiceClients
{
    public class AdvertApiProfile : Profile
    {
        public AdvertApiProfile()
        {
            CreateMap<AdvertModel, CreateAdvertModel>().ReverseMap();
            CreateMap<CreateAdvertViewModel, CreateAdvertModel>().ReverseMap();
            CreateMap<CreateAdvertResponse, AdvertResponse>().ReverseMap();
            CreateMap<ConfirmAdvertRequest, ConfirmAdvertModel>().ReverseMap();
            CreateMap<AdvertModel, Advertisement>().ReverseMap();
            CreateMap<Advertisement, IndexViewModel>()
                .ForMember(dest => dest.Image, opt => opt.MapFrom(src => src.FilePath))
                .ReverseMap();
        }
    }
}