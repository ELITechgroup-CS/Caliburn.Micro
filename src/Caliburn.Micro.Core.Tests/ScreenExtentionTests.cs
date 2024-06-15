using System;
using System.Threading;
using Xunit;

namespace Caliburn.Micro.Core.Tests
{
    public class ConductWithTests
    {
        [Fact]
        public void Screen_ConductWithTests()
        {
            var root = new Screen();

            var child1 = new StateScreen
            {
                DisplayName = "screen1"
            };
            // simulate a long deactivation process 
            var child2 = new StateScreen(TimeSpan.FromSeconds(3))
            {
                DisplayName = "screen2"
            };

            var child3 = new StateScreen()
            {
                DisplayName = "screen3"
            };

            child1.ConductWith(root);
            child2.ConductWith(root);
            child3.ConductWith(root);

            ScreenExtensions.TryActivate(root);

            Assert.True(child1.WasActivated, "child 1 should be active");
            Assert.True(child2.WasActivated, "child 2 should be active");
            Assert.True(child3.WasActivated, "child 3 should be active");

            ScreenExtensions.TryDeactivate(root, true);

            Assert.True(child1.IsClosed, "child 1 should be closed");
            Assert.True(child2.IsClosed, "child 2 should be closed");
            Assert.True(child3.IsClosed, "child 3 should be closed");
        }

        [Fact]
        public void Conductor_ConductWithTests()
        {
            var root = new Conductor<StateScreen>.Collection.AllActive();

            var child1 = new StateScreen
            {
                DisplayName = "screen1"
            };
            var child2 = new StateScreen(TimeSpan.FromSeconds(3))
            {
                DisplayName = "screen2",
                IsClosable = false,
            };

            var child3 = new StateScreen()
            {
                DisplayName = "screen3"
            };

            root.Items.Add(child1);
            root.Items.Add(child2);
            root.Items.Add(child3);

            ScreenExtensions.TryActivate(root);

            Assert.True(child1.WasActivated, "child 1 should be active");
            Assert.True(child2.WasActivated, "child 2 should be active");
            Assert.True(child3.WasActivated, "child 3 should be active");

            ScreenExtensions.TryDeactivate(root, true);

            Assert.True(child1.IsClosed, "child 1 should be closed");
            Assert.True(child2.IsClosed, "child 2 should be closed");
            Assert.True(child3.IsClosed, "child 3 should be closed");
        }

        class StateScreen : Screen
        {
            public StateScreen()
            {
            }

            public StateScreen(TimeSpan? deactivationDelay)
            {
                this.deactivationDelay = deactivationDelay;
            }

            public bool WasActivated { get; private set; }
            public bool IsClosed { get; private set; }
            public bool IsClosable { get; set; }

            public override bool CanClose()
            {
                return IsClosable;
            }

            protected override void OnActivate()
            {
                if (deactivationDelay.HasValue)
                {
                    Thread.Sleep(deactivationDelay.Value);
                }

                base.OnActivate();

                WasActivated = true;
                IsClosable = false;
            }

            protected override void OnDeactivate(bool close)
            {
                base.OnDeactivate(close);

                if (deactivationDelay.HasValue)
                {
                    Thread.Sleep(deactivationDelay.Value);
                }

                IsClosed = close;
            }

            private readonly TimeSpan? deactivationDelay;
        }
    }
}
