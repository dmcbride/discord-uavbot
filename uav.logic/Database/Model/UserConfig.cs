using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace uav.logic.Database.Model;

public class UserConfig : IDapperMappedType
{
    private static IReadOnlyList<decimal> creditRoomMultiplierPerLevel = [
        0.036m, // room 1
        0.04m,  // room 2
        0.04m,  // room 3
        0.005m, // room 4
        0.005m, // room 5
        0.005m, // room 6
        0.06m,  // room 7
        0.005m, // room 8
        0.02m,  // room 9
        0.02m,  // room 10
        0.03m,  // room 11
        0.03m,  // room 12
        0.07m,  // room 13
    ];

    [Description("user_id")]
    public ulong UserId { get; set; }
    [Description("lounge_level")]
    public int LoungeLevel { get; set; } = 0;
    [Description("has_exodus")]
    public bool HasExodus { get; set; } = false;
    [Description("credits_1")]
    public int Credits1 { get; set; } = 0;
    [Description("credits_2")]
    public int Credits2 { get; set; } = 0;
    [Description("credits_3")]
    public int Credits3 { get; set; } = 0;
    [Description("credits_4")]
    public int Credits4 { get; set; } = 0;
    [Description("credits_5")]
    public int Credits5 { get; set; } = 0;
    [Description("credits_6")]
    public int Credits6 { get; set; } = 0;
    [Description("credits_7")]
    public int Credits7 { get; set; } = 0;
    [Description("credits_8")]
    public int Credits8 { get; set; } = 0;
    [Description("credits_9")]
    public int Credits9 { get; set; } = 0;
    [Description("credits_10")]
    public int Credits10 { get; set; } = 0;
    [Description("credits_11")]
    public int Credits11 { get; set; } = 0;
    [Description("credits_12")]
    public int Credits12 { get; set; } = 0;
    [Description("credits_13")]
    public int Credits13 { get; set; } = 0;

    // lounge is 15% plus 5% per additional level of the lounge, if there is any lounge level.
    private decimal LoungeMultiplier => LoungeLevel > 0 ? .1m + .05m * LoungeLevel : 0;

    // station is the sum of all the credits rooms 1-8 times their respective multipliers.
    private decimal StationMultiplier => 
            Credits1 * creditRoomMultiplierPerLevel[0] +
            Credits2 * creditRoomMultiplierPerLevel[1] +
            Credits3 * creditRoomMultiplierPerLevel[2] +
            Credits4 * creditRoomMultiplierPerLevel[3] +
            Credits5 * creditRoomMultiplierPerLevel[4] +
            Credits6 * creditRoomMultiplierPerLevel[5] +
            Credits7 * creditRoomMultiplierPerLevel[6] +
            Credits8 * creditRoomMultiplierPerLevel[7] +
            Credits9 * creditRoomMultiplierPerLevel[8] +
            Credits10 * creditRoomMultiplierPerLevel[9] +
            Credits11 * creditRoomMultiplierPerLevel[10] +
            Credits12 * creditRoomMultiplierPerLevel[11] +
            Credits13 * creditRoomMultiplierPerLevel[12]
            ;

    // exodus is doubling everything above.
    private decimal ExodusMultiplier => HasExodus ? 1 : 0;

    public (int totalCredits, int loungeBonus, int station, int exodus) CalculateCredits(int baseCredits)
    {
        var loungeBonus = (int)Math.Round(baseCredits * LoungeMultiplier, MidpointRounding.ToEven);

        var station = (int)Math.Round((baseCredits + loungeBonus) * StationMultiplier, MidpointRounding.ToEven);

        var exodus = (int)Math.Round((baseCredits + loungeBonus + station) * ExodusMultiplier, MidpointRounding.ToEven);

        // total is all three together with the base.
        var totalCredits = baseCredits + loungeBonus + station + exodus;

        return (totalCredits, loungeBonus, station, exodus);
    }

    public (decimal LoungeMultiplier, decimal StationMultiplier, decimal ExodusMultiplier) GetMultipliers()
    {
        return (LoungeMultiplier + 1, StationMultiplier + 1, ExodusMultiplier + 1);
    }

    // same as above, but in reverse
    public int CalculateBaseCredits(int totalCredits)
    {
        // remove exodus.
        totalCredits = (int)Math.Round(totalCredits / (1 + ExodusMultiplier));

        // remove station.
        totalCredits = (int)Math.Round(totalCredits / (1 + StationMultiplier));

        // remove lounge.
        totalCredits = (int)Math.Round(totalCredits / (1 + LoungeMultiplier));

        return totalCredits;
    }

    public override string ToString()
    {
        return $"UserConfig: {UserId} - Lounge {LoungeLevel} - Exodus {HasExodus} - Credits {Credits1},{Credits2},{Credits3},{Credits4},{Credits5},{Credits6},{Credits7},{Credits8},{Credits9},{Credits10},{Credits11},{Credits12},{Credits13}";
    }
}