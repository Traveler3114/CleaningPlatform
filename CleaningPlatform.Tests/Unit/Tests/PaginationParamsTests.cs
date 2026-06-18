using CleaningPlatformAPI.Models;
using FluentAssertions;
using Xunit;

namespace CleaningPlatform.Tests.Unit.Tests;

public class PaginationParamsTests
{
    [Fact]
    public void Page_ClampsToMinimumOne()
    {
        var p = new PaginationParams { Page = 0 };
        p.Page.Should().Be(1);
    }

    [Fact]
    public void Page_Negative_ClampsToOne()
    {
        var p = new PaginationParams { Page = -5 };
        p.Page.Should().Be(1);
    }

    [Fact]
    public void Page_ValidValue_StaysAsIs()
    {
        var p = new PaginationParams { Page = 3 };
        p.Page.Should().Be(3);
    }

    [Fact]
    public void PageSize_DefaultsToFifty()
    {
        var p = new PaginationParams();
        p.PageSize.Should().Be(50);
    }

    [Fact]
    public void PageSize_CapsAtMax()
    {
        var p = new PaginationParams { PageSize = 500 };
        p.PageSize.Should().Be(200);
    }

    [Fact]
    public void PageSize_Negative_ResetsToDefault()
    {
        var p = new PaginationParams { PageSize = -1 };
        p.PageSize.Should().Be(50);
    }

    [Fact]
    public void Skip_ComputesCorrectly()
    {
        var p = new PaginationParams { Page = 3, PageSize = 25 };
        p.Skip.Should().Be(50);
    }

    [Fact]
    public void Take_EqualsPageSize()
    {
        var p = new PaginationParams { PageSize = 10 };
        p.Take.Should().Be(10);
    }
}
