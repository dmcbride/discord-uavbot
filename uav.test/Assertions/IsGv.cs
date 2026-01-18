using System;
using TUnit.Assertions.Core;
using uav.logic.Models;

namespace uav.test.Assertions;

public class IsGvAssertion : Assertion<GV>
{
    private readonly GV _expectedValue;
    private const double Epsilon = 0.01; // 1% tolerance

    public IsGvAssertion(AssertionContext<GV> context, GV expectedValue)
        : base(context)
    {
        _expectedValue = expectedValue;
    }

    protected override Task<AssertionResult> CheckAsync(EvaluationMetadata<GV> metadata)
    {
        var actualValue = metadata.Value;
        var exception = metadata.Exception;

        if (exception != null)
        {
            return Task.FromResult(AssertionResult.Failed($"threw {exception.GetType().Name}"));
        }

        if (actualValue == null && _expectedValue == null)
        {
            return Task.FromResult(AssertionResult.Passed);
        }

        if (actualValue == null)
        {
            return Task.FromResult(AssertionResult.Failed("actual GV is null"));
        }

        if (_expectedValue == null)
        {
            return Task.FromResult(AssertionResult.Failed("expected GV is null"));
        }

        if (Math.Abs(actualValue - _expectedValue) / _expectedValue < Epsilon)
        {
            return Task.FromResult(AssertionResult.Passed);
        }

        return Task.FromResult(AssertionResult.Failed($"expected GV to be {_expectedValue} but was {actualValue}"));
    }

    protected override string GetExpectation()
    {
        return $"to be {_expectedValue}";
    }
}

