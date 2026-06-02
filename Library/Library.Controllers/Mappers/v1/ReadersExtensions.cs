using Library.Contracts.v1.Reader.Request;
using Library.Contracts.v1.Reader.Response;
using Library.Dto.Output;
using Library.Models;

namespace Library.Controllers.Mappers.v1;

public static class ReadersExtensions
{
    public static Reader ToReader(this CreateReaderRequest request) =>
        new()
        {
            FullName = request.FullName,
            PhoneNumber = request.PhoneNumber,
            ExpiryDate = request.ExpiryDate
        };
    
    public static BorrowedBookResponse ToBorrowedBookResponse(this BorrowedBookDto dto) =>
        new(
            dto.BookId, 
            dto.BorrowDate, 
            dto.DueDate
            );
}