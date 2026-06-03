using System;
using Crashmania.Game;
using NUnit.Framework;

namespace Crashmania.Tests
{
    public sealed class CrashCurveEvaluatorTests
    {
        private const double BaseMultiplier = 1.006;

        [Test]
        public void MultiplierAtTenSecondsMatchesPreviousOneSecondCurvePoint()
        {
            var previousOneSecondMultiplier = Math.Pow(BaseMultiplier, 100.0);

            var multiplier = CrashCurveEvaluator.GetMultiplierAtTime(10f);

            Assert.AreEqual(previousOneSecondMultiplier, multiplier, 0.0000001);
        }

        [Test]
        public void TimeAtMultiplierRoundTripsRepresentativeValues()
        {
            var secondsValues = new[] { 0f, 1f, 5f, 10f, 20f };
            foreach (var seconds in secondsValues)
            {
                var multiplier = CrashCurveEvaluator.GetMultiplierAtTime(seconds);
                var roundTripSeconds = CrashCurveEvaluator.GetTimeAtMultiplier(multiplier);

                Assert.AreEqual(seconds, roundTripSeconds, 0.0001f);
            }
        }

        [Test]
        public void MultiplierRemainsMonotonicAndAccelerating()
        {
            var atZero = CrashCurveEvaluator.GetMultiplierAtTime(0f);
            var atOne = CrashCurveEvaluator.GetMultiplierAtTime(1f);
            var atTwo = CrashCurveEvaluator.GetMultiplierAtTime(2f);
            var atThree = CrashCurveEvaluator.GetMultiplierAtTime(3f);

            Assert.Greater(atOne, atZero);
            Assert.Greater(atTwo, atOne);
            Assert.Greater(atThree, atTwo);
            Assert.Greater(atTwo - atOne, atOne - atZero);
            Assert.Greater(atThree - atTwo, atTwo - atOne);
        }
    }
}
