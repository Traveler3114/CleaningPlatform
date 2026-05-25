using CleaningPlatformAPI.Common;
using FluentAssertions;
using Xunit;

namespace CleaningPlatform.Tests.Unit.Tests;

public class OperationResultTests
{
    [Fact]
    public void Ok_SetsSuccessTrue_AndData()
    {
        var result = OperationResult<string>.Ok("hello");
        result.Success.Should().BeTrue();
        result.Data.Should().Be("hello");
        result.Message.Should().BeNull();
    }

    [Fact]
    public void Fail_SetsSuccessFalse_AndMessage()
    {
        var result = OperationResult<int>.Fail("something went wrong");
        result.Success.Should().BeFalse();
        result.Message.Should().Be("something went wrong");
        result.Data.Should().Be(0);
    }

    [Fact]
    public void Ok_WithNullData_AllowsNull()
    {
        var result = OperationResult<object>.Ok(null!);
        result.Success.Should().BeTrue();
        result.Data.Should().BeNull();
    }
}
