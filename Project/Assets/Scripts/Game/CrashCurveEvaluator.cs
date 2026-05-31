using System;

namespace Crashmania.Game
{
    public static class CrashCurveEvaluator
    {
        private const double BaseMultiplier = 1.006;
        private const double TicksPerSecond = 100.0;

        public static double GetMultiplierAtTime(float seconds)
        {
            return Math.Pow(BaseMultiplier, TicksPerSecond * Math.Max(0f, seconds));
        }

        public static float GetTimeAtMultiplier(double multiplier)
        {
            if (multiplier <= 1.0)
            {
                return 0f;
            }

            return (float)(Math.Log(multiplier) / (TicksPerSecond * Math.Log(BaseMultiplier)));
        }
    }
}
