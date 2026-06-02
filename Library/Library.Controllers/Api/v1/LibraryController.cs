using Asp.Versioning;
using Library.Abstraction.Services;
using Library.Contracts.v1.Books.Response;
using Library.Contracts.v1.Library.Requests;
using Library.Contracts.v1.Library.Response;
using Library.Contracts.v1.Reader.Response;
using Library.Controllers.Mappers.v1;
using Microsoft.AspNetCore.Mvc;

namespace Library.Controllers.Api.v1;

[ApiController]
[ApiVersion("1")]
[Route("api/v{version:apiVersion}/library")]
public class LibraryController : ControllerBase
{
    private readonly ILibraryService _libraryService;

    public LibraryController(ILibraryService libraryService)
    {
        _libraryService = libraryService;
    }
    
    /// <summary>Выдача книги</summary>
    [HttpPost("borrow")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(BorrowedBookResponse), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> BorrowBook([FromBody] BorrowBookRequest request)
    {
        var result = await _libraryService.BorrowBook(request.BookId, request.ReaderId);

        return Content(result.ToString());
    }

    /// <summary>Получение списка книг библиотеки</summary>
    [HttpGet("books")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(IList<LibraryBookResponse>), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetLibraryBooks([FromQuery] GetLibraryBooksRequest request)
    {
        var result = await _libraryService.GetLibraryBooks(request.ToGetLibraryBooksDto());
        
        return Ok(new PagedListResponse<LibraryBookResponse>(
            result.Items.Select(bl => bl.ToLibraryBookResponse()).ToList(),
            result.Page,
            result.PageSize,
            result.TotalCount,
            result.TotalPages
        ));
    }

    /// <summary>Возврат книги</summary>
    [HttpPost("return")]
    [Produces("application/json")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> ReturnBook([FromBody] ReturnBookRequest request)
    {
        await _libraryService.ReturnBook(request.BookId);
        
        return Ok();
    }

    /// <summary>Получение детальной информации о книге</summary>
    [HttpGet("{id:guid}/details")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(BookDetailsResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetBookDetailsById([FromRoute] Guid id)
    {
        var result = await _libraryService.GetBookDetailsById(id);
        
        return Ok(result.ToBookDetailsResponse());
    }
    
    /// <summary>Получение детальной информации о книге</summary>
    [HttpGet("{title}/details")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(BookDetailsResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetBookDetailsByTitle([FromRoute] string title)
    {
        var result = await _libraryService.GetBookDetailsByTitle(title);
        
        return Ok(result.ToBookDetailsResponse());
    }
}