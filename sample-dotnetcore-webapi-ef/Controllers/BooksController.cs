using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using sample_dotnetcore_webapi_ef_infrastructure.Entities;
using sample_dotnetcore_webapi_ef_services.Dtos;
using sample_dotnetcore_webapi_ef_services.Repositories;

namespace sample_dotnetcore_webapi_ef.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {

        private readonly IBookRepository _bookRepository;
        private readonly IMapper _mapper;

        public BooksController(IBookRepository repository, IMapper mapper)
        {
            _bookRepository = repository;
            _mapper = mapper;            
        }

        [HttpGet(Name = nameof(GetAll))]
        public IActionResult GetAll(bool? read = null)
        {
            List<Book> items = _bookRepository.GetAll(read).ToList();

            IEnumerable<BookDto> toReturn = items.Select(x => _mapper.Map<BookDto>(x));

            return Ok(toReturn);
        }

        [HttpGet]
        [Route("{id:int}", Name = nameof(GetSingle))]
        public IActionResult GetSingle(int id)
        {
            Book item = _bookRepository.GetSingle(id);

            if (item == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<BookDto>(item));
        }

        [HttpPost(Name = nameof(Add))]
        public ActionResult<BookDto> Add([FromBody] BookCreateDto bookCreateDto)
        {
            if (bookCreateDto == null)
            {
                return BadRequest();
            }

            //Book toAdd = _mapper.Map<Book>(bookCreateDto);

            Book toAdd = new Book { Author = bookCreateDto.Author, Description = bookCreateDto.Description, Genre = bookCreateDto.Genre, Read = bookCreateDto.Read, Title = bookCreateDto.Title };

            _bookRepository.Add(toAdd);

            if (!_bookRepository.Save())
            {
                throw new Exception("Creating an item failed on save.");
            }

            Book newItem = _bookRepository.GetSingle(toAdd.Id);

            return CreatedAtRoute(nameof(GetSingle), new { id = newItem.Id },
                _mapper.Map<BookDto>(newItem));
        }

        [HttpPatch("{id:int}", Name = nameof(PartiallyUpdate))]
        public ActionResult<BookDto> PartiallyUpdate(int id, [FromBody] JsonPatchDocument<BookUpdateDto> patchDoc)
        {
            if (patchDoc == null)
            {
                return BadRequest();
            }

            Book existingEntity = _bookRepository.GetSingle(id);

            if (existingEntity == null)
            {
                return NotFound();
            }

            BookUpdateDto bookUpdateDto = _mapper.Map<BookUpdateDto>(existingEntity);
            patchDoc.ApplyTo(bookUpdateDto);

            TryValidateModel(bookUpdateDto);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _mapper.Map(bookUpdateDto, existingEntity);
            Book updated = _bookRepository.Update(id, existingEntity);

            if (!_bookRepository.Save())
            {
                throw new Exception("Updating an item failed on save.");
            }

            return Ok(_mapper.Map<BookDto>(updated));
        }

        [HttpDelete]
        [Route("{id:int}", Name = nameof(Remove))]
        public IActionResult Remove(int id)
        {
            Book item = _bookRepository.GetSingle(id);

            if (item == null)
            {
                return NotFound();
            }

            _bookRepository.Delete(id);

            if (!_bookRepository.Save())
            {
                throw new Exception("Deleting an item failed on save.");
            }

            return NoContent();
        }

        [HttpPut]
        [Route("{id:int}", Name = nameof(Update))]
        public ActionResult<BookDto> Update(int id, [FromBody] BookUpdateDto updateDto)
        {
            if (updateDto == null)
            {
                return BadRequest();
            }

            var item = _bookRepository.GetSingle(id);

            if (item == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            //_mapper.Map(updateDto, item);

            item.Author = updateDto.Author;
            item.Description = updateDto.Description;
            item.Genre = updateDto.Genre;
            item.Read = updateDto.Read;
            item.Title = updateDto.Title;
            
            _bookRepository.Update(id, item);

            if (!_bookRepository.Save())
            {
                throw new Exception("Updating an item failed on save.");
            }

            return Ok(_mapper.Map<BookDto>(item));
        }

    }
}