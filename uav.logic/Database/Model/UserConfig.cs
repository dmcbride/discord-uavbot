using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace uav.logic.Database.Model;

public class UserConfig : IDapperMappedType
{
    private static IReadOnlyList<double> creditRoomMultiplierPerLevel = new[] {
        0.036, // room 1
        0.04,  // room 2
        0.04,  // room 3
        0.005, // room 4
        0.005, // room 5
        0.005, // room 6
        0.06,  // room 7
        0.005, // room 8
    };

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

    // lounge is 15% plus 5% per additional level of the lounge, if there is any lounge level.
    private double loungeMultiplier => LoungeLevel > 0 ? .1 + .05 * LoungeLevel : 0;

    // station is the sum of all the credits rooms 1-8 times their respective multipliers.
    private double stationMultiplier => 
            Credits1 * creditRoomMultiplierPerLevel[0] +
            Credits2 * creditRoomMultiplierPerLevel[1] +
            Credits3 * creditRoomMultiplierPerLevel[2] +
            Credits4 * creditRoomMultiplierPerLevel[3] +
            Credits5 * creditRoomMultiplierPerLevel[4] +
            Credits6 * creditRoomMultiplierPerLevel[5] +
            Credits7 * creditRoomMultiplierPerLevel[6] +
            Credits8 * creditRoomMultiplierPerLevel[7];

    // exodus is doubling everything above.
    private double exodusMultiplier => HasExodus ? 1 : 0;

    public (int totalCredits, int loungeBonus, int station, int exodus) CalculateCredits(int baseCredits)
    {
        var loungeBonus = (int)Math.Round(baseCredits * loungeMultiplier);
        
        var station = (int)Math.Round((baseCredits + loungeBonus) * stationMultiplier);

        var exodus = HasExodus ? (baseCredits + loungeBonus + station) : 0;

        // total is all three together with the base.
        var totalCredits = baseCredits + loungeBonus + station + exodus;

        return (totalCredits, loungeBonus, station, exodus);
    }

    // same as above, but in reverse
    public int CalculateBaseCredits(int totalCredits)
    {
        // remove exodus.
        totalCredits = (int)Math.Round(totalCredits / (1 + exodusMultiplier));

        // remove station.
        totalCredits = (int)Math.Round(totalCredits / (1 + stationMultiplier));

        // remove lounge.
        totalCredits = (int)Math.Round(totalCredits / (1 + loungeMultiplier));

        return totalCredits;
    }
}