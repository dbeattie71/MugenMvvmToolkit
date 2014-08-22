#region Copyright
// ****************************************************************************
// <copyright file="AttachedMembersModule.cs">
// Copyright � Vyacheslav Volkov 2012-2014
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
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Binding.Models.EventArg;
using MugenMvvmToolkit.Converters;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;

namespace MugenMvvmToolkit.Infrastructure
{
    public class AttachedMembersModule : IModule
    {
        #region Fields

        internal readonly static IAttachedBindingMemberInfo<object, ICollectionViewManager> CollectionViewManagerMember;
        internal readonly static IAttachedBindingMemberInfo<Control, bool> DisableValidationMember;

        private readonly static IAttachedBindingMemberInfo<object, IContentViewManager> ContentViewManagerMember;
        private readonly static IAttachedBindingMemberInfo<object, IEnumerable> ItemsSourceMember;
        private readonly static IAttachedBindingMemberInfo<Control, object> ContenMember;
        private readonly static IAttachedBindingMemberInfo<Control, IDataTemplateSelector> ContenTemplateMember;

        #endregion

        #region Constructors

        static AttachedMembersModule()
        {
            //Object
            ItemsSourceMember = AttachedBindingMember.CreateAutoProperty<object, IEnumerable>(AttachedMemberConstants.ItemsSource, ObjectItemsSourceChanged);
            CollectionViewManagerMember = AttachedBindingMember.CreateAutoProperty<object, ICollectionViewManager>("CollectionViewManager");
            ContentViewManagerMember = AttachedBindingMember.CreateAutoProperty<object, IContentViewManager>("ContentViewManager");

            //Control
            DisableValidationMember = AttachedBindingMember.CreateAutoProperty<Control, bool>("DisableValidation");
            ContenMember = AttachedBindingMember.CreateAutoProperty<Control, object>(AttachedMemberConstants.Content, ContentChanged);
            ContenTemplateMember = AttachedBindingMember.CreateAutoProperty<Control, IDataTemplateSelector>(AttachedMemberConstants.ContentTemplate, ContentTemplateChanged);
        }

        #endregion

        #region Methods

        private static void Register([NotNull] IBindingMemberProvider memberProvider)
        {
            Should.NotBeNull(memberProvider, "memberProvider");

            //Object
            memberProvider.Register(AttachedBindingMember.CreateMember<object, object>(AttachedMemberConstants.ItemsSource,
                    GetObjectItemsSource, SetObjectItemsSource, ObserveObjectItemsSource));

            memberProvider.Register(CollectionViewManagerMember);
            memberProvider.Register(ContentViewManagerMember);
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty<object, IDataTemplateSelector>(AttachedMemberConstants.ItemTemplate));

            //Control
            memberProvider.Register(AttachedBindingMember
                .CreateMember<Control, object>(AttachedMemberConstants.FindByNameMethod, FindByNameControlMember, null));
            memberProvider.Register(AttachedBindingMember
                .CreateMember<Control, bool>(AttachedMemberConstants.Enabled,
                    (info, control, arg3) => control.Enabled, (info, control, arg3) => control.Enabled = (bool)arg3[0],
                    memberChangeEventName: "EnabledChanged"));
            memberProvider.Register(AttachedBindingMember
                .CreateMember<Control, bool>(AttachedMemberConstants.Focused,
                    (info, control, arg3) => control.Focused, null,
                    memberChangeEventName: "LostFocus"));

            memberProvider.Register(ContenMember);
            memberProvider.Register(ContenTemplateMember);
            memberProvider.Register(DisableValidationMember);

            //DateTimePicker
            memberProvider.Register(AttachedBindingMember.CreateMember<DateTimePicker, DateTime>("Value", (info, picker, arg3) => picker.Value,
                (info, picker, arg3) =>
                {
                    var value = (DateTime)arg3[0];
                    if (value < picker.MinDate)
                        picker.Value = picker.MinDate;
                    else if (value > picker.MaxDate)
                        picker.Value = picker.MaxDate;
                    else
                        picker.Value = value;
                    return null;
                }, memberChangeEventName: "ValueChanged"));

            //MenuItem
            memberProvider.Register(AttachedBindingMember
                .CreateMember<MenuItem, bool>(AttachedMemberConstants.Enabled,
                    (info, control, arg3) => control.Enabled, (info, control, arg3) => control.Enabled = (bool)arg3[0]));

            //ToolStripItem
            memberProvider.Register(AttachedBindingMember.CreateMember<ToolStripItem, object>(AttachedMemberConstants.Parent,
                    GetParentToolStripItem, null, ObserveParentMemberToolStripItem));
            memberProvider.Register(AttachedBindingMember.CreateMember<ToolStripItem, object>(AttachedMemberConstants.FindByNameMethod,
                    FindByNameMemberToolStripItem, null));
            memberProvider.Register(AttachedBindingMember
                .CreateMember<ToolStripItem, bool>(AttachedMemberConstants.Enabled,
                    (info, control, arg3) => control.Enabled, (info, control, arg3) => control.Enabled = (bool)arg3[0],
                    memberChangeEventName: "EnabledChanged"));

            //TabControl
            memberProvider.Register(AttachedBindingMember.CreateMember<TabControl, object>(AttachedMemberConstants.SelectedItem,
                GetSelectedItemTabControl, SetSelectedItemTabControl, memberChangeEventName: "Selected"));

            //ComboBox
            memberProvider.Register(AttachedBindingMember.CreateMember<ComboBox, object>(AttachedMemberConstants.ItemsSource, (info, box, arg3) => box.DataSource,
                (info, box, arg3) => box.DataSource = arg3[0], memberChangeEventName: "DataSourceChanged"));
            memberProvider.Register(
                AttachedBindingMember.CreateMember<ComboBox, object>(AttachedMemberConstants.SelectedItem,
                    (info, box, arg3) => box.SelectedItem, (info, box, arg3) => box.SelectedItem = arg3[0],
                    memberChangeEventName: "SelectedIndexChanged"));

            //DataGridView
            memberProvider.Register(AttachedBindingMember.CreateMember<DataGridView, object>(AttachedMemberConstants.ItemsSource,
                (info, view, arg3) => view.DataSource, (info, view, arg3) =>
                {
                    view.DataSource = arg3[0];
                    view.Refresh();
                    return null;
                }, memberChangeEventName: "DataSourceChanged"));
            memberProvider.Register(AttachedBindingMember.CreateMember<DataGridView, object>(AttachedMemberConstants.SelectedItem,
                GetSelectedItemDataGridView, SetSelectedItemDataGridView, memberChangeEventName: "CurrentCellChanged"));
        }

        #region Control

        private static void ContentTemplateChanged(Control control, AttachedMemberChangedEventArgs<IDataTemplateSelector> args)
        {
            UpdateContent(control, ContenMember.GetValue(control, null), args.NewValue);
        }

        private static void ContentChanged(Control control, AttachedMemberChangedEventArgs<object> args)
        {
            UpdateContent(control, args.NewValue, ContenTemplateMember.GetValue(control, null));
        }

        private static void UpdateContent(Control container, object value, IDataTemplateSelector selector)
        {
            if (selector != null)
                value = selector.SelectTemplate(value, container);
            var content = value as Control;
            if (content == null)
            {
                var viewModel = value as IViewModel;
                if (viewModel != null)
                    content = ViewModelToViewConverter.Instance.Convert(viewModel, null, null, null) as Control;
            }
            if (content == null && value != null)
            {
                Tracer.Warn("The content value {0} is not a Control.", value);
                content = new TextBox
                {
                    ReadOnly = true,
                    ForeColor = Color.Red,
                    Text = value.ToString(),
                    Multiline = true
                };

            }
            IContentViewManager viewManager = ContentViewManagerMember.GetValue(container, null);
            if (viewManager == null)
            {
                container.Controls.Clear();

                if (content != null)
                {
                    content.Dock = DockStyle.Fill;
                    content.AutoSize = true;
                    container.Size = content.Size;
                    container.Controls.Clear();
                    container.Controls.Add(content);
                }
            }
            else
                viewManager.SetContent(container, content);
        }

        private static object FindByNameControlMember(IBindingMemberInfo bindingMemberInfo, Control control, IList<object> arg3)
        {
            var root = PlatformExtensions.GetRootControl(control);
            if (root != null)
                control = root;
            return control.Controls.Find((string)arg3[0], true).FirstOrDefault();
        }

        #endregion

        #region Object

        private static IDisposable ObserveObjectItemsSource(IBindingMemberInfo bindingMemberInfo, object component, IEventListener arg3)
        {
            return GetObjectItemsSourceMember(component).TryObserve(component, arg3);
        }

        private static object SetObjectItemsSource(IBindingMemberInfo bindingMemberInfo, object component, object[] arg3)
        {
            return GetObjectItemsSourceMember(component).SetValue(component, arg3);
        }

        private static object GetObjectItemsSource(IBindingMemberInfo bindingMemberInfo, object component, object[] arg3)
        {
            return GetObjectItemsSourceMember(component).GetValue(component, arg3);
        }

        private static void ObjectItemsSourceChanged(object control, AttachedMemberChangedEventArgs<IEnumerable> args)
        {
            ServiceProvider.AttachedValueProvider
                .GetOrAdd(control, "@!generator", (o, o1) => new ItemsSourceGenerator(o), null)
                .Update(args.NewValue);
        }

        private static IBindingMemberInfo GetObjectItemsSourceMember(object component)
        {
            return BindingServiceProvider.MemberProvider.GetBindingMember(component.GetType(),
                AttachedMemberConstants.ItemsSource, true, false) ?? ItemsSourceMember;
        }

        #endregion

        #region ToolStripItem

        private static object FindByNameMemberToolStripItem(IBindingMemberInfo bindingMemberInfo, ToolStripItem target,
            IList<object> arg3)
        {
            Control control = GetOwner(target);
            if (control == null)
                return null;
            return FindByNameControlMember(null, control, arg3);
        }

        private static object GetParentToolStripItem(IBindingMemberInfo bindingMemberInfo, ToolStripItem target, IList<object> arg3)
        {
            return GetOwner(target);
        }

        private static IDisposable ObserveParentMemberToolStripItem(IBindingMemberInfo bindingMemberInfo, ToolStripItem toolStripItem, IEventListener arg3)
        {
            EventHandler handler = arg3.ToWeakEventListener().Handle;
            ToolStrip owner = GetOwner(toolStripItem);
            WeakReference ownerRef = null;
            if (owner != null)
            {
                owner.ParentChanged += handler;
                ownerRef = ServiceProvider.WeakReferenceFactory(owner, true);
            }
            toolStripItem.OwnerChanged += handler;
            var menuItemRef = ServiceProvider.WeakReferenceFactory(toolStripItem, true);
            return new ActionToken(() =>
            {
                if (ownerRef != null)
                {
                    var toolStrip = ownerRef.Target as ToolStrip;
                    if (toolStrip != null)
                        toolStrip.ParentChanged -= handler;
                    ownerRef = null;
                }
                var item = menuItemRef.Target as ToolStripItem;
                if (item != null)
                    item.OwnerChanged -= handler;
                menuItemRef = null;
            });
        }

        private static ToolStrip GetOwner(ToolStripItem menuItem)
        {
            ToolStrip owner = menuItem.Owner;
            while (owner is ToolStripDropDownMenu)
                owner = (owner as ToolStripDropDownMenu).OwnerItem.Owner;
            return owner;
        }

        #endregion

        #region DataGridView

        private static object SetSelectedItemDataGridView(IBindingMemberInfo bindingMemberInfo, DataGridView dataGridView, object[] arg3)
        {
            dataGridView.ClearSelection();
            var item = arg3[0];
            if (item == null)
                return null;
            for (int i = 0; i < dataGridView.Rows.Count; i++)
            {
                if (Equals(dataGridView.Rows[i].DataBoundItem, item))
                {
                    var row = dataGridView.Rows[i];
                    row.Selected = true;
                    if (row.Cells.Count > 0)
                        row.Cells[0].Selected = true;
                    return null;
                }
            }
            return null;
        }

        private static object GetSelectedItemDataGridView(IBindingMemberInfo bindingMemberInfo, DataGridView dataGridView, object[] arg3)
        {
            var row = dataGridView.CurrentRow;
            if (row == null)
                return null;
            return row.DataBoundItem;
        }

        #endregion

        #region TabControl

        private static object GetSelectedItemTabControl(IBindingMemberInfo bindingMemberInfo, TabControl tabControl, object[] arg3)
        {
            if (tabControl.TabCount == 0 || tabControl.SelectedIndex < 0)
                return null;
            return BindingServiceProvider
                .ContextManager
                .GetBindingContext(tabControl.TabPages[tabControl.SelectedIndex]).Value;
        }

        private static object SetSelectedItemTabControl(IBindingMemberInfo bindingMemberInfo, TabControl tabControl, object[] arg3)
        {
            var item = arg3[0];
            foreach (TabPage tabPage in tabControl.TabPages)
            {
                if (Equals(BindingServiceProvider.ContextManager.GetBindingContext(tabPage).Value, item))
                {
                    tabControl.SelectedTab = tabPage;
                    return null;
                }
            }
            return null;
        }

        #endregion

        #endregion

        #region Implementation of IModule

        /// <summary>
        ///     Gets the priority.
        /// </summary>
        public int Priority
        {
            get { return ModuleBase.BindingModulePriority; }
        }

        /// <summary>
        ///     Loads the current module.
        /// </summary>
        public bool Load(IModuleContext context)
        {
            Register(BindingServiceProvider.MemberProvider);
            BindingServiceProvider.ErrorProvider = new BindingErrorProvider();
            return true;
        }

        /// <summary>
        ///     Unloads the current module.
        /// </summary>
        public void Unload(IModuleContext context)
        {
        }

        #endregion
    }
}