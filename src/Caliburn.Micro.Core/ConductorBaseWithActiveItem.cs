namespace Caliburn.Micro
{
    /// <summary>
    /// A base class for various implementations of <see cref="IConductor"/> that maintain an active item.
    /// </summary>
    /// <typeparam name="T">The type that is being conducted.</typeparam>
    public abstract class ConductorBaseWithActiveItem<T> : ConductorBase<T>, IConductActiveItem where T : class
    {
        private T _activeItem;

        /// <summary>
        /// The currently active item.
        /// </summary>
        public T ActiveItem
        {
            get => _activeItem;
            set => ActivateItem(value);
        }

        /// <summary>
        /// The currently active item.
        /// </summary>
        /// <value></value>
        object IHaveActiveItem.ActiveItem
        {
            get => ActiveItem;
            set => ActiveItem = (T)value;
        }

        /// <summary>
        /// Changes the active item.
        /// </summary>
        /// <param name="newItem">The new item to activate.</param>
        /// <param name="closePrevious">Indicates whether or not to close the previous active item.</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        protected virtual void ChangeActiveItem(T newItem, bool closePrevious)
        {
            ScreenExtensions.TryDeactivate(_activeItem, closePrevious);
            newItem = EnsureItem(newItem);

            _activeItem = newItem;
            NotifyOfPropertyChange(nameof(ActiveItem));

            if (IsActive)
                ScreenExtensions.TryActivate(newItem);

            OnActivationProcessed(_activeItem, true);
        }
    }
}
