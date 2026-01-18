using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using uav.logic.Database.Model;

namespace uav.logic.tests.Database.Model
{
    public class UserConfigTests
    {
#pragma warning disable IDE1006 // Naming Styles
        public record CreditsExpectation(int totalCredits, int loungeBonus, int station, int exodus);
#pragma warning restore IDE1006 // Naming Styles

        [Test]
      [MethodDataSource(typeof(UserConfigTests), nameof(TestCalculateCreditsData))]
      public async Task TestCalculateCredits(UserConfig userConfig, int baseCredits, CreditsExpectation expected)
      {
          // Act
          var result = userConfig.CalculateCredits(baseCredits);
          var reverse = userConfig.CalculateBaseCredits(result.totalCredits);

          // Assert
          await Assert.That(result.loungeBonus).IsEqualTo(expected.loungeBonus);
          await Assert.That(result.station).IsEqualTo(expected.station);
          await Assert.That(result.exodus).IsEqualTo(expected.exodus);
          await Assert.That(result.totalCredits).IsEqualTo(expected.totalCredits);

          await Assert.That(reverse).IsEqualTo(baseCredits);
      }
      public static IEnumerable<Func<(UserConfig userConfig, int baseCredits, CreditsExpectation expected)>> TestCalculateCreditsData()
      {
          yield return () => (
            new UserConfig {
                LoungeLevel = 40,
                HasExodus = true,
                Credits1 = 5,
                Credits4 = 1,
            },
            812,
            new CreditsExpectation(totalCredits: 5966, loungeBonus: 1705, station: 466, exodus: 2983)
          );

          yield return () => (
            new UserConfig {
                LoungeLevel = 0,
                HasExodus = false,
                Credits1 = 0,
                Credits4 = 0,
            },
            812,
            new CreditsExpectation(totalCredits: 812, loungeBonus: 0, station: 0, exodus: 0)
          );

          yield return () => (
            new UserConfig {
                LoungeLevel = 40,
                HasExodus = true,
                Credits1 = 5,
                Credits4 = 1,
            },
            0,
            new CreditsExpectation(totalCredits: 0, loungeBonus: 0, station: 0, exodus: 0)
          );

          yield return () => (
            new UserConfig {
                LoungeLevel = 40,
                HasExodus = true,
                Credits1 = 5,
                Credits4 = 1,
            },
            838,
            new CreditsExpectation(totalCredits: 6158, loungeBonus: 1760, station: 481, exodus: 3079)
          );
      }
    }
}