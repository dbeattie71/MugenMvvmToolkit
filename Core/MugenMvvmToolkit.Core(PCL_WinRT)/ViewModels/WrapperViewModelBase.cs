﻿#region Copyright
// ****************************************************************************
// <copyright file="WrapperViewModelBase.cs">
// Copyright © Vyacheslav Volkov 2012-2014
// </copyright>
// ****************************************************************************
// <author>Vyacheslav Volkov</author>
// <email>vvs0205@outlook.com</email>
// <project>MugenMvvmToolkit</project>
// <web>https://github.com/MugenMvvmToolkit/MugenMvvmToolkit</web>
// <license>
// See license.txt in this solution or http://opensource.org/licenses/MS-PL
// </license>
// ****************************************************************************
#endregion
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using JetBrains.Annotations;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Navigation;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;
using MugenMvvmToolkit.Utils;

namespace MugenMvvmToolkit.ViewModels
{
    /// <summary>
    ///     Represents the base wrapper for view models.
    /// </summary>
    public abstract class WrapperViewModelBase<TViewModel> : ViewModelBase, ICloseableViewModel, INavigableViewModel,
                                                             IHasOperationResult, IHasDisplayName, ISelectable, IWrapperViewModel, IHasState
        where TViewModel : class, IViewModel
    {
        #region Fields

        private readonly object _locker;
        private readonly Dictionary<string, string> _wrappedPropertyNames;
        private ICommand _closeCommand;
        private string _displayName;
        private EventHandler<ICloseableViewModel, ViewModelClosedEventArgs> _internalClosedEvent;
        private EventHandler<ICloseableViewModel, ViewModelClosingEventArgs> _internalClosingEvent;
        private bool _isSelected;
        private TViewModel _viewModel;
        private bool? _operationResult;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="WrapperViewModelBase{TViewModel}" /> class.
        /// </summary>
        protected WrapperViewModelBase()
        {
            _locker = new object();
            _closeCommand = new RelayCommand(ExecuteClose);
            _wrappedPropertyNames = new Dictionary<string, string>
            {
                {"CloseCommand", "CloseCommand"},
                {"DisplayName", "DisplayName"},
                {"IsSelected", "IsSelected"},
                {"OperationResult", "OperationResult"}
            };
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the current <see cref="IViewModel" />.
        /// </summary>
        public TViewModel ViewModel
        {
            get { return _viewModel; }
        }

        /// <summary>
        ///     Gets the collection of properties that should be retranslated use property changed handler.
        /// </summary>
        protected IDictionary<string, string> WrappedPropertyNames
        {
            get { return _wrappedPropertyNames; }
        }

        #endregion

        #region Implementation of interfaces

        /// <summary>
        ///     Gets the current <see cref="IViewModel" />.
        /// </summary>
        IViewModel IWrapperViewModel.ViewModel
        {
            get { return _viewModel; }
        }

        /// <summary>
        ///     Gets or sets a command that attempts to remove this workspace from the user interface.
        /// </summary>
        public virtual ICommand CloseCommand
        {
            get
            {
                var closeableViewModel = ViewModel as ICloseableViewModel;
                if (closeableViewModel == null)
                    return _closeCommand;
                return closeableViewModel.CloseCommand;
            }
            set
            {
                if (CloseCommand == value)
                    return;
                _closeCommand = value;
                var closeableViewModel = ViewModel as ICloseableViewModel;
                if (closeableViewModel == null)
                    _closeCommand = value;
                else
                    closeableViewModel.CloseCommand = value;
                OnPropertyChanged("CloseCommand");
            }
        }

        /// <summary>
        ///     Wraps the specified view-model.
        /// </summary>
        public void Wrap(IViewModel viewModel, IDataContext context = null)
        {
            EnsureNotDisposed();
            Should.NotBeNull(viewModel, "viewModel");
            lock (_locker)
            {
                if (_viewModel != null)
                    throw ExceptionManager.ObjectInitialized("ViewModel", viewModel);
                _viewModel = (TViewModel)viewModel;
            }
            ViewModel.PropertyChanged += ViewModelOnPropertyChanged;
            Closed += OnClosed;
            var closeableViewModel = ViewModel as ICloseableViewModel;
            if (closeableViewModel != null)
            {
                var internalClosedEvent = _internalClosedEvent;
                if (internalClosedEvent != null)
                {
                    _internalClosedEvent = null;
                    foreach (var @delegate in internalClosedEvent.GetInvocationList())
                        closeableViewModel.Closed += (EventHandler<ICloseableViewModel, ViewModelClosedEventArgs>)@delegate;
                }
                var internalClosingEvent = _internalClosingEvent;
                if (internalClosingEvent != null)
                {
                    _internalClosingEvent = null;
                    foreach (var @delegate in internalClosingEvent.GetInvocationList())
                        closeableViewModel.Closing += (EventHandler<ICloseableViewModel, ViewModelClosingEventArgs>)@delegate;
                }
            }
            ViewModel.Subscribe(this);
            OnWrapped(context);

            //Invalidating properties.
            OnPropertyChanged(string.Empty);
        }

        /// <summary>
        ///     Tries to close view-model.
        /// </summary>
        /// <param name="parameter">The specified parameter, if any.</param>
        /// <returns>An instance of task with result.</returns>
        Task<bool> ICloseableViewModel.CloseAsync(object parameter)
        {
            var closeableViewModel = ViewModel as ICloseableViewModel;
            if (closeableViewModel != null)
                return closeableViewModel.CloseAsync(parameter);
            var closingEvent = _internalClosingEvent;
            ViewModelClosedEventArgs closedArgs = null;
            if (closingEvent != null)
            {
                var args = new ViewModelClosingEventArgs(this, parameter);
                closingEvent(this, args);
                if (args.Cancel)
                    return MvvmUtils.FalseTaskResult;
                closedArgs = args;
            }
            var closedEvent = _internalClosedEvent;
            if (closedEvent != null)
                closedEvent(this, closedArgs ?? new ViewModelClosedEventArgs(this, parameter));
            return MvvmUtils.TrueTaskResult;
        }

        /// <summary>
        ///     Occurs when <see cref="ICloseableViewModel" /> is closing.
        /// </summary>
        public event EventHandler<ICloseableViewModel, ViewModelClosingEventArgs> Closing
        {
            add
            {
                var closeableViewModel = ViewModel as ICloseableViewModel;
                if (closeableViewModel == null)
                {
                    lock (_locker)
                    {
                        closeableViewModel = ViewModel as ICloseableViewModel;
                        if (closeableViewModel == null)
                        {
                            _internalClosingEvent += value;
                            return;
                        }
                    }
                }
                closeableViewModel.Closing += value;
            }
            remove
            {
                var closeableViewModel = ViewModel as ICloseableViewModel;
                if (closeableViewModel == null)
                {
                    lock (_locker)
                    {
                        closeableViewModel = ViewModel as ICloseableViewModel;
                        if (closeableViewModel == null)
                        {
                            _internalClosingEvent -= value;
                            return;
                        }
                    }
                }
                closeableViewModel.Closing -= value;
            }
        }

        /// <summary>
        ///     Occurs when <see cref="ICloseableViewModel" /> is closed.
        /// </summary>
        public event EventHandler<ICloseableViewModel, ViewModelClosedEventArgs> Closed
        {
            add
            {
                var closeableViewModel = ViewModel as ICloseableViewModel;
                if (closeableViewModel == null)
                {
                    lock (_locker)
                    {
                        closeableViewModel = ViewModel as ICloseableViewModel;
                        if (closeableViewModel == null)
                        {
                            _internalClosedEvent += value;
                            return;
                        }
                    }
                }
                closeableViewModel.Closed += value;
            }
            remove
            {
                var closeableViewModel = ViewModel as ICloseableViewModel;
                if (closeableViewModel == null)
                {
                    lock (_locker)
                    {
                        closeableViewModel = ViewModel as ICloseableViewModel;
                        if (closeableViewModel == null)
                        {
                            _internalClosedEvent -= value;
                            return;
                        }
                    }
                }
                closeableViewModel.Closed -= value;
            }
        }


        /// <summary>
        ///     Gets or sets the display name for the current model.
        /// </summary>
        public virtual string DisplayName
        {
            get
            {
                var hasDisplayName = ViewModel as IHasDisplayName;
                if (hasDisplayName == null)
                    return _displayName;
                return hasDisplayName.DisplayName;
            }
            set
            {
                if (DisplayName == value)
                    return;
                var hasDisplayName = ViewModel as IHasDisplayName;
                if (hasDisplayName == null)
                    _displayName = value;
                else
                    hasDisplayName.DisplayName = value;
                OnPropertyChanged("DisplayName");
            }
        }

        /// <summary>
        ///     Gets or sets the operation result value.
        /// </summary>
        public virtual bool? OperationResult
        {
            get
            {
                var hasOperationResult = ViewModel as IHasOperationResult;
                if (hasOperationResult == null)
                    return _operationResult;
                return hasOperationResult.OperationResult;
            }
            set
            {
                if (OperationResult == value)
                    return;
                var hasOperationResult = ViewModel as IHasOperationResult;
                if (hasOperationResult == null)
                    _operationResult = value;
                else
                    hasOperationResult.OperationResult = value;
                OnPropertyChanged("OperationResult");
            }
        }

        /// <summary>
        ///     Gets or sets the property that indicates that current model is selected.
        /// </summary>
        public virtual bool IsSelected
        {
            get
            {
                var selectable = ViewModel as ISelectable;
                if (selectable == null)
                    return _isSelected;
                return selectable.IsSelected;
            }
            set
            {
                if (value == IsSelected)
                    return;
                var selectable = ViewModel as ISelectable;
                if (selectable == null)
                    _isSelected = value;
                else
                    selectable.IsSelected = value;
                OnPropertyChanged("IsSelected");
            }
        }

        /// <summary>
        ///     Called when a view-model becomes the active view-model in a frame.
        /// </summary>
        /// <param name="context">
        ///     The specified <see cref="INavigationContext" />.
        /// </param>
        void INavigableViewModel.OnNavigatedTo(INavigationContext context)
        {
            var navigableViewModel = ViewModel as INavigableViewModel;
            if (navigableViewModel != null)
                navigableViewModel.OnNavigatedTo(context);
            OnShow(context);
        }

        /// <summary>
        ///     Called just before a view-model is no longer the active view-model in a frame.
        /// </summary>
        /// <param name="context">
        ///     The specified <see cref="INavigationContext" />.
        /// </param>
        Task<bool> INavigableViewModel.OnNavigatingFrom(INavigationContext context)
        {
            var navigableViewModel = ViewModel as INavigableViewModel;
            if (navigableViewModel != null)
                return navigableViewModel.OnNavigatingFrom(context);
            return MvvmUtils.TrueTaskResult;
        }

        /// <summary>
        ///     Called when a view-model is no longer the active view-model in a frame.
        /// </summary>
        /// <param name="context">
        ///     The specified <see cref="INavigationContext" />.
        /// </param>
        void INavigableViewModel.OnNavigatedFrom(INavigationContext context)
        {
            var navigableViewModel = ViewModel as INavigableViewModel;
            if (navigableViewModel != null)
                navigableViewModel.OnNavigatedFrom(context);
        }

        /// <summary>
        ///     Loads state.
        /// </summary>
        void IHasState.LoadState(IDataContext state)
        {
            if (ViewModel == null)
            {
                string typeName;
                if (state.TryGetData(ViewModelConstants.ViewModelTypeName, out typeName))
                {
                    Tracer.Info("Trying to restore view model '{0}' from wrapper '{1}'", typeName, this);
                    var vmType = Type.GetType(typeName, false);
                    if (vmType != null)
                        Wrap(GetViewModel(vmType));
                }
            }
            var hasState = ViewModel as IHasState;
            if (hasState != null)
                hasState.LoadState(state);
            OnLoadState(state);
        }

        /// <summary>
        ///     Saves state.
        /// </summary>
        void IHasState.SaveState(IDataContext state)
        {
            OnSaveState(state);
            if (ViewModel == null)
                return;
            object data;
            if (ViewModel.Settings.Metadata.TryGetData(ViewModelConstants.StateManager, out data) && data != null)
                return;

            state.AddOrUpdate(ViewModelConstants.ViewModelTypeName, ViewModel.GetType().AssemblyQualifiedName);
            var hasState = ViewModel as IHasState;
            if (hasState != null)
                hasState.SaveState(state);
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Tries to close view-model.
        /// </summary>
        protected Task<bool> CloseAsync(object parameter = null)
        {
            var t = MvvmUtils.TryCloseAsync(this, parameter, null);
            t.WithTaskExceptionHandler(this);
            return t;
        }

        /// <summary>
        ///     Occurs when view model was wrapped.
        /// </summary>
        protected virtual void OnWrapped(IDataContext context)
        {
        }

        /// <summary>
        ///     Occurs when view model is closed.
        /// </summary>
        protected virtual void OnClosed([CanBeNull] object parameter)
        {
        }

        /// <summary>
        ///     Occurs when view model is shown.
        /// </summary>
        protected virtual void OnShow([CanBeNull] object parameter)
        {
        }

        /// <summary>
        ///     Loads state.
        /// </summary>
        protected virtual void OnLoadState(IDataContext state)
        {
        }

        /// <summary>
        ///     Saves state.
        /// </summary>
        protected virtual void OnSaveState(IDataContext state)
        {
        }

        private void ViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            string value;
            if (string.IsNullOrEmpty(args.PropertyName))
                OnPropertyChanged(args, PropertyChangeExecutionMode);
            else if (WrappedPropertyNames.TryGetValue(args.PropertyName, out value))
            {
                if (args.PropertyName != value)
                    args = new PropertyChangedEventArgs(value);
                OnPropertyChanged(args, PropertyChangeExecutionMode);
            }
        }

        private void ExecuteClose(object o)
        {
            CloseAsync(o);
        }

        private void OnClosed(object sender, ViewModelClosedEventArgs args)
        {
            OnClosed(args.Parameter);
        }

        #endregion

        #region Overrides of ViewModelBase

        /// <summary>
        ///     Occurs after the current view model is disposed, use for clear resource and event listeners(Internal only).
        /// </summary>
        internal override void OnDisposeInternal(bool disposing)
        {
            if (disposing)
            {
                Closed -= OnClosed;
                _internalClosingEvent = null;
                _internalClosedEvent = null;
                if (_viewModel != null)
                    _viewModel.PropertyChanged -= ViewModelOnPropertyChanged;
            }
            base.OnDisposeInternal(disposing);
        }

        #endregion
    }
}