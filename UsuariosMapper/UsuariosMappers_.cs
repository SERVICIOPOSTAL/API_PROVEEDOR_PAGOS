using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApiProveedorPagos.Models;
using WebApiProveedorPagos.Models.Dtos;

namespace WebApiProveedorPagos
{
    public class UsuariosMappers : Profile
    {
        public UsuariosMappers()
        {
           
            CreateMap<Usuario, UsuarioDto>().ReverseMap();
        }
    }
}
