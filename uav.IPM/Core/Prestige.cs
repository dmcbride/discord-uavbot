namespace uav.IPM.Core;

// This file is a subset of the actual IPM code as donated by Jefferson.
// I have tried to replicate with less weird code, but have been unable
// to replicate it simply with bug-for-bug compatibility, so I've given
// up and just included the original code that I need here.

public class Prestige
{
    public Homebase homebase;
    public int prestigeTier;
    public StationProjects station = new();


    public int totalPrestigeRewards;
    public int prestigePointsGalaxyCurrent;
    public int prestigePointsBase;
    public int prestigePointsExodus;
    public int prestigePointsStation;
    public int prestigePointsLounge;

    public int prestigeRewardsUnlocked;
    public int prestigeNewRewardCost;

    //Partial calcs
    public double valueDifference;
    public double prestigePointsDifference;
    public double partialRatio;
    public double[] valueMax = new double[215];
    public double[] valueMin = new double[215];
    public double[] prestigeTierValueBase = new double[215];

    public Prestige(double gv) : this()
    {
        homebase = new Homebase(gv);
    }

    public Prestige()
    {
        homebase = new Homebase(0);
        PrestigeInitialization();
    }

    public int CalculateBaseCredits(double gv)
    {
        homebase.galaxyTotalValue = gv;
        CalculateGalaxyValue();
        return prestigePointsBase;
    }

    public (double floorGv, double ceilingGv) FindTierRangeFor(int credits)
    {
        // Find the tier that the credits fall into using binary search
        int low = 0;
        int high = 214;
        while (low < high)
        {
            int mid = (low + high) / 2;
            if (prestigeTierValueBase[mid] < credits)
            {
                low = mid + 1;
            }
            else
            {
                high = mid;
            }
        }
        return (valueMin[low], valueMax[low]);
    }

    public void PrestigeInitialization()
    {
        valueMin[1] = 1E7f;
        valueMin[2] = 1E8f;
        valueMin[3] = 1E9f;
        valueMin[4] = 1E10f;
        valueMin[5] = 1E11f;
        valueMin[6] = 1E12f;
        valueMin[7] = 1E13f;
        valueMin[8] = 1E14f;
        valueMin[9] = 1E15f;
        valueMin[10] = 1E16f;
        valueMin[11] = 1E17f;
        valueMin[12] = 1E18f;
        valueMin[13] = 1E19f;
        valueMin[14] = 1E20f;
        valueMin[15] = 1E21f;
        valueMin[16] = 1E22f;
        valueMin[17] = 1E23f;
        valueMin[18] = 1E24f;
        valueMin[19] = 1E25f;
        valueMin[20] = 1E26f;
        valueMin[21] = 1E27f;
        valueMin[22] = 1E28f;
        valueMin[23] = 1E29f;
        valueMin[24] = 1E30f;
        valueMin[25] = 1E31f;
        valueMin[26] = 1E32f;
        valueMin[27] = 1E33f;
        valueMin[28] = 1E34f;
        valueMin[29] = 1E35d;
        valueMin[30] = 1E36d;
        valueMin[31] = 1E37d;
        valueMin[32] = 1E38d;
        valueMin[33] = 1E39d;
        valueMin[34] = 1E40d;
        valueMin[35] = 1E41d;
        valueMin[36] = 1E42d;
        valueMin[37] = 1E43d;
        valueMin[38] = 1E44d;
        valueMin[39] = 1E45d;
        valueMin[40] = 1E46d;
        valueMin[41] = 1E47d;
        valueMin[42] = 1E48d;
        valueMin[43] = 1E49d;
        valueMin[44] = 1E50d;
        valueMin[45] = 1E51d;
        valueMin[46] = 1E52d;
        valueMin[47] = 1E53d;
        valueMin[48] = 1E54d;
        valueMin[49] = 1E55d;
        valueMin[50] = 1E56d;
        valueMin[51] = 1E57d;
        valueMin[52] = 1E58d;
        valueMin[53] = 1E59d;
        valueMin[54] = 1E60d;
        valueMin[55] = 1E61d;
        valueMin[56] = 1E62d;
        valueMin[57] = 1E63d;
        valueMin[58] = 1E64d;
        valueMin[59] = 1E65d;
        valueMin[60] = 1E66d;
        valueMin[61] = 1E67d;
        valueMin[62] = 1E68d;
        valueMin[63] = 1E69d;
        valueMin[64] = 1E70d;
        valueMin[65] = 1E71d;
        valueMin[66] = 1E72d;
        valueMin[67] = 1E73d;
        valueMin[68] = 1E74d;
        valueMin[69] = 1E75d;
        valueMin[70] = 1E76d;
        valueMin[71] = 1E77d;
        valueMin[72] = 1E78d;
        valueMin[73] = 1E79d;
        valueMin[74] = 1E80d;
        valueMin[75] = 1E81d;
        valueMin[76] = 1E82d;
        valueMin[77] = 1E83d;
        valueMin[78] = 1E84d;
        valueMin[79] = 1E85d;
        valueMin[80] = 1E86d;
        valueMin[81] = 1E87d;
        valueMin[82] = 1E88d;
        valueMin[83] = 1E89d;
        valueMin[84] = 1E90d;
        valueMin[85] = 1E91d;
        valueMin[86] = 1E92d;
        valueMin[87] = 1E93d;
        valueMin[88] = 1E94d;
        valueMin[89] = 1E95d;
        valueMin[90] = 1E96d;
        valueMin[91] = 1E97d;
        valueMin[92] = 1E98d;
        valueMin[93] = 1E99d;
        valueMin[94] = 1E100d;
        valueMin[95] = 1E101d;
        valueMin[96] = 1E102d;
        valueMin[97] = 1E103d;
        valueMin[98] = 1E104d;
        valueMin[99] = 1E105d;
        valueMin[100] = 1E106d;
        valueMin[101] = 1E107d;
        valueMin[102] = 1E108d;
        valueMin[103] = 1E109d;
        valueMin[104] = 1E110d;
        valueMin[105] = 1E111d;
        valueMin[106] = 1E112d;
        valueMin[107] = 1E113d;
        valueMin[108] = 1E114d;
        valueMin[109] = 1E115d;
        valueMin[110] = 1E116d;
        valueMin[111] = 1E117d;
        valueMin[112] = 1E118d;
        valueMin[113] = 1E119d;
        valueMin[114] = 1E120d;
        valueMin[115] = 1E121d;
        valueMin[116] = 1E122d;
        valueMin[117] = 1E123d;
        valueMin[118] = 1E124d;
        valueMin[119] = 1E125d;
        valueMin[120] = 1E126d;
        valueMin[121] = 1E127d;
        valueMin[122] = 1E128d;
        valueMin[123] = 1E129d;
        valueMin[124] = 1E130d;
        valueMin[125] = 1E131d;
        valueMin[126] = 1E132d;
        valueMin[127] = 1E133d;
        valueMin[128] = 1E134d;
        valueMin[129] = 1E135d;
        valueMin[130] = 1E136d;
        valueMin[131] = 1E137d;
        valueMin[132] = 1E138d;
        valueMin[133] = 1E139d;
        valueMin[134] = 1E140d;
        valueMin[135] = 1E141d;
        valueMin[136] = 1E142d;
        valueMin[137] = 1E143d;
        valueMin[138] = 1E144d;
        valueMin[139] = 1E145d;
        valueMin[140] = 1E146d;
        valueMin[141] = 1E147d;
        valueMin[142] = 1E148d;
        valueMin[143] = 1E149d;
        valueMin[144] = 1E150d;
        valueMin[145] = 1E151d;
        valueMin[146] = 1E152d;
        valueMin[147] = 1E153d;
        valueMin[148] = 1E154d;
        valueMin[149] = 1E155d;
        valueMin[150] = 1E156d;
        valueMin[151] = 1E157d;
        valueMin[152] = 1E158d;
        valueMin[153] = 1E159d;
        valueMin[154] = 1E160d;
        valueMin[155] = 1E161d;
        valueMin[156] = 1E162d;
        valueMin[157] = 1E163d;
        valueMin[158] = 1E164d;
        valueMin[159] = 1E165d;
        valueMin[160] = 1E166d;
        valueMin[161] = 1E167d;
        valueMin[162] = 1E168d;
        valueMin[163] = 1E169d;
        valueMin[164] = 1E170d;
        valueMin[165] = 1E171d;
        valueMin[166] = 1E172d;
        valueMin[167] = 1E173d;
        valueMin[168] = 1E174d;
        valueMin[169] = 1E175d;
        valueMin[170] = 1E176d;
        valueMin[171] = 1E177d;
        valueMin[172] = 1E178d;
        valueMin[173] = 1E179d;
        valueMin[174] = 1E180d;
        valueMin[175] = 1E181d;
        valueMin[176] = 1E182d;
        valueMin[177] = 1E183d;
        valueMin[178] = 1E184d;
        valueMin[179] = 1E185d;
        valueMin[180] = 1E186d;
        valueMin[181] = 1E187d;
        valueMin[182] = 1E188d;
        valueMin[183] = 1E189d;
        valueMin[184] = 1E190d;
        valueMin[185] = 1E191d;
        valueMin[186] = 1E192d;
        valueMin[187] = 1E193d;
        valueMin[188] = 1E194d;
        valueMin[189] = 1E195d;
        valueMin[190] = 1E196d;
        valueMin[191] = 1E197d;
        valueMin[192] = 1E198d;
        valueMin[193] = 1E199d;
        valueMin[194] = 1E200d;
        valueMin[195] = 1E201d;
        valueMin[196] = 1E202d;
        valueMin[197] = 1E203d;
        valueMin[198] = 1E204d;
        valueMin[199] = 1E205d;
        valueMin[200] = 1E206d;
        valueMin[201] = 1E207d;
        valueMin[202] = 1E208d;
        valueMin[203] = 1E209d;
        valueMin[204] = 1E210d;
        valueMin[205] = 1E211d;
        valueMin[206] = 1E212d;
        valueMin[207] = 1E213d;
        valueMin[208] = 1E214d;
        valueMin[209] = 1E215d;
        valueMin[210] = 1E216d;
        valueMin[211] = 1E217d;
        valueMin[212] = 1E218d;
        valueMin[213] = 1E219d;


        valueMax[1] = 1E8f;
        valueMax[2] = 1E9f;
        valueMax[3] = 1E10f;
        valueMax[4] = 1E11f;
        valueMax[5] = 1E12f;
        valueMax[6] = 1E13f;
        valueMax[7] = 1E14f;
        valueMax[8] = 1E15f;
        valueMax[9] = 1E16f;
        valueMax[10] = 1E17f;
        valueMax[11] = 1E18f;
        valueMax[12] = 1E19f;
        valueMax[13] = 1E20f;
        valueMax[14] = 1E21f;
        valueMax[15] = 1E22f;
        valueMax[16] = 1E23f;
        valueMax[17] = 1E24f;
        valueMax[18] = 1E25f;
        valueMax[19] = 1E26f;
        valueMax[20] = 1E27f;
        valueMax[21] = 1E28f;
        valueMax[22] = 1E29f;
        valueMax[23] = 1E30f;
        valueMax[24] = 1E31f;
        valueMax[25] = 1E32f;
        valueMax[26] = 1E33f;
        valueMax[27] = 1E34f;
        valueMax[28] = 1E35f;
        valueMax[29] = 1E36d;
        valueMax[30] = 1E37d;
        valueMax[31] = 1E38d;
        valueMax[32] = 1E39d;
        valueMax[33] = 1E40d;
        valueMax[34] = 1E41d;
        valueMax[35] = 1E42d;
        valueMax[36] = 1E43d;
        valueMax[37] = 1E44d;
        valueMax[38] = 1E45d;
        valueMax[39] = 1E46d;
        valueMax[40] = 1E47d;
        valueMax[41] = 1E48d;
        valueMax[42] = 1E49d;
        valueMax[43] = 1E50d;
        valueMax[44] = 1E51d;
        valueMax[45] = 1E52d;
        valueMax[46] = 1E53d;
        valueMax[47] = 1E54d;
        valueMax[48] = 1E55d;
        valueMax[49] = 1E56d;
        valueMax[50] = 1E57d;
        valueMax[51] = 1E58d;
        valueMax[52] = 1E59d;
        valueMax[53] = 1E60d;
        valueMax[54] = 1E61d;
        valueMax[55] = 1E62d;
        valueMax[56] = 1E63d;
        valueMax[57] = 1E64d;
        valueMax[58] = 1E65d;
        valueMax[59] = 1E66d;
        valueMax[60] = 1E67d;
        valueMax[61] = 1E68d;
        valueMax[62] = 1E69d;
        valueMax[63] = 1E70d;
        valueMax[64] = 1E71d;
        valueMax[65] = 1E72d;
        valueMax[66] = 1E73d;
        valueMax[67] = 1E74d;
        valueMax[68] = 1E75d;
        valueMax[69] = 1E76d;
        valueMax[70] = 1E77d;
        valueMax[71] = 1E78d;
        valueMax[72] = 1E79d;
        valueMax[73] = 1E80d;
        valueMax[74] = 1E81d;
        valueMax[75] = 1E82d;
        valueMax[76] = 1E83d;
        valueMax[77] = 1E84d;
        valueMax[78] = 1E85d;
        valueMax[79] = 1E86d;
        valueMax[80] = 1E87d;
        valueMax[81] = 1E88d;
        valueMax[82] = 1E89d;
        valueMax[83] = 1E90d;
        valueMax[84] = 1E91d;
        valueMax[85] = 1E92d;
        valueMax[86] = 1E93d;
        valueMax[87] = 1E94d;
        valueMax[88] = 1E95d;
        valueMax[89] = 1E96d;
        valueMax[90] = 1E97d;
        valueMax[91] = 1E98d;
        valueMax[92] = 1E99d;
        valueMax[93] = 1E100d;
        valueMax[94] = 1E101d;
        valueMax[95] = 1E102d;
        valueMax[96] = 1E103d;
        valueMax[97] = 1E104d;
        valueMax[98] = 1E105d;
        valueMax[99] = 1E106d;
        valueMax[100] = 1E106d;
        valueMax[101] = 1E107d;
        valueMax[102] = 1E108d;
        valueMax[103] = 1E109d;
        valueMax[104] = 1E110d;
        valueMax[105] = 1E111d;
        valueMax[106] = 1E112d;
        valueMax[107] = 1E113d;
        valueMax[108] = 1E114d;
        valueMax[109] = 1E115d;
        valueMax[110] = 1E116d;
        valueMax[111] = 1E117d;
        valueMax[112] = 1E118d;
        valueMax[113] = 1E119d;
        valueMax[114] = 1E120d;
        valueMax[115] = 1E121d;
        valueMax[116] = 1E122d;
        valueMax[117] = 1E123d;
        valueMax[118] = 1E124d;
        valueMax[119] = 1E125d;
        valueMax[120] = 1E126d;
        valueMax[121] = 1E127d;
        valueMax[122] = 1E128d;
        valueMax[123] = 1E129d;
        valueMax[124] = 1E130d;
        valueMax[125] = 1E131d;
        valueMax[126] = 1E132d;
        valueMax[127] = 1E133d;
        valueMax[128] = 1E134d;
        valueMax[129] = 1E135d;
        valueMax[130] = 1E136d;
        valueMax[131] = 1E137d;
        valueMax[132] = 1E138d;
        valueMax[133] = 1E139d;
        valueMax[134] = 1E140d;
        valueMax[135] = 1E141d;
        valueMax[136] = 1E142d;
        valueMax[137] = 1E143d;
        valueMax[138] = 1E144d;
        valueMax[139] = 1E145d;
        valueMax[140] = 1E146d;
        valueMax[141] = 1E147d;
        valueMax[142] = 1E148d;
        valueMax[143] = 1E149d;
        valueMax[144] = 1E150d;
        valueMax[145] = 1E151d;
        valueMax[146] = 1E152d;
        valueMax[147] = 1E153d;
        valueMax[148] = 1E154d;
        valueMax[149] = 1E155d;
        valueMax[150] = 1E156d;
        valueMax[151] = 1E157d;
        valueMax[152] = 1E158d;
        valueMax[153] = 1E159d;
        valueMax[154] = 1E160d;
        valueMax[155] = 1E161d;
        valueMax[156] = 1E162d;
        valueMax[157] = 1E163d;
        valueMax[158] = 1E164d;
        valueMax[159] = 1E165d;
        valueMax[160] = 1E166d;
        valueMax[161] = 1E167d;
        valueMax[162] = 1E168d;
        valueMax[163] = 1E169d;
        valueMax[164] = 1E170d;
        valueMax[165] = 1E171d;
        valueMax[166] = 1E172d;
        valueMax[167] = 1E173d;
        valueMax[168] = 1E174d;
        valueMax[169] = 1E175d;
        valueMax[170] = 1E176d;
        valueMax[157] = 1E163d;
        valueMax[158] = 1E164d;
        valueMax[159] = 1E165d;
        valueMax[160] = 1E166d;
        valueMax[161] = 1E167d;
        valueMax[162] = 1E168d;
        valueMax[163] = 1E169d;
        valueMax[164] = 1E170d;
        valueMax[165] = 1E171d;
        valueMax[166] = 1E172d;
        valueMax[167] = 1E173d;
        valueMax[168] = 1E174d;
        valueMax[169] = 1E175d;
        valueMax[170] = 1E176d;
        valueMax[171] = 1E177d;
        valueMax[172] = 1E178d;
        valueMax[173] = 1E179d;
        valueMax[174] = 1E180d;
        valueMax[175] = 1E181d;
        valueMax[176] = 1E182d;
        valueMax[177] = 1E183d;
        valueMax[178] = 1E184d;
        valueMax[179] = 1E185d;
        valueMax[180] = 1E186d;
        valueMax[181] = 1E187d;
        valueMax[182] = 1E188d;
        valueMax[183] = 1E189d;
        valueMax[184] = 1E190d;
        valueMax[185] = 1E191d;
        valueMax[186] = 1E192d;
        valueMax[187] = 1E193d;
        valueMax[188] = 1E194d;
        valueMax[189] = 1E195d;
        valueMax[190] = 1E196d;
        valueMax[191] = 1E197d;
        valueMax[192] = 1E198d;
        valueMax[193] = 1E199d;
        valueMax[194] = 1E200d;
        valueMax[195] = 1E201d;
        valueMax[196] = 1E202d;
        valueMax[197] = 1E203d;
        valueMax[198] = 1E204d;
        valueMax[199] = 1E205d;
        valueMax[200] = 1E206d;
        valueMax[201] = 1E207d;
        valueMax[202] = 1E208d;
        valueMax[203] = 1E209d;
        valueMax[204] = 1E210d;
        valueMax[205] = 1E211d;
        valueMax[206] = 1E212d;
        valueMax[207] = 1E213d;
        valueMax[208] = 1E214d;
        valueMax[209] = 1E215d;
        valueMax[210] = 1E216d;
        valueMax[211] = 1E217d;
        valueMax[212] = 1E218d;
        valueMax[213] = 1E219d;





        prestigeTierValueBase[1] = 10;
        for (int i = 2; i < prestigeTierValueBase.Length; i++)
        {
            prestigeTierValueBase[i] = Mathf.FloorToInt((prestigeTierValueBase[i - 1] + (i * 10.3f)));
            //prestigeTierValueBase[i] = Mathf.RoundToInt(4+((((i + 2f) * (Mathf.Pow (1.19f, i + 2))) + 5f)));
        }

        prestigeTierValueBase[95] *= .996f;
        prestigeTierValueBase[96] *= .992f;
        prestigeTierValueBase[97] *= .988f;
        prestigeTierValueBase[98] *= .984f;
        prestigeTierValueBase[99] *= .981f;
        prestigeTierValueBase[100] *= .978f;
        prestigeTierValueBase[101] *= .976f;
        prestigeTierValueBase[102] *= .974f;
        prestigeTierValueBase[103] *= .972f;
        prestigeTierValueBase[104] *= .970f;
        prestigeTierValueBase[105] *= .969f;
        prestigeTierValueBase[106] *= .968f;
        prestigeTierValueBase[107] *= .967f;
        prestigeTierValueBase[108] *= .966f;
        prestigeTierValueBase[109] *= .965f;
        prestigeTierValueBase[110] *= .964f;
        prestigeTierValueBase[111] *= .963f;
        prestigeTierValueBase[112] *= .962f;
        prestigeTierValueBase[113] *= .961f;
        prestigeTierValueBase[114] *= .960f;
        prestigeTierValueBase[115] *= .959f;
        prestigeTierValueBase[116] *= .958f;
        prestigeTierValueBase[117] *= .957f;
        prestigeTierValueBase[118] *= .956f;
        prestigeTierValueBase[119] *= .955f;
        prestigeTierValueBase[120] *= .954f;
        prestigeTierValueBase[121] *= .953f;
        prestigeTierValueBase[122] *= .952f;
        prestigeTierValueBase[123] *= .951f;
        prestigeTierValueBase[124] *= .950f;
        prestigeTierValueBase[125] *= .949f;
        prestigeTierValueBase[126] *= .948f;
        prestigeTierValueBase[127] *= .947f;
        prestigeTierValueBase[128] *= .946f;
        prestigeTierValueBase[129] *= .945f;
        prestigeTierValueBase[130] *= .944f;
        prestigeTierValueBase[131] *= .943f;
        prestigeTierValueBase[132] *= .942f;
        prestigeTierValueBase[133] *= .941f;
        prestigeTierValueBase[134] *= .940f;
        prestigeTierValueBase[135] *= .939f;
        prestigeTierValueBase[136] *= .938f;
        prestigeTierValueBase[137] *= .937f;
        prestigeTierValueBase[138] *= .936f;
        prestigeTierValueBase[139] *= .935f;
        prestigeTierValueBase[140] *= .934f;
        prestigeTierValueBase[141] *= .933f;
        prestigeTierValueBase[142] *= .932f;
        prestigeTierValueBase[143] *= .931f;
        prestigeTierValueBase[144] *= .930f;
        prestigeTierValueBase[145] *= .929f;
        prestigeTierValueBase[146] *= .928f;
        prestigeTierValueBase[147] *= .927f;
        prestigeTierValueBase[148] *= .926f;
        prestigeTierValueBase[149] *= .925f;
        prestigeTierValueBase[150] *= .924f;
        prestigeTierValueBase[151] *= .923f;
        prestigeTierValueBase[152] *= .922f;
        prestigeTierValueBase[153] *= .921f;
        prestigeTierValueBase[154] *= .920f;
        prestigeTierValueBase[155] *= .919f;
        prestigeTierValueBase[156] *= .918f;
        prestigeTierValueBase[157] *= .917f;
        prestigeTierValueBase[158] *= .916f;
        prestigeTierValueBase[159] *= .915f;
        prestigeTierValueBase[160] *= .914f;
        prestigeTierValueBase[161] *= .913f;
        prestigeTierValueBase[162] *= .912f;
        prestigeTierValueBase[163] *= .911f;
        prestigeTierValueBase[164] *= .910f;
        prestigeTierValueBase[165] *= .909f;
        prestigeTierValueBase[166] *= .908f;
        prestigeTierValueBase[167] *= .907f;
        prestigeTierValueBase[168] *= .906f;
        prestigeTierValueBase[169] *= .905f;
        prestigeTierValueBase[170] *= .904f;
        prestigeTierValueBase[171] *= .903f;
        prestigeTierValueBase[172] *= .902f;
        prestigeTierValueBase[173] *= .901f;
        prestigeTierValueBase[174] *= .900f;
        prestigeTierValueBase[175] *= .899f;
        prestigeTierValueBase[176] *= .898f;
        prestigeTierValueBase[177] *= .897f;
        prestigeTierValueBase[178] *= .896f;
        prestigeTierValueBase[179] *= .895f;
        prestigeTierValueBase[180] *= .894f;
        prestigeTierValueBase[181] *= .893f;
        prestigeTierValueBase[182] *= .892f;
        prestigeTierValueBase[183] *= .891f;
        prestigeTierValueBase[184] *= .890f;
        prestigeTierValueBase[185] *= .889f;
        prestigeTierValueBase[186] *= .888f;
        prestigeTierValueBase[187] *= .887f;
        prestigeTierValueBase[188] *= .886f;
        prestigeTierValueBase[189] *= .885f;
        prestigeTierValueBase[190] *= .884f;
        prestigeTierValueBase[191] *= .883f;
        prestigeTierValueBase[192] *= .882f;
        prestigeTierValueBase[193] *= .881f;
        prestigeTierValueBase[194] *= .880f;
        prestigeTierValueBase[195] *= .879f;
        prestigeTierValueBase[196] *= .878f;
        prestigeTierValueBase[197] *= .877f;
        prestigeTierValueBase[198] *= .876f;
        prestigeTierValueBase[199] *= .875f;
        prestigeTierValueBase[200] *= .874f;
        prestigeTierValueBase[201] *= .873f;
        prestigeTierValueBase[202] *= .872f;
        prestigeTierValueBase[203] *= .871f;
        prestigeTierValueBase[204] *= .870f;
        prestigeTierValueBase[205] *= .869f;
        prestigeTierValueBase[206] *= .868f;
        prestigeTierValueBase[207] *= .867f;
        prestigeTierValueBase[208] *= .866f;
        prestigeTierValueBase[209] *= .865f;
        prestigeTierValueBase[210] *= .864f;
        prestigeTierValueBase[211] *= .863f;
        prestigeTierValueBase[212] *= .862f;
        prestigeTierValueBase[213] *= .861f;
        prestigeTierValueBase[214] *= .860f;
        valueMax[176] = 1E182d;
        valueMax[177] = 1E183d;
        valueMax[178] = 1E184d;
        valueMax[179] = 1E185d;
        valueMax[180] = 1E186d;
        valueMax[181] = 1E187d;
        valueMax[182] = 1E188d;
        valueMax[183] = 1E189d;
        valueMax[184] = 1E190d;
        valueMax[185] = 1E191d;
        valueMax[186] = 1E192d;
        valueMax[187] = 1E193d;
        valueMax[188] = 1E194d;
        valueMax[189] = 1E195d;
        valueMax[190] = 1E196d;
        valueMax[191] = 1E197d;
        valueMax[192] = 1E198d;
        valueMax[193] = 1E199d;
        valueMax[194] = 1E200d;
        valueMax[195] = 1E201d;
        valueMax[196] = 1E202d;
        valueMax[197] = 1E203d;
        valueMax[198] = 1E204d;
        valueMax[199] = 1E205d;
        valueMax[200] = 1E206d;
        valueMax[201] = 1E207d;
        valueMax[202] = 1E208d;
        valueMax[203] = 1E209d;
        valueMax[204] = 1E210d;
        valueMax[205] = 1E211d;
        valueMax[206] = 1E212d;
        valueMax[207] = 1E213d;
        valueMax[208] = 1E214d;
        valueMax[209] = 1E215d;
        valueMax[210] = 1E216d;
        valueMax[211] = 1E217d;
        valueMax[212] = 1E218d;
        valueMax[213] = 1E219d;

        prestigeTierValueBase[1] = 10;
        for (int i = 2; i < prestigeTierValueBase.Length; i++)
        {
            prestigeTierValueBase[i] = Mathf.FloorToInt((prestigeTierValueBase[i - 1] + (i * 10.3f)));
        }
    }

    public void CalculateGalaxyValue()
    {
        if (homebase.galaxyTotalValue < 9999000f)
        {
            prestigeTier = 0;
        }
        else if (homebase.galaxyTotalValue >= 9999000f && homebase.galaxyTotalValue < 1E8)
        {
            prestigeTier = 1;
        }
        else if (homebase.galaxyTotalValue >= 1E8 && homebase.galaxyTotalValue < 1E9)
        {
            prestigeTier = 2;
        }
        else if (homebase.galaxyTotalValue >= 1E9 && homebase.galaxyTotalValue < 1E10)
        {
            prestigeTier = 3;
        }
        else if (homebase.galaxyTotalValue >= 1E10 && homebase.galaxyTotalValue < 1E11)
        {
            prestigeTier = 4;
        }
        else if (homebase.galaxyTotalValue >= 1E11 && homebase.galaxyTotalValue < 1E12)
        {
            prestigeTier = 5;
        }
        else if (homebase.galaxyTotalValue >= 1E12 && homebase.galaxyTotalValue < 1E13)
        {
            prestigeTier = 6;
        }
        else if (homebase.galaxyTotalValue >= 1E13 && homebase.galaxyTotalValue < 1E14)
        {
            prestigeTier = 7;
        }
        else if (homebase.galaxyTotalValue >= 1E14 && homebase.galaxyTotalValue < 1E15)
        {
            prestigeTier = 8;
        }
        else if (homebase.galaxyTotalValue >= 1E15 && homebase.galaxyTotalValue < 1E16)
        {
            prestigeTier = 9;
        }
        else if (homebase.galaxyTotalValue >= 1E16 && homebase.galaxyTotalValue < 1E17)
        {
            prestigeTier = 10;
        }
        else if (homebase.galaxyTotalValue >= 1E17 && homebase.galaxyTotalValue < 1E18)
        {
            prestigeTier = 11;
        }
        else if (homebase.galaxyTotalValue >= 1E18 && homebase.galaxyTotalValue < 1E19)
        {
            prestigeTier = 12;
        }
        else if (homebase.galaxyTotalValue >= 1E19 && homebase.galaxyTotalValue < 1E20)
        {
            prestigeTier = 13;
        }
        else if (homebase.galaxyTotalValue >= 1E20 && homebase.galaxyTotalValue < 1E21)
        {
            prestigeTier = 14;
        }
        else if (homebase.galaxyTotalValue >= 1E21 && homebase.galaxyTotalValue < 1E22)
        {
            prestigeTier = 15;
        }
        else if (homebase.galaxyTotalValue >= 1E22 && homebase.galaxyTotalValue < 1E23)
        {
            prestigeTier = 16;
        }
        else if (homebase.galaxyTotalValue >= 1E23 && homebase.galaxyTotalValue < 1E24)
        {
            prestigeTier = 17;
        }
        else if (homebase.galaxyTotalValue >= 1E24 && homebase.galaxyTotalValue < 1E25)
        {
            prestigeTier = 18;
        }
        else if (homebase.galaxyTotalValue >= 1E25 && homebase.galaxyTotalValue < 1E26)
        {
            prestigeTier = 19;
        }
        else if (homebase.galaxyTotalValue >= 1E26 && homebase.galaxyTotalValue < 1E27)
        {
            prestigeTier = 20;
        }
        else if (homebase.galaxyTotalValue >= 1E27 && homebase.galaxyTotalValue < 1E28)
        {
            prestigeTier = 21;
        }
        else if (homebase.galaxyTotalValue >= 1E28 && homebase.galaxyTotalValue < 1E29)
        {
            prestigeTier = 22;
        }
        else if (homebase.galaxyTotalValue >= 1E29 && homebase.galaxyTotalValue < 1E30)
        {
            prestigeTier = 23;
        }
        else if (homebase.galaxyTotalValue >= 1E30 && homebase.galaxyTotalValue < 1E31)
        {
            prestigeTier = 24;
        }
        else if (homebase.galaxyTotalValue >= 1E31 && homebase.galaxyTotalValue < 1E32)
        {
            prestigeTier = 25;
        }
        else if (homebase.galaxyTotalValue >= 1E32 && homebase.galaxyTotalValue < 1E33)
        {
            prestigeTier = 26;
        }
        else if (homebase.galaxyTotalValue >= 1E33 && homebase.galaxyTotalValue < 1E34)
        {
            prestigeTier = 27;
        }
        else if (homebase.galaxyTotalValue >= 1E34 && homebase.galaxyTotalValue < 1E35)
        {
            prestigeTier = 28;
        }
        else if (homebase.galaxyTotalValue >= 1E35 && homebase.galaxyTotalValue < 1E36)
        {
            prestigeTier = 29;
        }
        else if (homebase.galaxyTotalValue >= 1E36 && homebase.galaxyTotalValue < 1E37)
        {
            prestigeTier = 30;
        }
        else if (homebase.galaxyTotalValue >= 1E37 && homebase.galaxyTotalValue < 1E38)
        {
            prestigeTier = 31;
        }
        else if (homebase.galaxyTotalValue >= 1E38 && homebase.galaxyTotalValue < 1E39)
        {
            prestigeTier = 32;
        }
        else if (homebase.galaxyTotalValue >= 1E39 && homebase.galaxyTotalValue < 1E40)
        {
            prestigeTier = 33;
        }
        else if (homebase.galaxyTotalValue >= 1E40 && homebase.galaxyTotalValue < 1E41)
        {
            prestigeTier = 34;
        }
        else if (homebase.galaxyTotalValue >= 1E41 && homebase.galaxyTotalValue < 1E42)
        {
            prestigeTier = 35;
        }
        else if (homebase.galaxyTotalValue >= 1E42 && homebase.galaxyTotalValue < 1E43)
        {
            prestigeTier = 36;
        }
        else if (homebase.galaxyTotalValue >= 1E43 && homebase.galaxyTotalValue < 1E44)
        {
            prestigeTier = 37;
        }
        else if (homebase.galaxyTotalValue >= 1E44 && homebase.galaxyTotalValue < 1E45)
        {
            prestigeTier = 38;
        }
        else if (homebase.galaxyTotalValue >= 1E45 && homebase.galaxyTotalValue < 1E46)
        {
            prestigeTier = 39;
        }
        else if (homebase.galaxyTotalValue >= 1E46 && homebase.galaxyTotalValue < 1E47)
        {
            prestigeTier = 41;
        }
        else if (homebase.galaxyTotalValue >= 1E47 && homebase.galaxyTotalValue < 1E48)
        {
            prestigeTier = 42;
        }
        else if (homebase.galaxyTotalValue >= 1E48 && homebase.galaxyTotalValue < 1E49)
        {
            prestigeTier = 43;
        }
        else if (homebase.galaxyTotalValue >= 1E49 && homebase.galaxyTotalValue < 1E50)
        {
            prestigeTier = 44;
        }
        else if (homebase.galaxyTotalValue >= 1E50 && homebase.galaxyTotalValue < 1E51)
        {
            prestigeTier = 45;
        }
        else if (homebase.galaxyTotalValue >= 1E51 && homebase.galaxyTotalValue < 1E52)
        {
            prestigeTier = 46;
        }
        else if (homebase.galaxyTotalValue >= 1E52 && homebase.galaxyTotalValue < 1E53)
        {
            prestigeTier = 47;
        }
        else if (homebase.galaxyTotalValue >= 1E53 && homebase.galaxyTotalValue < 1E54)
        {
            prestigeTier = 48;
        }
        else if (homebase.galaxyTotalValue >= 1E54 && homebase.galaxyTotalValue < 1E55)
        {
            prestigeTier = 49;
        }
        else if (homebase.galaxyTotalValue >= 1E55 && homebase.galaxyTotalValue < 1E56)
        {
            prestigeTier = 50;
        }
        else if (homebase.galaxyTotalValue >= 1E56 && homebase.galaxyTotalValue < 1E57)
        {
            prestigeTier = 51;
        }
        else if (homebase.galaxyTotalValue >= 1E57 && homebase.galaxyTotalValue < 1E58)
        {
            prestigeTier = 52;
        }
        else if (homebase.galaxyTotalValue >= 1E58 && homebase.galaxyTotalValue < 1E59)
        {
            prestigeTier = 53;
        }
        else if (homebase.galaxyTotalValue >= 1E59 && homebase.galaxyTotalValue < 1E60)
        {
            prestigeTier = 54;
        }
        else if (homebase.galaxyTotalValue >= 1E60 && homebase.galaxyTotalValue < 1E61)
        {
            prestigeTier = 55;
        }
        else if (homebase.galaxyTotalValue >= 1E61 && homebase.galaxyTotalValue < 1E62)
        {
            prestigeTier = 56;
        }
        else if (homebase.galaxyTotalValue >= 1E62 && homebase.galaxyTotalValue < 1E63)
        {
            prestigeTier = 57;
        }
        else if (homebase.galaxyTotalValue >= 1E63 && homebase.galaxyTotalValue < 1E64)
        {
            prestigeTier = 58;
        }
        else if (homebase.galaxyTotalValue >= 1E64 && homebase.galaxyTotalValue < 1E65)
        {
            prestigeTier = 59;
        }
        else if (homebase.galaxyTotalValue >= 1E65 && homebase.galaxyTotalValue < 1E66)
        {
            prestigeTier = 60;
        }
        else if (homebase.galaxyTotalValue >= 1E66 && homebase.galaxyTotalValue < 1E67)
        {
            prestigeTier = 61;
        }
        else if (homebase.galaxyTotalValue >= 1E67 && homebase.galaxyTotalValue < 1E68)
        {
            prestigeTier = 62;
        }
        else if (homebase.galaxyTotalValue >= 1E68 && homebase.galaxyTotalValue < 1E69)
        {
            prestigeTier = 63;
        }
        else if (homebase.galaxyTotalValue >= 1E69 && homebase.galaxyTotalValue < 1E70)
        {
            prestigeTier = 64;
        }
        else if (homebase.galaxyTotalValue >= 1E70 && homebase.galaxyTotalValue < 1E71)
        {
            prestigeTier = 65;
        }
        else if (homebase.galaxyTotalValue >= 1E71 && homebase.galaxyTotalValue < 1E72)
        {
            prestigeTier = 66;
        }
        else if (homebase.galaxyTotalValue >= 1E72 && homebase.galaxyTotalValue < 1E73)
        {
            prestigeTier = 67;
        }
        else if (homebase.galaxyTotalValue >= 1E73 && homebase.galaxyTotalValue < 1E74)
        {
            prestigeTier = 68;
        }
        else if (homebase.galaxyTotalValue >= 1E74 && homebase.galaxyTotalValue < 1E75)
        {
            prestigeTier = 69;
        }
        else if (homebase.galaxyTotalValue >= 1E75 && homebase.galaxyTotalValue < 1E76)
        {
            prestigeTier = 70;
        }
        else if (homebase.galaxyTotalValue >= 1E76 && homebase.galaxyTotalValue < 1E77)
        {
            prestigeTier = 71;
        }
        else if (homebase.galaxyTotalValue >= 1E77 && homebase.galaxyTotalValue < 1E78)
        {
            prestigeTier = 72;
        }
        else if (homebase.galaxyTotalValue >= 1E78 && homebase.galaxyTotalValue < 1E79)
        {
            prestigeTier = 73;
        }
        else if (homebase.galaxyTotalValue >= 1E79 && homebase.galaxyTotalValue < 1E80)
        {
            prestigeTier = 74;
        }
        else if (homebase.galaxyTotalValue >= 1E80 && homebase.galaxyTotalValue < 1E81)
        {
            prestigeTier = 75;
        }
        else if (homebase.galaxyTotalValue >= 1E81 && homebase.galaxyTotalValue < 1E82)
        {
            prestigeTier = 76;
        }
        else if (homebase.galaxyTotalValue >= 1E82 && homebase.galaxyTotalValue < 1E83)
        {
            prestigeTier = 77;
        }
        else if (homebase.galaxyTotalValue >= 1E83 && homebase.galaxyTotalValue < 1E84)
        {
            prestigeTier = 78;
        }
        else if (homebase.galaxyTotalValue >= 1E84 && homebase.galaxyTotalValue < 1E85)
        {
            prestigeTier = 79;
        }
        else if (homebase.galaxyTotalValue >= 1E85 && homebase.galaxyTotalValue < 1E86)
        {
            prestigeTier = 80;
        }
        else if (homebase.galaxyTotalValue >= 1E86 && homebase.galaxyTotalValue < 1E87)
        {
            prestigeTier = 81;
        }
        else if (homebase.galaxyTotalValue >= 1E87 && homebase.galaxyTotalValue < 1E88)
        {
            prestigeTier = 82;
        }
        else if (homebase.galaxyTotalValue >= 1E88 && homebase.galaxyTotalValue < 1E89)
        {
            prestigeTier = 83;
        }
        else if (homebase.galaxyTotalValue >= 1E89 && homebase.galaxyTotalValue < 1E90)
        {
            prestigeTier = 84;
        }
        else if (homebase.galaxyTotalValue >= 1E90 && homebase.galaxyTotalValue < 1E91)
        {
            prestigeTier = 85;
        }
        else if (homebase.galaxyTotalValue >= 1E91 && homebase.galaxyTotalValue < 1E92)
        {
            prestigeTier = 86;
        }
        else if (homebase.galaxyTotalValue >= 1E92 && homebase.galaxyTotalValue < 1E93)
        {
            prestigeTier = 87;
        }
        else if (homebase.galaxyTotalValue >= 1E93 && homebase.galaxyTotalValue < 1E94)
        {
            prestigeTier = 88;
        }
        else if (homebase.galaxyTotalValue >= 1E94 && homebase.galaxyTotalValue < 1E95)
        {
            prestigeTier = 89;
        }
        else if (homebase.galaxyTotalValue >= 1E95 && homebase.galaxyTotalValue < 1E96)
        {
            prestigeTier = 90;
        }
        else if (homebase.galaxyTotalValue >= 1E96 && homebase.galaxyTotalValue < 1E97)
        {
            prestigeTier = 91;
        }
        else if (homebase.galaxyTotalValue >= 1E97 && homebase.galaxyTotalValue < 1E98)
        {
            prestigeTier = 92;
        }
        else if (homebase.galaxyTotalValue >= 1E98 && homebase.galaxyTotalValue < 1E99)
        {
            prestigeTier = 93;
        }
        else if (homebase.galaxyTotalValue >= 1E99 && homebase.galaxyTotalValue < 1E100)
        {
            prestigeTier = 94;
        }
        else if (homebase.galaxyTotalValue >= 1E100 && homebase.galaxyTotalValue < 1E101)
        {
            prestigeTier = 95;
        }
        else if (homebase.galaxyTotalValue >= 1E101 && homebase.galaxyTotalValue < 1E102)
        {
            prestigeTier = 96;
        }
        else if (homebase.galaxyTotalValue >= 1E102 && homebase.galaxyTotalValue < 1E103)
        {
            prestigeTier = 97;
        }
        else if (homebase.galaxyTotalValue >= 1E103 && homebase.galaxyTotalValue < 1E104)
        {
            prestigeTier = 98;
        }
        else if (homebase.galaxyTotalValue >= 1E104 && homebase.galaxyTotalValue < 1E105)
        {
            prestigeTier = 99;
        }
        else if (homebase.galaxyTotalValue >= 1E105 && homebase.galaxyTotalValue < 1E106)
        {
            prestigeTier = 100;
        }
        else if (homebase.galaxyTotalValue >= 1E106 && homebase.galaxyTotalValue < 1E107)
        {
            prestigeTier = 101;
        }
        else if (homebase.galaxyTotalValue >= 1E107 && homebase.galaxyTotalValue < 1E108)
        {
            prestigeTier = 102;
        }
        else if (homebase.galaxyTotalValue >= 1E108 && homebase.galaxyTotalValue < 1E109)
        {
            prestigeTier = 103;
        }
        else if (homebase.galaxyTotalValue >= 1E109 && homebase.galaxyTotalValue < 1E110)
        {
            prestigeTier = 104;
        }
        else if (homebase.galaxyTotalValue >= 1E110 && homebase.galaxyTotalValue < 1E111)
        {
            prestigeTier = 105;
        }
        else if (homebase.galaxyTotalValue >= 1E111 && homebase.galaxyTotalValue < 1E112)
        {
            prestigeTier = 106;
        }
        else if (homebase.galaxyTotalValue >= 1E112 && homebase.galaxyTotalValue < 1E113)
        {
            prestigeTier = 107;
        }
        else if (homebase.galaxyTotalValue >= 1E113 && homebase.galaxyTotalValue < 1E114)
        {
            prestigeTier = 108;
        }
        else if (homebase.galaxyTotalValue >= 1E114 && homebase.galaxyTotalValue < 1E115)
        {
            prestigeTier = 109;
        }
        else if (homebase.galaxyTotalValue >= 1E115 && homebase.galaxyTotalValue < 1E116)
        {
            prestigeTier = 110;
        }
        else if (homebase.galaxyTotalValue >= 1E116 && homebase.galaxyTotalValue < 1E117)
        {
            prestigeTier = 111;
        }
        else if (homebase.galaxyTotalValue >= 1E117 && homebase.galaxyTotalValue < 1E118)
        {
            prestigeTier = 112;
        }
        else if (homebase.galaxyTotalValue >= 1E118 && homebase.galaxyTotalValue < 1E119)
        {
            prestigeTier = 113;
        }
        else if (homebase.galaxyTotalValue >= 1E119 && homebase.galaxyTotalValue < 1E120)
        {
            prestigeTier = 114;
        }
        else if (homebase.galaxyTotalValue >= 1E120 && homebase.galaxyTotalValue < 1E121)
        {
            prestigeTier = 115;
        }
        else if (homebase.galaxyTotalValue >= 1E121 && homebase.galaxyTotalValue < 1E122)
        {
            prestigeTier = 116;
        }
        else if (homebase.galaxyTotalValue >= 1E122 && homebase.galaxyTotalValue < 1E123)
        {
            prestigeTier = 117;
        }
        else if (homebase.galaxyTotalValue >= 1E123 && homebase.galaxyTotalValue < 1E124)
        {
            prestigeTier = 118;
        }
        else if (homebase.galaxyTotalValue >= 1E124 && homebase.galaxyTotalValue < 1E125)
        {
            prestigeTier = 119;
        }
        else if (homebase.galaxyTotalValue >= 1E125 && homebase.galaxyTotalValue < 1E126)
        {
            prestigeTier = 120;
        }
        else if (homebase.galaxyTotalValue >= 1E126 && homebase.galaxyTotalValue < 1E127)
        {
            prestigeTier = 121;
        }
        else if (homebase.galaxyTotalValue >= 1E127 && homebase.galaxyTotalValue < 1E128)
        {
            prestigeTier = 122;
        }
        else if (homebase.galaxyTotalValue >= 1E128 && homebase.galaxyTotalValue < 1E129)
        {
            prestigeTier = 123;
        }
        else if (homebase.galaxyTotalValue >= 1E129 && homebase.galaxyTotalValue < 1E130)
        {
            prestigeTier = 124;
        }
        else if (homebase.galaxyTotalValue >= 1E130 && homebase.galaxyTotalValue < 1E131)
        {
            prestigeTier = 125;
        }
        else if (homebase.galaxyTotalValue >= 1E131 && homebase.galaxyTotalValue < 1E132)
        {
            prestigeTier = 126;
        }
        else if (homebase.galaxyTotalValue >= 1E132 && homebase.galaxyTotalValue < 1E133)
        {
            prestigeTier = 127;
        }
        else if (homebase.galaxyTotalValue >= 1E133 && homebase.galaxyTotalValue < 1E134)
        {
            prestigeTier = 128;
        }
        else if (homebase.galaxyTotalValue >= 1E134 && homebase.galaxyTotalValue < 1E135)
        {
            prestigeTier = 129;
        }
        else if (homebase.galaxyTotalValue >= 1E135 && homebase.galaxyTotalValue < 1E136)
        {
            prestigeTier = 130;
        }
        else if (homebase.galaxyTotalValue >= 1E136 && homebase.galaxyTotalValue < 1E137)
        {
            prestigeTier = 131;
        }
        else if (homebase.galaxyTotalValue >= 1E137 && homebase.galaxyTotalValue < 1E138)
        {
            prestigeTier = 132;
        }
        else if (homebase.galaxyTotalValue >= 1E138 && homebase.galaxyTotalValue < 1E139)
        {
            prestigeTier = 133;
        }
        else if (homebase.galaxyTotalValue >= 1E139 && homebase.galaxyTotalValue < 1E140)
        {
            prestigeTier = 134;
        }
        else if (homebase.galaxyTotalValue >= 1E140 && homebase.galaxyTotalValue < 1E141)
        {
            prestigeTier = 135;
        }
        else if (homebase.galaxyTotalValue >= 1E141 && homebase.galaxyTotalValue < 1E142)
        {
            prestigeTier = 136;
        }
        else if (homebase.galaxyTotalValue >= 1E142 && homebase.galaxyTotalValue < 1E143)
        {
            prestigeTier = 137;
        }
        else if (homebase.galaxyTotalValue >= 1E143 && homebase.galaxyTotalValue < 1E144)
        {
            prestigeTier = 138;
        }
        else if (homebase.galaxyTotalValue >= 1E144 && homebase.galaxyTotalValue < 1E145)
        {
            prestigeTier = 139;
        }
        else if (homebase.galaxyTotalValue >= 1E145 && homebase.galaxyTotalValue < 1E146)
        {
            prestigeTier = 140;
        }
        else if (homebase.galaxyTotalValue >= 1E146 && homebase.galaxyTotalValue < 1E147)
        {
            prestigeTier = 141;
        }
        else if (homebase.galaxyTotalValue >= 1E147 && homebase.galaxyTotalValue < 1E148)
        {
            prestigeTier = 142;
        }
        else if (homebase.galaxyTotalValue >= 1E148 && homebase.galaxyTotalValue < 1E149)
        {
            prestigeTier = 143;
        }
        else if (homebase.galaxyTotalValue >= 1E149 && homebase.galaxyTotalValue < 1E150)
        {
            prestigeTier = 144;
        }
        else if (homebase.galaxyTotalValue >= 1E150 && homebase.galaxyTotalValue < 1E151)
        {
            prestigeTier = 145;
        }
        else if (homebase.galaxyTotalValue >= 1E151 && homebase.galaxyTotalValue < 1E152)
        {
            prestigeTier = 146;
        }
        else if (homebase.galaxyTotalValue >= 1E152 && homebase.galaxyTotalValue < 1E153)
        {
            prestigeTier = 147;
        }
        else if (homebase.galaxyTotalValue >= 1E153 && homebase.galaxyTotalValue < 1E154)
        {
            prestigeTier = 148;
        }
        else if (homebase.galaxyTotalValue >= 1E154 && homebase.galaxyTotalValue < 1E155)
        {
            prestigeTier = 149;
        }
        else if (homebase.galaxyTotalValue >= 1E155 && homebase.galaxyTotalValue < 1E156)
        {
            prestigeTier = 150;
        }
        else if (homebase.galaxyTotalValue >= 1E156 && homebase.galaxyTotalValue < 1E157)
        {
            prestigeTier = 151;
        }
        else if (homebase.galaxyTotalValue >= 1E157 && homebase.galaxyTotalValue < 1E158)
        {
            prestigeTier = 152;
        }
        else if (homebase.galaxyTotalValue >= 1E158 && homebase.galaxyTotalValue < 1E159)
        {
            prestigeTier = 153;
        }
        else if (homebase.galaxyTotalValue >= 1E159 && homebase.galaxyTotalValue < 1E160)
        {
            prestigeTier = 154;
        }
        else if (homebase.galaxyTotalValue >= 1E160 && homebase.galaxyTotalValue < 1E161)
        {
            prestigeTier = 155;
        }
        else if (homebase.galaxyTotalValue >= 1E161 && homebase.galaxyTotalValue < 1E162)
        {
            prestigeTier = 156;
        }
        else if (homebase.galaxyTotalValue >= 1E162 && homebase.galaxyTotalValue < 1E163)
        {
            prestigeTier = 157;
        }
        else if (homebase.galaxyTotalValue >= 1E163 && homebase.galaxyTotalValue < 1E164)
        {
            prestigeTier = 158;
        }
        else if (homebase.galaxyTotalValue >= 1E164 && homebase.galaxyTotalValue < 1E165)
        {
            prestigeTier = 159;
        }
        else if (homebase.galaxyTotalValue >= 1E165 && homebase.galaxyTotalValue < 1E166)
        {
            prestigeTier = 160;
        }
        else if (homebase.galaxyTotalValue >= 1E166 && homebase.galaxyTotalValue < 1E167)
        {
            prestigeTier = 161;
        }
        else if (homebase.galaxyTotalValue >= 1E167 && homebase.galaxyTotalValue < 1E168)
        {
            prestigeTier = 162;
        }
        else if (homebase.galaxyTotalValue >= 1E168 && homebase.galaxyTotalValue < 1E169)
        {
            prestigeTier = 163;
        }
        else if (homebase.galaxyTotalValue >= 1E169 && homebase.galaxyTotalValue < 1E170)
        {
            prestigeTier = 164;
        }
        else if (homebase.galaxyTotalValue >= 1E170 && homebase.galaxyTotalValue < 1E171)
        {
            prestigeTier = 165;
        }
        else if (homebase.galaxyTotalValue >= 1E171 && homebase.galaxyTotalValue < 1E172)
        {
            prestigeTier = 166;
        }
        else if (homebase.galaxyTotalValue >= 1E172 && homebase.galaxyTotalValue < 1E173)
        {
            prestigeTier = 167;
        }
        else if (homebase.galaxyTotalValue >= 1E173 && homebase.galaxyTotalValue < 1E174)
        {
            prestigeTier = 168;
        }
        else if (homebase.galaxyTotalValue >= 1E174 && homebase.galaxyTotalValue < 1E175)
        {
            prestigeTier = 169;
        }
        else if (homebase.galaxyTotalValue >= 1E175 && homebase.galaxyTotalValue < 1E176)
        {
            prestigeTier = 170;
        }
        else if (homebase.galaxyTotalValue >= 1E176 && homebase.galaxyTotalValue < 1E177)
        {
            prestigeTier = 171;
        }
        else if (homebase.galaxyTotalValue >= 1E177 && homebase.galaxyTotalValue < 1E178)
        {
            prestigeTier = 172;
        }
        else if (homebase.galaxyTotalValue >= 1E178 && homebase.galaxyTotalValue < 1E179)
        {
            prestigeTier = 173;
        }
        else if (homebase.galaxyTotalValue >= 1E179 && homebase.galaxyTotalValue < 1E180)
        {
            prestigeTier = 174;
        }
        else if (homebase.galaxyTotalValue >= 1E180 && homebase.galaxyTotalValue < 1E181)
        {
            prestigeTier = 175;
        }
        else if (homebase.galaxyTotalValue >= 1E181 && homebase.galaxyTotalValue < 1E182)
        {
            prestigeTier = 176;
        }
        else if (homebase.galaxyTotalValue >= 1E182 && homebase.galaxyTotalValue < 1E183)
        {
            prestigeTier = 177;
        }
        else if (homebase.galaxyTotalValue >= 1E183 && homebase.galaxyTotalValue < 1E184)
        {
            prestigeTier = 178;
        }
        else if (homebase.galaxyTotalValue >= 1E184 && homebase.galaxyTotalValue < 1E185)
        {
            prestigeTier = 179;
        }
        else if (homebase.galaxyTotalValue >= 1E185 && homebase.galaxyTotalValue < 1E186)
        {
            prestigeTier = 180;
        }
        else if (homebase.galaxyTotalValue >= 1E186 && homebase.galaxyTotalValue < 1E187)
        {
            prestigeTier = 181;
        }
        else if (homebase.galaxyTotalValue >= 1E187 && homebase.galaxyTotalValue < 1E188)
        {
            prestigeTier = 182;
        }
        else if (homebase.galaxyTotalValue >= 1E188 && homebase.galaxyTotalValue < 1E189)
        {
            prestigeTier = 183;
        }
        else if (homebase.galaxyTotalValue >= 1E189 && homebase.galaxyTotalValue < 1E190)
        {
            prestigeTier = 184;
        }
        else if (homebase.galaxyTotalValue >= 1E190 && homebase.galaxyTotalValue < 1E191)
        {
            prestigeTier = 185;
        }
        else if (homebase.galaxyTotalValue >= 1E191 && homebase.galaxyTotalValue < 1E192)
        {
            prestigeTier = 186;
        }
        else if (homebase.galaxyTotalValue >= 1E192 && homebase.galaxyTotalValue < 1E193)
        {
            prestigeTier = 187;
        }
        else if (homebase.galaxyTotalValue >= 1E193 && homebase.galaxyTotalValue < 1E194)
        {
            prestigeTier = 188;
        }
        else if (homebase.galaxyTotalValue >= 1E194 && homebase.galaxyTotalValue < 1E195)
        {
            prestigeTier = 189;
        }
        else if (homebase.galaxyTotalValue >= 1E195 && homebase.galaxyTotalValue < 1E196)
        {
            prestigeTier = 190;
        }
        else if (homebase.galaxyTotalValue >= 1E196 && homebase.galaxyTotalValue < 1E197)
        {
            prestigeTier = 191;
        }
        else if (homebase.galaxyTotalValue >= 1E197 && homebase.galaxyTotalValue < 1E198)
        {
            prestigeTier = 192;
        }
        else if (homebase.galaxyTotalValue >= 1E198 && homebase.galaxyTotalValue < 1E199)
        {
            prestigeTier = 193;
        }
        else if (homebase.galaxyTotalValue >= 1E199 && homebase.galaxyTotalValue < 1E200)
        {
            prestigeTier = 194;
        }
        else if (homebase.galaxyTotalValue >= 1E200 && homebase.galaxyTotalValue < 1E201)
        {
            prestigeTier = 195;
        }
        else if (homebase.galaxyTotalValue >= 1E201 && homebase.galaxyTotalValue < 1E202)
        {
            prestigeTier = 196;
        }
        else if (homebase.galaxyTotalValue >= 1E202 && homebase.galaxyTotalValue < 1E203)
        {
            prestigeTier = 197;
        }
        else if (homebase.galaxyTotalValue >= 1E203 && homebase.galaxyTotalValue < 1E204)
        {
            prestigeTier = 198;
        }
        else if (homebase.galaxyTotalValue >= 1E204 && homebase.galaxyTotalValue < 1E205)
        {
            prestigeTier = 199;
        }
        else if (homebase.galaxyTotalValue >= 1E205 && homebase.galaxyTotalValue < 1E206)
        {
            prestigeTier = 200;
        }
        else if (homebase.galaxyTotalValue >= 1E206 && homebase.galaxyTotalValue < 1E207)
        {
            prestigeTier = 201;
        }
        else if (homebase.galaxyTotalValue >= 1E207 && homebase.galaxyTotalValue < 1E208)
        {
            prestigeTier = 202;
        }
        else if (homebase.galaxyTotalValue >= 1E208 && homebase.galaxyTotalValue < 1E209)
        {
            prestigeTier = 203;
        }
        else if (homebase.galaxyTotalValue >= 1E209 && homebase.galaxyTotalValue < 1E210)
        {
            prestigeTier = 204;
        }
        else if (homebase.galaxyTotalValue >= 1E210 && homebase.galaxyTotalValue < 1E211)
        {
            prestigeTier = 205;
        }
        else if (homebase.galaxyTotalValue >= 1E211 && homebase.galaxyTotalValue < 1E212)
        {
            prestigeTier = 206;
        }
        else if (homebase.galaxyTotalValue >= 1E212 && homebase.galaxyTotalValue < 1E213)
        {
            prestigeTier = 207;
        }
        else if (homebase.galaxyTotalValue >= 1E213 && homebase.galaxyTotalValue < 1E214)
        {
            prestigeTier = 208;
        }
        else if (homebase.galaxyTotalValue >= 1E214 && homebase.galaxyTotalValue < 1E215)
        {
            prestigeTier = 209;
        }
        else if (homebase.galaxyTotalValue >= 1E215 && homebase.galaxyTotalValue < 1E216)
        {
            prestigeTier = 210;
        }
        else if (homebase.galaxyTotalValue >= 1E216 && homebase.galaxyTotalValue < 1E217)
        {
            prestigeTier = 211;
        }
        else if (homebase.galaxyTotalValue >= 1E217 && homebase.galaxyTotalValue < 1E218)
        {
            prestigeTier = 212;
        }
        else if (homebase.galaxyTotalValue >= 1E218 && homebase.galaxyTotalValue < 1E219)
        {
            prestigeTier = 213;
        }
        else
        {
            prestigeTier = 214;
        }


        if (prestigeTier == 0)
        {
            prestigePointsGalaxyCurrent = 0;
            prestigePointsBase = 0;
            prestigePointsLounge = 0;
            prestigePointsExodus = 0;
            prestigePointsStation = 0;
        }
        else
        {
            valueDifference = valueMax[prestigeTier] - valueMin[prestigeTier];
            if (prestigeTier != 214)
            {
                prestigePointsDifference = prestigeTierValueBase[prestigeTier + 1] - prestigeTierValueBase[prestigeTier];
                partialRatio = (homebase.galaxyTotalValue - valueMin[prestigeTier]) / valueMax[prestigeTier];
            }
            else
            {
                prestigePointsDifference = 0;
                partialRatio = 0;
            }
            //partialRatio = homebase.galaxyTotalValue / valueDifference;
            int exodusBonus;
            if (homebase.exodusBundleUnlockedBool == true)
            {
                exodusBonus = 2;
            }
            else
            {
                exodusBonus = 1;
            }
            //Debug.Log ("Check2");
            prestigePointsBase = Mathf.RoundToInt(((double)partialRatio * prestigePointsDifference) + (prestigeTierValueBase[prestigeTier]));
            var total = prestigePointsBase;
            prestigePointsLounge = Mathf.RoundToInt((1 * prestigePointsBase) - prestigePointsBase);
            total += prestigePointsLounge;
            prestigePointsStation = Mathf.RoundToInt((station.creditsBonuses * (total)) - total);
            total += prestigePointsStation;
            prestigePointsGalaxyCurrent = Mathf.RoundToInt(total * exodusBonus);

            prestigePointsExodus = Mathf.RoundToInt(total);



            //Debug.Log ("value difference = " + valueDifference);
            //Debug.Log ("prestige points different = " + prestigePointsDifference);
            //Debug.Log ("partial ratio = " + partialRatio);
            //Debug.Log ("prestige bonus reward = " + prestigeRewardBenefit [7]);
            //Debug.Log ("prestige points total = " + prestigePointsGalaxyCurrent);
        }

    }
}

public class StationProjects
{
    internal int creditsBonuses = 0;
}

public class Homebase(double gv)
{
    internal double galaxyTotalValue = gv;
    internal bool exodusBundleUnlockedBool = true;
}