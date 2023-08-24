using AutoMapper;
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
    public class NumeroVillaController : ControllerBase
    {
        private readonly ILogger<NumeroVillaController> _logger;
        //private readonly ApplicationDbContext _context;
        private readonly IVillaRepositorio _villaRepo;
        private readonly INumeroVillaRepositorio _numeroRepo;
        private readonly IMapper _mapper;
        protected APIResponse _response;


        public NumeroVillaController(ILogger<NumeroVillaController> logger,
            IVillaRepositorio villaRepo, INumeroVillaRepositorio numeroRepo, IMapper mapper)
        {
            _logger = logger;
            this._villaRepo = villaRepo;
            this._numeroRepo = numeroRepo;
            this._mapper = mapper;
            _response = new();
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        //public async Task<ActionResult<IEnumerable<VillaDto>>> GetVillas()
        public async Task<ActionResult<APIResponse>> GetNumeroVillas()
        {
            try
            {
                _logger.LogInformation("Obtener Numero de villas");
                IEnumerable<NumeroVilla> numeroVillaList = await _numeroRepo.ObtenerTodos();
                _response.Resultado = _mapper.Map<IEnumerable<NumeroVillaDto>>(numeroVillaList);
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


        [HttpGet("id:int", Name = "GetNumeroVilla")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        //public async Task<ActionResult<VillaDto>> GetVilla(int id)
        public async Task<ActionResult<APIResponse>> GetNumeroVilla(int id)
        {
            try
            {
                if (id == 0)
                {
                    _logger.LogError("Error al traer Numero villa con id= " + id);
                    _response.statusCode = HttpStatusCode.BadRequest;
                    _response.IsExitoso = false;
                    return BadRequest(_response);
                }
                //var villa = VillaStore.villaList.FirstOrDefault(v => v.Id == id);
                var numeroVilla = await _numeroRepo.Obtener(v => v.VillaNo == id);
                if (numeroVilla == null)
                {
                    _response.IsExitoso = false;
                    _response.statusCode = HttpStatusCode.NotFound;
                    return NotFound(_response);
                }
                _response.Resultado = _mapper.Map<NumeroVillaDto>(numeroVilla);
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
            public async Task<ActionResult<APIResponse>> CrearNumeroVilla([FromBody] NumeroVillaCreateDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                if (await _numeroRepo.Obtener(v => v.VillaNo == createDto.VillaNo) != null)
                {
                    ModelState.AddModelError("NombreExiste", "El Numero de Villa  ya existe");
                    return BadRequest(ModelState);
                }
                if (await _villaRepo.Obtener(v=>v.Id==createDto.VillaId) ==null)
                {
                    ModelState.AddModelError("ClaveForanea", "El ID de la Villa no existe");
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
                NumeroVilla modelo = _mapper.Map<NumeroVilla>(createDto);
                modelo.FechaCreacion = DateTime.Now;
                modelo.FechaActualizacion = DateTime.Now;
                await _numeroRepo.Crear(modelo); // dentro de Crear está SaveChangesAsync                                                   
                _response.Resultado = modelo;
                _response.statusCode = HttpStatusCode.Created;
                return CreatedAtRoute("GetNumeroVilla", new { id = modelo.VillaNo }, _response);
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
            public async Task<IActionResult> DeleteNumeroVilla(int id)
        {    //IActionResult xq retorna NoContent, no retorna contenido
            try
            {
                if (id == 0)
                {
                    _response.IsExitoso = false;
                    _response.statusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }
                var numeroVilla = await _numeroRepo.Obtener(v => v.VillaNo == id);
                if (numeroVilla == null)
                {
                    _response.IsExitoso = false;
                    _response.statusCode = HttpStatusCode.NotFound;
                    return NotFound(_response);
                }
                await _numeroRepo.Remover(numeroVilla); //Remove NO es Async
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
        public async Task<IActionResult> UpdateNumeroVilla(int id, [FromBody] NumeroVillaUpdateDto updateDto)
        {
            if (updateDto == null || id != updateDto.VillaNo)
            {
                _response.IsExitoso = false;
                _response.statusCode= HttpStatusCode.BadRequest;
                return BadRequest(_response);
            }
            if (await _villaRepo.Obtener(v=>v.Id==updateDto.VillaId) == null)
            {
                ModelState.AddModelError("ClaveForanea", "El Id de la Villa No existe");
                return BadRequest(ModelState);
            }
            NumeroVilla modelo = _mapper.Map<NumeroVilla>(updateDto);
            await _numeroRepo.Actualizar(modelo); //Upd no es Async   En Actualizar esta SaveChangesAsync            
            _response.statusCode=HttpStatusCode.NoContent;
            return Ok(_response);
        }


        //[HttpPatch("{id:int}")]
        //[ProducesResponseType(StatusCodes.Status204NoContent)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        //public async Task<IActionResult> UpdatePartialVilla(int id, JsonPatchDocument<VillaUpdateDto> patchDto)
        //{
        //    if (patchDto == null || id == 0)
        //    {
        //        return BadRequest();
        //    }
        //    var villa = await _villaRepo.Obtener(v => v.Id == id, tracked: false);
        //    // AsNoTracking para no cause conflicto al actualizar el regisrto
        //    VillaUpdateDto villaDto = _mapper.Map<VillaUpdateDto>(villa);
        //    //VillaUpdateDto villaDto = new()
        //    //{
        //    //    Id = villa.Id,
        //    //    Nombre = villa.Nombre,  
        //    //    Amenidad = villa.Amenidad
        //    //};
        //    if (villa is null)
        //        return BadRequest();
        //    patchDto.ApplyTo(villaDto, ModelState);
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }
        //    Villa modelo = _mapper.Map<Villa>(villaDto);
        //    //Villa modelo = new()
        //    //{
        //    //    Id = villaDto.Id,             
        //    //    Amenidad = villaDto.Amenidad
        //    //};
        //    await _villaRepo.Actualizar(modelo);
        //    _response.statusCode=HttpStatusCode.NoContent;
        //    return Ok(_response);
        //}

    }
}
