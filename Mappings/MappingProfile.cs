using AutoMapper;
using NuevoSistemaNat.Models.Entities;
using NuevoSistemaNat.Models.ViewModels;


namespace NuevoSistemaNat.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Movimiento, MovimientoViewModel>()
            .ForMember(dest => dest.ProductoNombre,
                opt => opt.MapFrom(src => src.Producto != null ? src.Producto.PrNombre : null));
            CreateMap<MovimientoViewModel, Movimiento>();
        }
    }
}
