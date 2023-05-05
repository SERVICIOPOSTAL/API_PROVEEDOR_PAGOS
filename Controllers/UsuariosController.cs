using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using WebApiProveedorPagos.Models;
using WebApiProveedorPagos.Models.Dtos;
using WebApiProveedorPagos.Repository.IRepository;

namespace WebApiProveedorPagos.Controllers
{
    
    [Route("api/Usuarios")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "ApiUsuarios")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public class UsuariosController : Controller
    {
        private readonly IUsuarioRepository _userRepo;
        private readonly IMapper _mapper;
        private readonly IConfiguration _config;

        public UsuariosController(IUsuarioRepository userRepo, IMapper mapper, IConfiguration config)
        {
            _userRepo = userRepo;
            _mapper = mapper;
            _config = config;
        }

     

        /// <summary>
        /// Acceso/Autenticación de usuario
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("Login")]
        public IActionResult Login(UsuarioAuthLoginDto usuarioAuthLoginDto)
        {
            //throw new Exception("Error generado");

            var usuarioDesdeRepo = _userRepo.Login(usuarioAuthLoginDto.Usuario, usuarioAuthLoginDto.Password);

            if (usuarioDesdeRepo == null)
            {
                return Unauthorized();
            }

            var claims = new[]
            {
            new Claim(ClaimTypes.NameIdentifier, usuarioDesdeRepo.Id.ToString()),
            new Claim(ClaimTypes.Name, usuarioDesdeRepo.UsuarioA.ToString())
        };

            //Generación de token
            
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetSection("AppSettings:Token").Value));
            var credenciales = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                
                Subject = new ClaimsIdentity(claims),
                NotBefore = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddMinutes(60),
                SigningCredentials = credenciales
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            
            return Ok(new
            {
                token = tokenHandler.WriteToken(token)

            });
        }
    }   
    
}