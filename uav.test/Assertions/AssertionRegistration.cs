using System.Runtime.CompilerServices;
using TUnit.Assertions.AssertConditions.Interfaces;
using TUnit.Assertions.AssertionBuilders;
using uav.logic.Models;

namespace uav.test.Assertions;

public static class AssertionRegistration
{
    public static InvokableValueAssertionBuilder<GV> IsGv(this IValueSource<GV> valueSource, GV expectedValue, [CallerArgumentExpression(nameof(expectedValue))] string? doNotPassIn1 = null)
    {
        return valueSource.RegisterAssertion(
            assertCondition: new IsGv(expectedValue),
            argumentExpressions: [doNotPassIn1]
        );
    }
}