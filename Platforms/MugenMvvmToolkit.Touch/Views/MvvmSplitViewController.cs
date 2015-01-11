#region Copyright

// ****************************************************************************
// <copyright file="MvvmSplitViewController.cs">
// Copyright (c) 2012-2015 Vyacheslav Volkov
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
using System.Threading;
using Foundation;
using MugenMvvmToolkit.Interfaces.Mediators;
using MugenMvvmToolkit.Interfaces.Views;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;
using UIKit;

namespace MugenMvvmToolkit.Views
{
    public abstract class MvvmSplitViewController : UISplitViewController, IMvvmViewController
    {
        #region Fields

        private IMvvmViewControllerMediator _mediator;

        #endregion

        #region Constructors

        protected MvvmSplitViewController()
        {            
        }

        protected MvvmSplitViewController(NSCoder coder)
            : base(coder)
        {            
        }

        protected MvvmSplitViewController(NSObjectFlag t)
            : base(t)
        {            
        }

        protected MvvmSplitViewController(IntPtr handle)
            : base(handle)
        {            
        }

        protected MvvmSplitViewController(string nibName, NSBundle bundle)
            : base(nibName, bundle)
        {            
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the current <see cref="IMvvmViewControllerMediator" />.
        /// </summary>
        protected IMvvmViewControllerMediator Mediator
        {
            get
            {
                if (_mediator == null)
                    Interlocked.CompareExchange(ref _mediator, PlatformExtensions.MvvmViewControllerMediatorFactory(this, DataContext.Empty), null);
                return _mediator;
            }
        }

        #endregion

        #region Implementation of IMvvmViewController

        public event EventHandler ViewDidLoadHandler
        {
            add { Mediator.ViewDidLoadHandler += value; }
            remove { Mediator.ViewDidLoadHandler -= value; }
        }

        public event EventHandler<ValueEventArgs<bool>> ViewWillAppearHandler
        {
            add { Mediator.ViewWillAppearHandler += value; }
            remove { Mediator.ViewWillAppearHandler -= value; }
        }

        public event EventHandler<ValueEventArgs<bool>> ViewDidAppearHandler
        {
            add { Mediator.ViewDidAppearHandler += value; }
            remove { Mediator.ViewDidAppearHandler -= value; }
        }

        public event EventHandler<ValueEventArgs<bool>> ViewDidDisappearHandler
        {
            add { Mediator.ViewDidDisappearHandler += value; }
            remove { Mediator.ViewDidDisappearHandler -= value; }
        }

        public event EventHandler<ValueEventArgs<bool>> ViewWillDisappearHandler
        {
            add { Mediator.ViewWillDisappearHandler += value; }
            remove { Mediator.ViewWillDisappearHandler -= value; }
        }

        public event EventHandler<ValueEventArgs<NSCoder>> DecodeRestorableStateHandler
        {
            add { Mediator.DecodeRestorableStateHandler += value; }
            remove { Mediator.DecodeRestorableStateHandler -= value; }
        }

        public event EventHandler<ValueEventArgs<NSCoder>> EncodeRestorableStateHandler
        {
            add { Mediator.EncodeRestorableStateHandler += value; }
            remove { Mediator.EncodeRestorableStateHandler -= value; }
        }

        public event EventHandler DisposeHandler
        {
            add { Mediator.DisposeHandler += value; }
            remove { Mediator.DisposeHandler -= value; }
        }

        #endregion

        #region Overrides of UIViewController

        public override void DecodeRestorableState(NSCoder coder)
        {
            Mediator.DecodeRestorableState(base.DecodeRestorableState, coder);
        }

        public override void EncodeRestorableState(NSCoder coder)
        {
            Mediator.EncodeRestorableState(base.EncodeRestorableState, coder);
        }

        public override void ViewDidAppear(bool animated)
        {
            Mediator.ViewDidAppear(base.ViewDidAppear, animated);
        }

        public override void ViewDidDisappear(bool animated)
        {
            Mediator.ViewDidDisappear(base.ViewDidDisappear, animated);
        }

        public override void ViewDidLoad()
        {
            Mediator.ViewDidLoad(base.ViewDidLoad);
        }

        public override void ViewWillAppear(bool animated)
        {
            Mediator.ViewWillAppear(base.ViewWillAppear, animated);
        }

        public override void ViewWillDisappear(bool animated)
        {
            Mediator.ViewWillDisappear(base.ViewWillDisappear, animated);
        }

        protected override void Dispose(bool disposing)
        {
            Mediator.Dispose(base.Dispose, disposing);
        }

        #endregion
    }
}