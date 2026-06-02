using Microsoft.AspNetCore.Http;

namespace Library.Contracts.v1.Books.Request;

/// <summary>
/// Запрос на добавления деталей по книге
/// </summary>
/// <param name="CoverImage">Изображение обложки книги</param>
/// <param name="Description">Краткое описание книги</param>
public sealed record AddBookDetailsRequest(IFormFile CoverImage, string Description);