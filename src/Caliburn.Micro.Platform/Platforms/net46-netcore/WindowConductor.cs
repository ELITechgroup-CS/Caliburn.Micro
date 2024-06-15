using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Caliburn.Micro
{
    public class WindowConductor
    {
        private bool deactivatingFromView;
        private bool deactivateFromViewModel;
        private bool actuallyClosing;
        private readonly Window view;
        private readonly object model;

        public WindowConductor(object model, Window view)
        {
            this.model = model;
            this.view = view;
        }

        public void Initialise()
        {
            if (model is IActivate activator)
            {
                activator.Activate();
            }

            if (model is IDeactivate deactivatable)
            {
                view.Closed += Closed;
                deactivatable.Deactivated += Deactivated;
            }

            if (model is IGuardClose guard)
            {
                view.Closing += Closing;
            }
        }

        private void Closed(object sender, EventArgs e)
        {
            view.Closed -= Closed;
            view.Closing -= Closing;

            if (deactivateFromViewModel)
            {
                return;
            }

            var deactivatable = (IDeactivate)model;

            deactivatingFromView = true;
            deactivatable.Deactivate(true);
            deactivatingFromView = false;
        }

        private void Deactivated(object sender, DeactivationEventArgs e)
        {
            if (!e.WasClosed)
            {
                return;
            }

            ((IDeactivate)model).Deactivated -= Deactivated;

            if (deactivatingFromView)
            {
                return;
            }

            deactivateFromViewModel = true;
            actuallyClosing = true;
            view.Close();
            actuallyClosing = false;
            deactivateFromViewModel = false;
        }

        private void Closing(object sender, CancelEventArgs e)
        {
            if (e.Cancel)
            {
                return;
            }

            var guard = (IGuardClose)model;

            if (actuallyClosing)
            {
                actuallyClosing = false;
                return;
            }

            var cachedDialogResult = view.DialogResult;

            e.Cancel = true;

            var canClose = guard.CanClose();

            if (!canClose)
                return;

            actuallyClosing = true;

            if (cachedDialogResult == null)
            {
                view.Close();
            }
            else if (view.DialogResult != cachedDialogResult)
            {
                view.DialogResult = cachedDialogResult;
            }
        }
    }
}
