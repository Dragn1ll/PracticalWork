using Asp.Versioning;
using Library.Abstraction.Services;
using Library.Contracts.v1.Books.Request;
using Library.Contracts.v1.Books.Response;
using Library.Controllers.Mappers.v1;
using Microsoft.AspNetCore.Mvc;

namespace Library.Controllers.Api.v1;

[ApiController]
[ApiVersion("1")]
[Route("api/v{version:apiVersion}/books")]
public class BooksController : ControllerBase
{
    private readonly IBookService _bookService;

    public BooksController(IBookService bookService)
    {
        _bookService = bookService;
    }

    /// <summary>Создание новой книги</summary>
    [HttpPost]
    [Produces("application/json")]
    [ProducesResponseType(typeof(CreateBookResponse), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> CreateBook([FromBody] CreateBookRequest request)
    {
        var result = await _bookService.CreateBook(request.ToBook());

        return Content(result.ToString());
    }

    /// <summary>Обновление данных книги</summary>
    [HttpPut("{id:guid}")]
    [Produces("application/json")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> UpdateBook([FromRoute] Guid id, [FromBody] UpdateBookRequest request)
    {
        await _bookService.UpdateBook(id, request.ToUpdateBookDto());
        
        return Ok();
    }

    /// <summary>Перевод книги в архив</summary>
    [HttpPost("{id:guid}/archive")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ArchiveBookResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> ArchiveBook([FromRoute] Guid id)
    {
        var result = await _bookService.ArchiveBook(id);
        
        return Ok(result.ToArchiveBookResponse());
    }
    
    /// <summary>Получение списка книг</summary>
    [HttpGet]
    [Produces("application/json")]
    [ProducesResponseType(typeof(IList<BookListResponse>), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetBooks([FromQuery] GetBookListRequest request)
    {
        var result = await _bookService.GetBooks(request.ToGetBookListDto());

        return Ok(new PagedListResponse<BookListResponse>(
            result.Items.Select(bl => bl.ToBookListResponse()).ToList(),
            result.Page,
            result.PageSize,
            result.TotalCount,
            result.TotalPages
            ));
    }
    
    /// <summary>Добавление деталей книг</summary>
    [HttpPost("{id:guid}/details")]
    [ProducesResponseType(201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> AddBookDetails([FromRoute] Guid id, [FromForm] AddBookDetailsRequest request)
    {
        await _bookService.CreateBookDetails(id, request.CoverImage, request.Description);
        
        return Ok();
    }
}