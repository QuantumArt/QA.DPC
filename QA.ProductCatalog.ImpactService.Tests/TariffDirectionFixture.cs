using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace QA.ProductCatalog.ImpactService.Tests
{
    [TestFixture]
    public class TariffDirectionFixture
    {
        [Test]
        public void GetKey_SimpleTariffDirection()
        {
            var a = new TariffDirection("OutgoingCalls", "RussiaExceptHome", "Russia",
                new[] {"ExceptMTS", "ExceptHome", "WithinPackage"});
            Assert.That(a.GetKey(),
                Is.EqualTo(
                    "BaseParameter: OutgoingCalls; Zone: RussiaExceptHome; Direction: Russia; BaseParameterModifiers: ExceptHome,ExceptMTS;"));
        }

        [Test]
        public void GetKey_BaseParameter()
        {
            var a = new TariffDirection("IncomingCalls", null, null, null);
            Assert.That(a.GetKey(),
                Is.EqualTo(
                    "BaseParameter: IncomingCalls; Zone: ; Direction: ; BaseParameterModifiers: ;"));
        }

        [Test]
        public void GetKey_SimpleTariffDirectionWithSpecials()
        {
            var a = new TariffDirection("OutgoingCalls", "RussiaExceptHome", "Russia",
                new[] { "ExceptMTS", "ExceptHome", "OverPackage" });
            Assert.That(a.GetKey(false),
                Is.EqualTo(
                    "BaseParameter: OutgoingCalls; Zone: RussiaExceptHome; Direction: Russia; BaseParameterModifiers: ExceptHome,ExceptMTS,OverPackage;"));
        }

        [Test]
        public void GetKey_EmptyTariffDirection()
        {
            var a = new TariffDirection(null, null, null, null);
            Assert.That(a.GetKey(true, true), Is.Empty);
        }

        [Test]
        public void GetKey_SimpleTariffDirectionWithoutBaseParameter()
        {
            var a = new TariffDirection(null, "RussiaExceptHome", "Russia",
                new[] { "ExceptMTS", "ExceptHome", "OverPackage" });
            Assert.That(a.GetKey(true, true), Is.Empty);
        }

    }
}
