using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using WaterLog_Backend;
using WaterLog_Backend.Models;

namespace UnitTesting
{
    class SeverityTest
    {
        [TestCase]
        public void HighSeverity() {
            Procedures p = new Procedures();
            var s = p.CalculateSeverityGivenValue(120);
            Assert.AreEqual("High", s);
        }
        [TestCase]
        public void MediumSeverity()
        {
            Procedures p = new Procedures();
            var s = p.CalculateSeverityGivenValue(70);
            Assert.AreEqual("Medium", s);
        }
        [TestCase]
        public void LowSeverity()
        {
            Procedures p = new Procedures();
            var s = p.CalculateSeverityGivenValue(20);
            Assert.AreEqual("Low", s);
        }
    }
}
