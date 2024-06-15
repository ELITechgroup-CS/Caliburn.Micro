using System.Collections.Generic;

namespace Caliburn.Micro
{
    /// <summary>
    /// An implementation of <see cref="IConductor"/> that holds on to and activates only one item at a time.
    /// </summary>
    public partial class Conductor<T> : ConductorBaseWithActiveItem<T> where T : class
    {
        /// <inheritdoc />
        public override void ActivateItem(T item)
        {
            if (item != null && item.Equals(ActiveItem))
            {
                if (IsActive)
                {
                    ScreenExtensions.TryActivate(item);
                    OnActivationProcessed(item, true);
                }
                return;
            }

            var closeResult = CloseStrategy.Execute([ActiveItem]);

            if (closeResult.CloseCanOccur)
            {
                ChangeActiveItem(item, true);
            }
            else
            {
                OnActivationProcessed(item, false);
            }
        }

        /// <summary>
        /// Deactivates the specified item.
        /// </summary>
        /// <param name="item">The item to close.</param>
        /// <param name="close">Indicates whether or not to close the item after deactivating it.</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public override void DeactivateItem(T item, bool close)
        {
            if (item == null || !item.Equals(ActiveItem))
            {
                return;
            }

            var closeResult = CloseStrategy.Execute([ActiveItem]);

            if (closeResult.CloseCanOccur)
            {
                ChangeActiveItem(default(T), close);
            }
        }

        /// <summary>
        /// Called to check whether or not this instance can close.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public override bool CanClose()
        {
            var closeResult = CloseStrategy.Execute([ActiveItem]);

            return closeResult.CloseCanOccur;
        }

        /// <summary>
        /// Called when activating.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        protected override void OnActivate()
        {
            ScreenExtensions.TryActivate(ActiveItem);
        }

        /// <summary>
        /// Called when deactivating.
        /// </summary>
        /// <param name="close">Indicates whether this instance will be closed.</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        protected override void OnDeactivate(bool close)
        {
            ScreenExtensions.TryDeactivate(ActiveItem, close);
        }

        /// <summary>
        /// Gets the children.
        /// </summary>
        /// <returns>The collection of children.</returns>
        public override IEnumerable<T> GetChildren()
        {
            return [ActiveItem];
        }
    }
}
