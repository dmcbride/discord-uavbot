using System;
using System.Collections.Generic;
using uav.logic.Database.Model;
using uav.logic.Models;
using uav.logic.Service;
using uav.test.Assertions;

namespace uav.test.ArkTests;

public class ArkCalculationTests
{
    [Test]
    [MethodDataSource(typeof(ArkCalculationTests), nameof(TestCalculateGvFromCreditsData))]
    public async Task ArkShouldCalculateGvFromCredits(int credits, GV expectedGV)
    {
        var userConfig = new UserConfig
        {
            // snapshot of my config.
            LoungeLevel = 48,
            HasExodus = true,
            Credits1 = 5,
            Credits2 = 4,
            Credits3 = 4,
            Credits4 = 4,
            Credits5 = 4,
            Credits6 = 6,
            Credits7 = 1,
            Credits8 = 6,
            Credits9 = 2,
            Credits10 = 0,
            Credits11 = 0,
            Credits12 = 1,
            Credits13 = 1
        };

        var arkService = new Ark();
        var gv = arkService.GVRequiredForCredits(credits, userConfig);
        await Assert.That(gv).IsGv(expectedGV);
    }

    public static IEnumerable<Func<(int credits, GV expectedGV)>> TestCalculateGvFromCreditsData()
    {
        yield return () => (25250, GV.FromString("37.15S"));
        yield return () => (25642, GV.FromString("52.44S"));
        yield return () => (680, GV.FromString("899M"));
        //yield return () => (5966, 100_000_000);
        //yield return () => (0, GV.Zero);
    }
}