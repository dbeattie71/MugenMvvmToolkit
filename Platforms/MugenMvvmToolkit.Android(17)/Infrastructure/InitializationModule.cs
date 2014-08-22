﻿#region Copyright
// ****************************************************************************
// <copyright file="InitializationModule.cs">
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
using Android.App;
using MugenMvvmToolkit.Infrastructure.Callbacks;
using MugenMvvmToolkit.Infrastructure.Navigation;
using MugenMvvmToolkit.Infrastructure.Presenters;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Callbacks;
using MugenMvvmToolkit.Interfaces.Navigation;
using MugenMvvmToolkit.Interfaces.Presenters;
using MugenMvvmToolkit.Models.IoC;

namespace MugenMvvmToolkit.Infrastructure
{
    /// <summary>
    ///     Represents the class that is used to initialize the IOC adapter.
    /// </summary>
    public class InitializationModule : InitializationModuleBase
    {
        #region Overrides of InitializationModuleBase

        /// <summary>
        ///     Loads the current module.
        /// </summary>
        protected override bool LoadInternal()
        {
            Context.IocContainer.BindToBindingInfo(GetViewFactory());
            Context.IocContainer.BindToBindingInfo(GetNavigationService());
            Context.IocContainer.BindToBindingInfo(GetApplicationStateManager());
            return base.LoadInternal();
        }

        /// <summary>
        ///     Gets the <see cref="ISerializer" /> that will be used in the current application by default.
        /// </summary>
        /// <returns>An instance of <see cref="ISerializer" />.</returns>
        protected override BindingInfo<ISerializer> GetSerializer()
        {
            var assemblies = Context.Assemblies;
            return BindingInfo<ISerializer>.FromMethod((container, list) => new Serializer(assemblies), DependencyLifecycle.SingleInstance);
        }

        /// <summary>
        ///     Gets the <see cref="IReflectionManager" /> that will be used by default.
        /// </summary>
        /// <returns>An instance of <see cref="IReflectionManager" />.</returns>
        protected override BindingInfo<IReflectionManager> GetReflectionManager()
        {
            return BindingInfo<IReflectionManager>.FromType<ExpressionReflectionManagerEx>(DependencyLifecycle.SingleInstance);
        }

        /// <summary>
        ///     Gets the <see cref="IViewModelPresenter" /> that will be used in the current application by default.
        /// </summary>
        /// <returns>An instance of <see cref="IViewModelPresenter" />.</returns>
        protected override BindingInfo<IViewModelPresenter> GetViewModelPresenter()
        {
            return BindingInfo<IViewModelPresenter>.FromMethod((container, list) =>
            {
                var presenter = new ViewModelPresenter();
                presenter.DynamicPresenters.Add(new DynamicViewModelNavigationPresenter());
#if !API8
                presenter.DynamicPresenters.Add(
                                    new DynamicViewModelWindowPresenter(container.Get<IViewMappingProvider>(),
                                        container.Get<IViewManager>(), container.Get<IThreadManager>(),
                                        container.Get<IOperationCallbackManager>()));
#endif
                return presenter;
            }, DependencyLifecycle.SingleInstance);
        }

        /// <summary>
        ///     Gets the <see cref="IMessagePresenter" /> that will be used in the current application by default.
        /// </summary>
        /// <returns>An instance of <see cref="IMessagePresenter" />.</returns>
        protected override BindingInfo<IMessagePresenter> GetMessagePresenter()
        {
            return BindingInfo<IMessagePresenter>.FromType<MessagePresenter>(DependencyLifecycle.SingleInstance);
        }

        /// <summary>
        ///     Gets the <see cref="IViewManager" /> that will be used in the current application by default.
        /// </summary>
        /// <returns>An instance of <see cref="IViewManager" />.</returns>
        protected override BindingInfo<IViewManager> GetViewManager()
        {
            return BindingInfo<IViewManager>.FromType<ViewManagerEx>(DependencyLifecycle.SingleInstance);
        }

        /// <summary>
        ///     Gets the <see cref="ITracer" /> that will be used by default.
        /// </summary>
        /// <returns>An instance of <see cref="ITracer" />.</returns>
        protected override BindingInfo<ITracer> GetTracer()
        {
            return BindingInfo<ITracer>.FromType<DebugTracer>(DependencyLifecycle.SingleInstance);
        }

        /// <summary>
        ///     Gets the <see cref="IThreadManager" /> that will be used in the current application by default.
        /// </summary>
        /// <returns>An instance of <see cref="IThreadManager" />.</returns>
        protected override BindingInfo<IThreadManager> GetThreadManager()
        {
            return BindingInfo<IThreadManager>.FromMethod((container, list) => new ThreadManager(Application.SynchronizationContext), DependencyLifecycle.SingleInstance);
        }

        /// <summary>
        ///     Gets the <see cref="INavigationProvider" /> that will be used in the current application by default.
        /// </summary>
        /// <returns>An instance of <see cref="INavigationProvider" />.</returns>
        protected override BindingInfo<INavigationProvider> GetNavigationProvider()
        {
            return BindingInfo<INavigationProvider>.FromType<NavigationProvider>(DependencyLifecycle.SingleInstance);
        }

        /// <summary>
        ///     Gets the <see cref="INavigationCachePolicy" /> that will be used in the current application by default.
        /// </summary>
        /// <returns>An instance of <see cref="INavigationCachePolicy" />.</returns>
        protected override BindingInfo<INavigationCachePolicy> GetNavigationCachePolicy()
        {
            return BindingInfo<INavigationCachePolicy>.FromType<EmptyNavigationCachePolicy>(DependencyLifecycle.SingleInstance);
        }

        /// <summary>
        ///     Gets the <see cref="IToastPresenter" /> that will be used in the current application by default.
        /// </summary>
        /// <returns>An instance of <see cref="IToastPresenter" />.</returns>
        protected override BindingInfo<IToastPresenter> GetToastPresenter()
        {
            return BindingInfo<IToastPresenter>.FromType<ToastPresenter>(DependencyLifecycle.SingleInstance);
        }

        /// <summary>
        ///     Gets the <see cref="IOperationCallbackFactory" /> that will be used in the current application by default.
        /// </summary>
        /// <returns>An instance of <see cref="IOperationCallbackFactory" />.</returns>
        protected override BindingInfo<IOperationCallbackFactory> GetOperationCallbackFactory()
        {
            return BindingInfo<IOperationCallbackFactory>.FromType<SerializableOperationCallbackFactory>(DependencyLifecycle.SingleInstance);
        }

        /// <summary>
        ///     Gets the <see cref="IAttachedValueProvider" /> that will be used by default.
        /// </summary>
        /// <returns>An instance of <see cref="IAttachedValueProvider" />.</returns>
        protected override BindingInfo<IAttachedValueProvider> GetAttachedValueProvider()
        {
            return BindingInfo<IAttachedValueProvider>.FromType<AttachedValueProvider>(DependencyLifecycle.SingleInstance);
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Gets the <see cref="IApplicationStateManager" /> that will be used in all view models by default.
        /// </summary>
        /// <returns>An instance of <see cref="IApplicationStateManager" />.</returns>
        protected virtual BindingInfo<IApplicationStateManager> GetApplicationStateManager()
        {
            return BindingInfo<IApplicationStateManager>.FromType<ApplicationStateManager>(DependencyLifecycle.SingleInstance);
        }

        /// <summary>
        ///     Gets the <see cref="INavigationService" /> that will be used in all view models by default.
        /// </summary>
        /// <returns>An instance of <see cref="INavigationService" />.</returns>
        protected virtual BindingInfo<INavigationService> GetNavigationService()
        {
            return BindingInfo<INavigationService>.FromType<NavigationService>(DependencyLifecycle.SingleInstance);
        }

        /// <summary>
        ///     Gets the <see cref="IViewFactory" /> that will be used in all view models by default.
        /// </summary>
        /// <returns>An instance of <see cref="IViewManager" />.</returns>
        protected virtual BindingInfo<IViewFactory> GetViewFactory()
        {
            return BindingInfo<IViewFactory>.FromType<ViewFactory>(DependencyLifecycle.SingleInstance);
        }

        #endregion
    }
}