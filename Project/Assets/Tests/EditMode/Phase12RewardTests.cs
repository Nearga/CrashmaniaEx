using System;
using System.Reflection;
using Crashmania.Game;
using Crashmania.Models;
using Crashmania.UI.Components;
using Crashmania.UI.Game;
using NUnit.Framework;
using UnityEngine;

namespace Crashmania.Tests
{
    public sealed class Phase12RewardTests
    {
        [TestCase(1.0, 4)]
        [TestCase(1.3, 4)]
        [TestCase(2.225, 5)]
        [TestCase(3.15, 6)]
        [TestCase(4.075, 7)]
        [TestCase(5.0, 8)]
        [TestCase(20.0, 8)]
        public void CoinCountUsesClampedRoundedInterpolation(double multiplier, int expected)
        {
            var root = new GameObject("RewardFlyout", typeof(RectTransform), typeof(CurrencyRewardFlyout));
            try
            {
                var flyout = root.GetComponent<CurrencyRewardFlyout>();
                Assert.AreEqual(expected, flyout.CalculateCoinCount(multiplier));
                Assert.AreEqual(16, flyout.RequiredPoolCapacity);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void BetPanelOnlyBlocksCurrencyDuringPendingAndFlight()
        {
            var root = new GameObject("BetPanel", typeof(RectTransform));
            try
            {
                var panel = root.AddComponent<BetPanelController>();
                Assert.IsFalse(panel.BlocksCurrencyToggle);

                InvokePrivate(panel, "SetState", BetPanelState.Pending);
                Assert.IsTrue(panel.BlocksCurrencyToggle);

                InvokePrivate(panel, "SetState", BetPanelState.InFlight);
                Assert.IsTrue(panel.BlocksCurrencyToggle);

                InvokePrivate(panel, "SetState", BetPanelState.Won);
                Assert.IsFalse(panel.BlocksCurrencyToggle);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void SuccessfulResolutionEmitsRewardAndRoundEndDoesNotCreditIt()
        {
            var controllerObject = new GameObject("CrashGameController");
            var panelObject = new GameObject("BetPanel", typeof(RectTransform));
            var sourceObject = new GameObject("RewardSource", typeof(RectTransform));
            try
            {
                sourceObject.transform.SetParent(panelObject.transform, false);
                var panel = panelObject.AddComponent<BetPanelController>();
                SetPrivate(panel, "panelId", "PanelA");
                SetPrivate(panel, "rewardSource", sourceObject.GetComponent<RectTransform>());

                var controller = controllerObject.AddComponent<CrashGameController>();
                SetPrivate(controller, "betPanels", new[] { panel });

                var rewardCount = 0;
                var positiveBalanceCredit = 0.0;
                controller.RewardEarned += reward =>
                {
                    rewardCount++;
                    Assert.AreEqual(12000.0, reward.Payout);
                    Assert.AreSame(sourceObject.GetComponent<RectTransform>(), reward.Source);
                };
                controller.OnBalanceChanged += (cc, sc) => positiveBalanceCredit += Math.Max(0.0, cc) + Math.Max(0.0, sc);

                InvokePrivate(controller, "OnBetResolved",
                    new CrashBetResolution("PanelA", true, 6000.0, CurrencyMode.CC, 2.0, 12000.0));
                InvokePrivate(controller, "OnRoundEnded", new CrashRoundEndedEvent(1, 3.0));

                Assert.AreEqual(1, rewardCount);
                Assert.AreEqual(0.0, positiveBalanceCredit);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(controllerObject);
                UnityEngine.Object.DestroyImmediate(panelObject);
            }
        }

        private static void SetPrivate(object target, string fieldName, object value)
        {
            target.GetType()
                .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
                ?.SetValue(target, value);
        }

        private static void InvokePrivate(object target, string methodName, params object[] arguments)
        {
            target.GetType()
                .GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)
                ?.Invoke(target, arguments);
        }
    }
}
