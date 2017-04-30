using Xunit;
using FluentAssertions;


namespace QA.ProductCatalog.ImpactService.Tests
{
    public class TariffDirectionFixture
    {
        [Fact]
        public void GetKey_SimpleTariffDirection()
        {
            var a = new TariffDirection("OutgoingCalls", "RussiaExceptHome", "Russia",
                new[] {"ExceptMTS", "ExceptHome", "WithinPackage"});
            a.GetKey(true)
                .Should().Be("BaseParameter: OutgoingCalls; Zone: RussiaExceptHome; Direction: Russia; BaseParameterModifiers: ExceptHome,ExceptMTS;");
        }

        [Fact]
        public void GetKey_BaseParameter()
        {
            var a = new TariffDirection("IncomingCalls", null, null, null);
            a.GetKey()
                .Should().Be("BaseParameter: IncomingCalls; Zone: ; Direction: ; BaseParameterModifiers: ;");
        }

        [Fact]
        public void GetKey_SimpleTariffDirectionWithSpecials()
        {
            var a = new TariffDirection("OutgoingCalls", "RussiaExceptHome", "Russia",
                new[] { "ExceptMTS", "ExceptHome", "OverPackage" });
            a.GetKey()
                .Should().Be("BaseParameter: OutgoingCalls; Zone: RussiaExceptHome; Direction: Russia; BaseParameterModifiers: ExceptHome,ExceptMTS,OverPackage;");
        }

        [Fact]
        public void GetKey_EmptyTariffDirection()
        {
            var a = new TariffDirection(null, null, null, null);
            a.GetKey(true, true).Should().BeEmpty();
        }

        [Fact]
        public void GetKey_SimpleTariffDirectionWithoutBaseParameter()
        {
            var a = new TariffDirection(null, "RussiaExceptHome", "Russia",
                new[] { "ExceptMTS", "ExceptHome", "OverPackage" });
            a.GetKey(true, true).Should().BeEmpty();
        }
    }
}
