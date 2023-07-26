using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using uav.logic.Database.Model;

namespace uav.logic.tests.Database.Model
{
    [TestClass]
    public class UserConfigTests
    {
      [TestMethod]
      [DynamicData(nameof(TestCalculateCreditsData), DynamicDataSourceType.Method)]
      public void TestCalculateCredits(UserConfig userConfig, int baseCredits, (int totalCredits, int loungeBonus, int station, int exodus) expected)
      {
          // Act
          var result = userConfig.CalculateCredits(baseCredits);
          var reverse = userConfig.CalculateBaseCredits(result.totalCredits);

          // Assert
          Assert.AreEqual(expected.loungeBonus, result.loungeBonus, $"Lounge bonus was {result.loungeBonus}.");
          Assert.AreEqual(expected.station, result.station, $"Station was {result.station}.");
          Assert.AreEqual(expected.exodus, result.exodus, $"Exodus was {result.exodus}.");
          Assert.AreEqual(expected.totalCredits, result.totalCredits, $"Total credits was {result.totalCredits}.");

          Assert.AreEqual(baseCredits, reverse, $"Reverse was {reverse}.");
      }
      private static IEnumerable<object[]> TestCalculateCreditsData()
      {
          yield return new object[] {
            new UserConfig {
                LoungeLevel = 40,
                HasExodus = true,
                Credits1 = 5,
                Credits4 = 1,
            },
            812,
            (totalCredits: 5966, loungeBonus: 1705, station: 466, exodus: 2983)
            };

          yield return new object[] {
            new UserConfig {
                LoungeLevel = 0,
                HasExodus = false,
                Credits1 = 0,
                Credits4 = 0,
            },
            812,
            (totalCredits: 812, loungeBonus: 0, station: 0, exodus: 0)
          };

          yield return new object[] {
            new UserConfig {
                LoungeLevel = 40,
                HasExodus = true,
                Credits1 = 5,
                Credits4 = 1,
            },
            0,
            (totalCredits: 0, loungeBonus: 0, station: 0, exodus: 0)
          };

          yield return new object[] {
            new UserConfig {
                LoungeLevel = 40,
                HasExodus = true,
                Credits1 = 5,
                Credits4 = 1,
            },
            838,
            (totalCredits: 6158, loungeBonus: 1760, station: 481, exodus: 3079)
          };
      }
    }
}