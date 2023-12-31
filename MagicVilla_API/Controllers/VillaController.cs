﻿using AutoMapper;
using MagicVilla_API.Datos;
using MagicVilla_API.Modelos;
using MagicVilla_API.Modelos.Dto;
using MagicVilla_API.Repositorio.IRepositorio;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace MagicVilla_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VillaController : ControllerBase
    {
        private readonly ILogger<VillaController> _logger;
        //private readonly ApplicationDbContext _context;
        private readonly IVillaRepositorio _villaRepo;
        private readonly IMapper _mapper;
        protected APIResponse _response;


        public VillaController(ILogger<VillaController> logger,
            IVillaRepositorio villaRepo, IMapper mapper)
        {
            _logger = logger;
            this._villaRepo = villaRepo;
            this._mapper = mapper;
            _response = new();
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        //public async Task<ActionResult<IEnumerable<VillaDto>>> GetVillas()
        public async Task<ActionResult<APIResponse>> GetVillas()
        {
            try
            {
                _logger.LogInformation("Obtener las villas");
                IEnumerable<Villa> villaList = await _villaRepo.ObtenerTodos();
                _response.Resultado = _mapper.Map<IEnumerable<VillaDto>>(villaList);
                _response.statusCode = HttpStatusCode.OK;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsExitoso = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
            }
            return _response;
        }

        [HttpGet("id:int", Name = "GetVilla")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        //public async Task<ActionResult<VillaDto>> GetVilla(int id)
        public async Task<ActionResult<APIResponse>> GetVilla(int id)
        {
            try
            {
                if (id == 0)
                {
                    _logger.LogError("Error al traer villa con id= " + id);
                    _response.statusCode = HttpStatusCode.BadRequest;
                    _response.IsExitoso = false;
                    return BadRequest(_response);
                }
                //var villa = VillaStore.villaList.FirstOrDefault(v => v.Id == id);
                var villa = await _villaRepo.Obtener(v => v.Id == id);
                if (villa == null)
                {
                    _response.IsExitoso = false;
                    _response.statusCode = HttpStatusCode.NotFound;
                    return NotFound(_response);
                }
                _response.Resultado = _mapper.Map<VillaDto>(villa);
                _response.statusCode = HttpStatusCode.OK;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsExitoso = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
            }
            return _response;
        }


        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        //public async Task<ActionResult<VillaDto>> CrearVilla([FromBody] VillaCreateDto createDto)
            public async Task<ActionResult<APIResponse>> CrearVilla([FromBody] VillaCreateDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                if (await _villaRepo.Obtener(v => v.Nombre.ToLower() == createDto.Nombre.ToLower()) != null)
                {
                    ModelState.AddModelError("NombreExiste", "la vista con ese Nombre ya existe");
                    return BadRequest(ModelState);
                }
                if (createDto == null)
                {
                    return BadRequest(createDto);
                }
                //if (villaDto.Id > 0)
                //{
                //    return StatusCode(StatusCodes.Status500InternalServerError);
                //}
                Villa modelo = _mapper.Map<Villa>(createDto);
                modelo.FechaCreacion = DateTime.Now;
                modelo.FechaActualizacion = DateTime.Now;
                await _villaRepo.Crear(modelo); // dentro de Crear está SaveChangesAsync                                                   
                _response.Resultado = modelo;
                _response.statusCode = HttpStatusCode.Created;
                return CreatedAtRoute("GetVilla", new { id = modelo.Id }, _response);
            }
            catch (Exception ex)
            {
                _response.IsExitoso = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
            }
            return _response;
        }


        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        //public async Task<IActionResult> DeleteVilla(int id)
            public async Task<IActionResult> DeleteVilla(int id)
        {    //IActionResult xq retorna NoContent, no retorna contenido
            try
            {
                if (id == 0)
                {
                    _response.IsExitoso = false;
                    _response.statusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }
                var villa = await _villaRepo.Obtener(v => v.Id == id);
                if (villa == null)
                {
                    _response.IsExitoso = false;
                    _response.statusCode = HttpStatusCode.NotFound;
                    return NotFound(_response);
                }
                await _villaRepo.Remover(villa); //Remove NO es Async
                _response.statusCode = HttpStatusCode.NoContent;                                           
                return Ok(_response); // xq no retorna contenido
            }
            catch (Exception ex)
            {
                _response.IsExitoso = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
            }
            return BadRequest(_response);
        }


        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateVilla(int id, [FromBody] VillaUpdateDto updateDto)
        {
            if (updateDto == null || id != updateDto.Id)
            {
                _response.IsExitoso = false;
                _response.statusCode= HttpStatusCode.BadRequest;
                return BadRequest(_response);
            }
            Villa modelo = _mapper.Map<Villa>(updateDto);
            await _villaRepo.Actualizar(modelo); //Upd no es Async   En Actualizar esta SaveChangesAsync            
            _response.statusCode=HttpStatusCode.NoContent;
            return Ok(_response);
        }


        [HttpPatch("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdatePartialVilla(int id, JsonPatchDocument<VillaUpdateDto> patchDto)
        {
            if (patchDto == null || id == 0)
            {
                return BadRequest();
            }
            var villa = await _villaRepo.Obtener(v => v.Id == id, tracked: false);
            // AsNoTracking para no cause conflicto al actualizar el regisrto
            VillaUpdateDto villaDto = _mapper.Map<VillaUpdateDto>(villa);
            //VillaUpdateDto villaDto = new()
            //{
            //    Id = villa.Id,
            //    Nombre = villa.Nombre,  
            //    Amenidad = villa.Amenidad
            //};
            if (villa is null)
                return BadRequest();
            patchDto.ApplyTo(villaDto, ModelState);
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            Villa modelo = _mapper.Map<Villa>(villaDto);
            //Villa modelo = new()
            //{
            //    Id = villaDto.Id,             
            //    Amenidad = villaDto.Amenidad
            //};
            await _villaRepo.Actualizar(modelo);
            _response.statusCode=HttpStatusCode.NoContent;
            return Ok(_response);
        }

    }
}
