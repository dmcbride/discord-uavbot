using System;
using TUnit.Assertions.AssertConditions;
using uav.logic.Models;

namespace uav.test.Assertions;

public class IsGv : ExpectedValueAssertCondition<GV, GV>
{
    public IsGv(GV expectedValue) : base(expectedValue) { }

    protected override string GetExpectation()
    {
        return $"Expected GV to be {this.ExpectedValue}";
    }

    protected override ValueTask<AssertionResult> GetResult(GV? actualValue, GV? expectedValue)
    {
        if (actualValue == null && expectedValue == null)
        {
            return AssertionResult.Passed;
        }

        if (actualValue == null)
        {
            return AssertionResult.Fail($"actual GV is null");
        }

        if (expectedValue == null)
        {
            return AssertionResult.Fail($"expected GV is null");
        }

        if (Math.Abs(actualValue - expectedValue) / expectedValue < 0.01)
        {
            return AssertionResult.Passed;
        }

        return AssertionResult.Fail($"expected GV to be {expectedValue} but was {actualValue}");
    }
}

