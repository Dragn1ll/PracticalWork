using Library.Models;

namespace Library.UnitTests.Models;

public class ReaderTests
{
    private static Reader CreateActiveReader(int expiryDaysFromNow = 30) => new()
    {
        FullName = "Иван Иванов",
        PhoneNumber = "+79001234567",
        Email = "ivan@test.com",
        IsActive = true,
        ExpiryDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(expiryDaysFromNow))
    };
 
    [Fact]
    public void CanDoSomething_WhenActiveAndNotExpired_ReturnsTrue()
    {
        var reader = CreateActiveReader();
        
        Assert.True(reader.CanDoSomething());
    }
 
    [Fact]
    public void CanDoSomething_WhenInactive_ReturnsFalse()
    {
        var reader = CreateActiveReader();
        
        reader.IsActive = false;
        
        Assert.False(reader.CanDoSomething());
    }
 
    [Fact]
    public void CanDoSomething_WhenExpired_ReturnsFalse()
    {
        var reader = new Reader
        {
            IsActive = true,
            ExpiryDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1))
        };
        
        Assert.False(reader.CanDoSomething());
    }
 
    [Fact]
    public void CanDoSomething_WhenExpiryIsToday_ReturnsTrue()
    {
        var reader = new Reader
        {
            IsActive = true,
            ExpiryDate = DateOnly.FromDateTime(DateTime.UtcNow)
        };
        
        Assert.True(reader.CanDoSomething());
    }
    
    [Fact]
    public void UpdateExpiryDate_WhenActiveAndValidDate_UpdatesDate()
    {
        var reader = CreateActiveReader();
        var newDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1));
 
        reader.UpdateExpiryDate(newDate);
 
        Assert.Equal(newDate, reader.ExpiryDate);
    }
 
    [Fact]
    public void UpdateExpiryDate_WhenInactive_ThrowsInvalidOperation()
    {
        var reader = new Reader { IsActive = false, ExpiryDate = DateOnly.FromDateTime(DateTime.UtcNow) };
        
        Assert.Throws<InvalidOperationException>(() =>
            reader.UpdateExpiryDate(DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1))));
    }
 
    [Fact]
    public void UpdateExpiryDate_WhenExpired_ThrowsInvalidOperation()
    {
        var reader = new Reader
        {
            IsActive = true,
            ExpiryDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1))
        };
        
        Assert.Throws<InvalidOperationException>(() =>
            reader.UpdateExpiryDate(DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1))));
    }
 
    [Fact]
    public void DeActiveReader_WhenActive_SetsIsActiveFalse()
    {
        var reader = CreateActiveReader();
        
        reader.DeActiveReader();
        
        Assert.False(reader.IsActive);
    }
 
    [Fact]
    public void DeActiveReader_WhenActive_SetsExpiryDateToToday()
    {
        var reader = CreateActiveReader();
        
        reader.DeActiveReader();
        
        Assert.Equal(DateOnly.FromDateTime(DateTime.UtcNow), reader.ExpiryDate);
    }
 
    [Fact]
    public void DeActiveReader_WhenAlreadyInactive_ThrowsInvalidOperation()
    {
        var reader = new Reader { IsActive = false };
        
        Assert.Throws<InvalidOperationException>(() => reader.DeActiveReader());
    }
}