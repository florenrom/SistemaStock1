using AutoMapper;
using NuevoSistemaNat.Models.Entities;
using NuevoSistemaNat.Models.ViewModels;


namespace NuevoSistemaNat.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Movimiento, MovimientoViewModel>();
            CreateMap<MovimientoViewModel, Movimiento>();
        }
    }
}
