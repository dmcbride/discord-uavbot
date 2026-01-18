using System.Runtime.CompilerServices;
using TUnit.Assertions.Core;
using uav.logic.Models;

namespace uav.test.Assertions;

public static class AssertionRegistration
{
    public static IsGvAssertion IsGv(
        this IAssertionSource<GV> source,
        GV expectedValue,
        [CallerArgumentExpression(nameof(expectedValue))] string? expression = null)
    {
        source.Context.ExpressionBuilder.Append($".IsGv({expression})");
        return new IsGvAssertion(source.Context, expectedValue);
    }
}