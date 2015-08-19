﻿#region Copyright

// ****************************************************************************
// <copyright file="ValueConverterWrapper.cs">
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
using System.Globalization;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;
#if XAMARIN_FORMS
using Xamarin.Forms;

namespace MugenMvvmToolkit.Xamarin.Forms.Binding.Converters
#elif WPF
using System.Windows.Data;

namespace MugenMvvmToolkit.WPF.Binding.Converters
#elif SILVERLIGHT
using System.Windows.Data;

namespace MugenMvvmToolkit.Silverlight.Binding.Converters
#elif WINDOWS_PHONE
using System.Windows.Data;

namespace MugenMvvmToolkit.WinPhone.Binding.Converters
#endif
{
    /// <summary>
    ///     Represents the native converter wrapper
    /// </summary>
    public sealed class ValueConverterWrapper : IBindingValueConverter, IValueConverter
    {
        #region Fields

        private readonly IValueConverter _valueConverter;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ValueConverterWrapper" /> class.
        /// </summary>
        public ValueConverterWrapper([NotNull] IValueConverter valueConverter)
        {
            Should.NotBeNull(valueConverter, "valueConverter");
            _valueConverter = valueConverter;
        }

        #endregion

        #region Implementation of IValueConverterCore

        /// <summary>
        ///     Converts a value.
        /// </summary>
        /// <returns>
        ///     A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <param name="context">The current context to use in the converter.</param>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture, IDataContext context)
        {
            return _valueConverter.Convert(value, targetType, parameter, culture);
        }

        /// <summary>
        ///     Converts a value.
        /// </summary>
        /// <returns>
        ///     A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        /// <param name="value">The value that is produced by the binding target.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <param name="context">The current context to use in the converter.</param>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture, IDataContext context)
        {
            return _valueConverter.ConvertBack(value, targetType, parameter, culture);
        }

        #endregion

        #region Implementation of IValueConverter

        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Convert(value, targetType, parameter, culture, DataContext.Empty);
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ConvertBack(value, targetType, parameter, culture, DataContext.Empty);
        }

        #endregion
    }
}