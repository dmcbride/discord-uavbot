using uav.logic.Database;
using uav.logic.Models;
using uav.logic.Constants;
using System;
using uav.logic.Database.Model;
using System.Linq;
using MathNet.Numerics;
using uav.IPM.Core;

namespace uav.logic.Service;

public class Ark
{
    private DatabaseService databaseService = new DatabaseService();
    private Credits creditService = new Credits();
    
    public string QueryCreditRangeNew(GV gv, UserConfig? config)
    {
        if (gv < 10_000_000)
        {
            return $@"Invalid GV ""{gv}"" - it must be at least 10M.";
        }

        if (gv > 1e219)
        {
            gv = GV.FromNumber(1e219);
        }

        var (totalCredits, expectedCredits, loungeBonus, station, exodus) = CalculateCredits(gv, config);

        if (totalCredits > 0)
        {
            return $"A GV of {gv} would get {totalCredits:N0} {IpmEmoji.ipmCredits} (including {expectedCredits:N0} base credits, {loungeBonus:N0} {IpmEmoji.ipmCredits} from the lounge, {station:N0} {IpmEmoji.ipmCredits} from the station, and {exodus:N0} {IpmEmoji.ipmCredits} from the Exodus).";
        }
        return $"A GV of {gv} would get {expectedCredits:N0} {IpmEmoji.ipmCredits}. If you submitted your config with `/register`, I could estimate your total credits.";
    }

    private (int totalCredits, int baseCredits, int loungeBonus, int station, int exodus) CalculateCredits(GV gv, UserConfig? config)
    {
        var baseCredits = creditService.CreditsForGv(gv);
        var (totalCredits, loungeBonus, station, exodus) = config?.CalculateCredits(baseCredits) ?? (0, 0, 0, 0);
        return (totalCredits, baseCredits, loungeBonus, station, exodus);
    }

    private static Random rng = new Random();

    // spell-checker: disable
    private readonly static string[] thankYous = new[] {
        $"{IpmEmoji.partying_face}{IpmEmoji.partying_face}{IpmEmoji.partying_face}",
        $"{IpmEmoji.tada}",
        $"Amazeballs!",
        $"Awesome!",
        $"Bonus!",
        $"Dynamite!",
        $"Excellent contribution.",
        $"Gadzooks!",
        $"Great job!",
        $"Ho-ho!",
        $"Holy Guacamole!",
        $"Holy pretzel!",
        $"Holy Shnikies!",
        $"Hurrah!",
        $"Huzzah!",
        $"I'll be damned!",
        $"Informative!",
        $"Jeepers creepers.",
        $"Jiminy Crickets.",
        $"Jinkies!",
        $"Nice!",
        $"Olé!",
        $"Om nom nom.",
        $"Son of a gun!",
        $"Spot on.",
        $"Thank you for feeding the algorithm.",
        $"That's gonna help, for sure!",
        $"Va-va-voom!",
        $"W00t!",
        $"Well, I'll be jitterbugged.",
        $"Whoopee!",
        $"Winner winner chicken dinner!",
        $"Wow such data!",
        $"Wowie zowie!",
        $"Wowzers!",
        $"Yee-haw!",
        $"Yesssss!",
        $"Yippee!",
        $"You did it!",
        $"Zoinks!",
    };
    // spell-checker: enable

    private static string ThankYou => thankYous[(int)(rng.NextDouble() * thankYous.Length)];

    public async Task<(string message, bool success)> UpdateCredits(string gvInput, string creditInput, IDbUser user)
    {
        if (!int.TryParse(creditInput, out var credits))
        {
            return ($@"Credit value of ""{creditInput}"" provided for GV ""{gvInput}"" isn't a number - did you put a GV value instead?", false);
        }

        return await UpdateCredits(gvInput, credits, user);
    }

    public async Task<(string message, bool success)> UpdateCredits(string gvInput, int credits, IDbUser user)
    {
        if (!GV.TryFromString(gvInput, out var gv, out var error))
        {
            return ($@"Invalid GV ""{gvInput}""{(error != null ? $"\n{error}" : string.Empty)}.", false);
        }

        return await UpdateCredits(gv, credits, user);
    }

    public async Task<(string message, bool success)> UpdateCredits(GV gv, int credits, IDbUser user)
    {            
        if (gv < 10_000_000 || gv > 1e109)
        {
            return ($@"""{gv}"" is not between 10M and 1E+109.", false);
        }

        var creditService = new Credits();
        var expectedMinimumCredits = creditService.TierCredits(gv);
        var expectedMaximumCredits = creditService.TierCredits(gv, 1);
        if (credits < expectedMinimumCredits || expectedMaximumCredits < credits)
        {
            return (@$"The given credits of {credits} for a GV of ""{gv}"" lies outside the expected range for tier {gv.TierNumber} of {expectedMinimumCredits} - {expectedMaximumCredits}. If this is incorrect, please send a screen-cap to Tanktalus showing this.", false);
        }

        if (Math.Log10(gv) == (int)Math.Log10(gv))
        {
            return ($"Thank you for the input.  However, {gv} is the base value for tier {gv.TierNumber}, and is already known with precision.", false);
        }

        try
        {
            var value = new ArkValue
            {
                Base_Credits = credits,
                Gv = gv,
                user_id = user.User_Id,
            };
            var (min, max) = creditService.TierRange(gv);
            var (success, atThisCredit, inTier) = await databaseService.AddArkValue(value, min, max, user);

            return ($"{ThankYou} {(success ? "Recorded" : "Previously recorded")} that your current GV of **{gv}** gives base credits of **{credits}**. There are now **{inTier}** report(s) in tier {gv.TierNumber} and **{atThisCredit}** report(s) for this base credit value.", true);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        return ("Unknown error.", false);
    }

    public GV GVRequiredForCredits(int credits, UserConfig userConfig)
    {
        if (credits < 10)
        {
            return GV.Zero;
        }

        // Set this up via binary search. Probably would be better using Newton's method, but co-pilot didn't want to do it easily enough.
        var prestige = new Prestige();

        var low = 10_000_000d;
        var high = 1e219;
        var tries = 0;

        while (tries < 200)
        {
            var mid = Math.Exp((Math.Log(low) + Math.Log(high)) / 2);
            var midCredits = CalculateCredits(GV.FromNumber(mid), userConfig).totalCredits;
            //Console.Error.WriteLine($"low: {low}, high: {high}, mid: {mid}, midCredits: {midCredits}");
            if (midCredits == credits)
            {
                Console.Error.WriteLine($"Found it: {mid}");
                return GV.FromNumber(mid);
            }
            if (midCredits < credits)
            {
                low = mid;
            }
            else
            {
                high = mid;
            }
            tries++;
        }

        return GV.Zero;
    }

    public async Task<GV> GVRequiredForCredits(int credits)
    {
        var nearArkValues = (await databaseService.FindValueByCredit(credits)).ToArray();

        var matchingArkValues = nearArkValues.Where(v => v.Base_Credits == credits);
        if (matchingArkValues.Any())
        {
            return GV.FromNumber(matchingArkValues.First().Gv);
        }

        var organisedByTier = nearArkValues.GroupBy(v => (int)Math.Floor(Math.Log10(v.Gv))).ToDictionary(x => x.Key, x => x.OrderBy(x => x.Gv).ToArray());
        var tiersToCheck = organisedByTier.Keys.OrderBy(x => x);

        // find the first tier that might match, then try to extrapolate.
        foreach (var tier in tiersToCheck)
        {
            var data = organisedByTier[tier];
            if (credits < data.Last().Base_Credits && credits > data.First().Base_Credits)
            {
                if (data.Length < 3)
                {
                    break;
                }

                var xData = data.Select(d => d.Gv).ToArray();
                var yData = data.Select(d => (double)d.Base_Credits).ToArray();

                var (b, m) = Fit.Line(xData, yData);
                // so now we have credits = m * gv + b, thus to get gv from credits, we need gv = (credits - b) / m
                var gv = (credits - b) / m;

                return GV.FromNumber(gv);
            }
        }

        return GV.Zero; // probably not enough data
    }
}