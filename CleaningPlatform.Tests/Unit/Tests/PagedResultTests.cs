using CleaningPlatformAPI.Common;
using FluentAssertions;
using Xunit;

namespace CleaningPlatform.Tests.Unit.Tests;

public class PagedResultTests
{
    [Fact]
    public void TotalPages_WithExactDivision_ReturnsCorrectCount()
    {
        var result = PagedResult<int>.From(new List<int> { 1, 2, 3, 4, 5 }, 20, 1, 5);
        result.TotalPages.Should().Be(4);
    }

    [Fact]
    public void TotalPages_WithRemainder_RoundsUp()
    {
        var result = PagedResult<int>.From(new List<int>(), 21, 1, 5);
        result.TotalPages.Should().Be(5);
    }

    [Fact]
    public void TotalPages_ZeroPageSize_ReturnsZero()
    {
        var result = PagedResult<int>.From(new List<int>(), 0, 1, 0);
        result.TotalPages.Should().Be(0);
    }

    [Fact]
    public void HasPreviousPage_FirstPage_ReturnsFalse()
    {
        var result = PagedResult<int>.From(new List<int>(), 50, 1, 10);
        result.HasPreviousPage.Should().BeFalse();
    }

    [Fact]
    public void HasPreviousPage_SecondPage_ReturnsTrue()
    {
        var result = PagedResult<int>.From(new List<int>(), 50, 2, 10);
        result.HasPreviousPage.Should().BeTrue();
    }

    [Fact]
    public void HasNextPage_LastPage_ReturnsFalse()
    {
        var result = PagedResult<int>.From(new List<int>(), 50, 5, 10);
        result.HasNextPage.Should().BeFalse();
    }

    [Fact]
    public void HasNextPage_MiddlePage_ReturnsTrue()
    {
        var result = PagedResult<int>.From(new List<int>(), 50, 3, 10);
        result.HasNextPage.Should().BeTrue();
    }

    [Fact]
    public void From_SetsPropertiesCorrectly()
    {
        var items = new List<string> { "a", "b" };
        var result = PagedResult<string>.From(items, 100, 3, 25);

        result.Items.Should().Equal(items);
        result.TotalCount.Should().Be(100);
        result.Page.Should().Be(3);
        result.PageSize.Should().Be(25);
    }

    [Fact]
    public void TotalPages_EmptyItems_ReturnsOne()
    {
        var result = PagedResult<int>.From(new List<int>(), 0, 1, 10);
        result.TotalPages.Should().Be(0);
    }
}
