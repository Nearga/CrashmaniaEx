using System;
using System.Collections.Generic;
using Crashmania.Config;
using Crashmania.Models;
using Crashmania.Services;
using Crashmania.UI.Game;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace Crashmania.Tests
{
    public sealed class BetPanelControllerTests
    {
        private const string TestPanelId = "PanelA";

        [Test]
        public void AutoplayCountdownDoesNotPlaceBet()
        {
            var fixture = CreatePanel();
            try
            {
                fixture.AutoplayToggle.isOn = true;

                fixture.Panel.OnCountdown();

                Assert.AreEqual(0, fixture.Service.PlaceBetCount);
                Assert.IsTrue(fixture.Panel.Autoplay.Enabled);
            }
            finally
            {
                fixture.Destroy();
            }
        }

        [Test]
        public void ManualBetUsesConfiguredAutoCashOutMultiplier()
        {
            var fixture = CreatePanel();
            try
            {
                fixture.AutoplayToggle.isOn = true;
                fixture.Panel.Autoplay.RemainingRounds = 10;
                fixture.Panel.Autoplay.CashOutMultiplier = 2.3;

                fixture.ActionButton.onClick.Invoke();

                Assert.AreEqual(1, fixture.Service.PlaceBetCount);
                Assert.AreEqual(2.3, fixture.Service.LastAutoCashOutMultiplier);
                Assert.AreEqual(BetPanelState.Pending, fixture.Panel.State);
            }
            finally
            {
                fixture.Destroy();
            }
        }

        [Test]
        public void CancelPendingBetKeepsAutoplayEnabled()
        {
            var fixture = CreatePanel();
            try
            {
                fixture.AutoplayToggle.isOn = true;

                fixture.ActionButton.onClick.Invoke();
                fixture.ActionButton.onClick.Invoke();

                Assert.AreEqual(1, fixture.Service.CancelBetCount);
                Assert.IsTrue(fixture.Panel.Autoplay.Enabled);
                Assert.AreEqual(BetPanelState.Idle, fixture.Panel.State);
            }
            finally
            {
                fixture.Destroy();
            }
        }

        [Test]
        public void FiniteAutoplayDisablesAfterLastAutoCashOutArmedBetResolves()
        {
            var fixture = CreatePanel();
            try
            {
                fixture.AutoplayToggle.isOn = true;
                fixture.Panel.Autoplay.RemainingRounds = 1;
                fixture.Panel.Autoplay.CashOutMultiplier = 1.8;

                fixture.ActionButton.onClick.Invoke();
                fixture.Panel.Resolve(new CrashBetResolution(TestPanelId, true, 6000.0, CurrencyMode.CC, 1.8, 10800.0));

                Assert.AreEqual(1, fixture.Service.PlaceBetCount);
                Assert.AreEqual(1.8, fixture.Service.LastAutoCashOutMultiplier);
                Assert.IsFalse(fixture.Panel.Autoplay.Enabled);
                Assert.AreEqual(0, fixture.Panel.Autoplay.RemainingRounds);
            }
            finally
            {
                fixture.Destroy();
            }
        }

        private static PanelFixture CreatePanel()
        {
            var root = new GameObject("BetPanel", typeof(RectTransform));
            var actionButton = CreateButton(root.transform, "ActionButton");
            var autoplayToggle = CreateToggle(root.transform, "AutoplayToggle");
            autoplayToggle.isOn = false;
            var panel = root.AddComponent<BetPanelController>();
            var service = new FakeCrashGameService();

            panel.Initialize(TestPanelId, service, CurrencyMode.CC);

            return new PanelFixture(root, panel, service, actionButton, autoplayToggle);
        }

        private static Button CreateButton(Transform parent, string name)
        {
            var buttonObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);
            return buttonObject.GetComponent<Button>();
        }

        private static Toggle CreateToggle(Transform parent, string name)
        {
            var toggleObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Toggle));
            toggleObject.transform.SetParent(parent, false);
            return toggleObject.GetComponent<Toggle>();
        }

        private sealed class PanelFixture
        {
            public PanelFixture(GameObject root, BetPanelController panel, FakeCrashGameService service, Button actionButton, Toggle autoplayToggle)
            {
                Root = root;
                Panel = panel;
                Service = service;
                ActionButton = actionButton;
                AutoplayToggle = autoplayToggle;
            }

            public GameObject Root { get; }
            public BetPanelController Panel { get; }
            public FakeCrashGameService Service { get; }
            public Button ActionButton { get; }
            public Toggle AutoplayToggle { get; }

            public void Destroy()
            {
                UnityEngine.Object.DestroyImmediate(Root);
            }
        }

        private sealed class FakeCrashGameService : ICrashGameService
        {
            public CrashGamePhase CurrentPhase => CrashGamePhase.Preparation;
            public double CurrentMultiplier => 1.0;
            public bool IsRunning => true;

            public int PlaceBetCount { get; private set; }
            public int CancelBetCount { get; private set; }
            public int CashOutCount { get; private set; }
            public double? LastAutoCashOutMultiplier { get; private set; }

#pragma warning disable CS0067
            public event Action<CrashCountdownEvent> CountdownTick;
            public event Action<CrashRoundStartedEvent> RoundStarted;
            public event Action<CrashMultiplierEvent> MultiplierUpdated;
            public event Action<CrashRoundEndedEvent> RoundEnded;
            public event Action<int> IntermissionStarted;
            public event Action<IReadOnlyList<CrashPlayerBet>> PlayerBetsUpdated;
            public event Action<CrashBetResolution> BetResolved;
#pragma warning restore CS0067

            public UniTask StartLoop(AppConfig config)
            {
                return UniTask.CompletedTask;
            }

            public void StopLoop()
            {
            }

            public bool PlaceBet(string panelId, double amount, CurrencyMode currency, double? autoCashOutMultiplier)
            {
                PlaceBetCount++;
                LastAutoCashOutMultiplier = autoCashOutMultiplier;
                return true;
            }

            public bool CancelBet(string panelId)
            {
                CancelBetCount++;
                return true;
            }

            public bool CashOut(string panelId)
            {
                CashOutCount++;
                return true;
            }
        }
    }
}
