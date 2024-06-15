using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Caliburn.Micro
{
    public partial class Conductor<T>
    {
        /// <summary>
        /// An implementation of <see cref="IConductor"/> that holds on many items.
        /// </summary>
        public partial class Collection
        {
            /// <summary>
            /// An implementation of <see cref="IConductor"/> that holds on to many items which are all activated.
            /// </summary>
            public class AllActive : ConductorBase<T>
            {
                private readonly BindableCollection<T> _items = new BindableCollection<T>();
                private readonly bool _openPublicItems;

                /// <summary>
                /// Initializes a new instance of the <see cref="Conductor&lt;T&gt;.Collection.AllActive"/> class.
                /// </summary>
                /// <param name="openPublicItems">if set to <c>true</c> opens public items that are properties of this class.</param>
                public AllActive(bool openPublicItems)
                    : this()
                {
                    _openPublicItems = openPublicItems;
                }

                /// <summary>
                /// Initializes a new instance of the <see cref="Conductor&lt;T&gt;.Collection.AllActive"/> class.
                /// </summary>
                public AllActive()
                {
                    _items.CollectionChanged += (s, e) =>
                    {
                        switch (e.Action)
                        {
                            case NotifyCollectionChangedAction.Add:
                                e.NewItems.OfType<IChild>().Apply(x => x.Parent = this);
                                break;
                            case NotifyCollectionChangedAction.Remove:
                                e.OldItems.OfType<IChild>().Apply(x => x.Parent = null);
                                break;
                            case NotifyCollectionChangedAction.Replace:
                                e.NewItems.OfType<IChild>().Apply(x => x.Parent = this);
                                e.OldItems.OfType<IChild>().Apply(x => x.Parent = null);
                                break;
                            case NotifyCollectionChangedAction.Reset:
                                _items.OfType<IChild>().Apply(x => x.Parent = this);
                                break;
                        }
                    };
                }

                /// <summary>
                /// Gets the items that are currently being conducted.
                /// </summary>
                public IObservableCollection<T> Items => _items;

                /// <summary>
                /// Called when activating.
                /// </summary>
                protected override void OnActivate()
                {
                    Parallel.ForEach(_items.OfType<IActivate>(), item => item.Activate());
                }

                /// <summary>
                /// Called when deactivating.
                /// </summary>
                /// <param name="close">Indicates whether this instance will be closed.</param>
                /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
                /// <returns>A task that represents the asynchronous operation.</returns>
                protected override void OnDeactivate(bool close)
                {
                    foreach (var deactivate in _items.OfType<IDeactivate>())
                    {
                        deactivate.Deactivate(close);
                    }

                    if (close)
                        _items.Clear();
                }

                /// <summary>
                /// Called to check whether or not this instance can close.
                /// </summary>
                /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
                /// <returns>A task that represents the asynchronous operation.</returns>
                public override bool CanClose()
                {
                    var closeResult = CloseStrategy.Execute([.. _items]);

                    if (!closeResult.CloseCanOccur && closeResult.Children.Any())
                    {
                        foreach (var deactivate in closeResult.Children.OfType<IDeactivate>())
                        {
                            deactivate.Deactivate(true);
                        }

                        _items.RemoveRange(closeResult.Children);
                    }

                    return closeResult.CloseCanOccur;
                }

                /// <summary>
                /// Called when initializing.
                /// </summary>
                protected override void OnInitialize()
                {
                    if (_openPublicItems)
                    {
                        var items = GetType().GetRuntimeProperties()
                            .Where(x => x.Name != "Parent" && typeof(T).GetTypeInfo().IsAssignableFrom(x.PropertyType.GetTypeInfo()))
                            .Select(x => x.GetValue(this, null))
                            .Cast<T>();
                        Parallel.ForEach(items, item => ActivateItem(item));
                    }
                }

                /// <summary>
                /// Activates the specified item.
                /// </summary>
                /// <param name="item">The item to activate.</param>
                /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
                /// <returns>A task that represents the asynchronous operation.</returns>
                public override void ActivateItem(T item)
                {
                    if (item == null)
                        return;

                    item = EnsureItem(item);

                    if (IsActive)
                        ScreenExtensions.TryActivate(item);

                    OnActivationProcessed(item, true);
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
                    if (item == null)
                        return;

                    if (close)
                    {
                        var closeResult = CloseStrategy.Execute([item]);

                        if (closeResult.CloseCanOccur)
                            CloseItemCore(item);
                    }
                    else
                        ScreenExtensions.TryDeactivate(item, false);
                }

                /// <summary>
                /// Gets the children.
                /// </summary>
                /// <returns>The collection of children.</returns>
                public override IEnumerable<T> GetChildren()
                {
                    return _items;
                }

                private void CloseItemCore(T item)
                {
                    ScreenExtensions.TryDeactivate(item, true);
                    _items.Remove(item);
                }

                /// <summary>
                /// Ensures that an item is ready to be activated.
                /// </summary>
                /// <param name="newItem">The item that is about to be activated.</param>
                /// <returns>The item to be activated.</returns>
                protected override T EnsureItem(T newItem)
                {
                    var index = _items.IndexOf(newItem);

                    if (index == -1)
                        _items.Add(newItem);
                    else
                        newItem = _items[index];

                    return base.EnsureItem(newItem);
                }
            }
        }
    }
}
