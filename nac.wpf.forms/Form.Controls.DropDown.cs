using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace nac.wpf.forms
{
    public partial class Form
    {
        
        private string DropDownItemSourceName(string fieldName)
        {
            return $"{fieldName}_dropDownItemSource";
        }

        public Form DropDownFor(string fieldName, IEnumerable<string> dropDownSource)
        {
            this.Model[fieldName] = "";
            string itemSourceFieldName = DropDownItemSourceName(fieldName);
            this.Model[itemSourceFieldName] = dropDownSource;

            ComboBox cb = new ComboBox();

            // item source binding
            Helper_BindField(itemSourceFieldName, cb, ComboBox.ItemsSourceProperty);

            // selected item binding
            Helper_BindField(fieldName, cb, ComboBox.SelectedItemProperty, BindingMode.TwoWay);

            this.Helper_AddRowToHost(cb, fieldName);

            return this;
        }


        public Form DropDownFor(string fieldName, Type enumType)
        {
            // enumType from: http://stackoverflow.com/questions/79126/create-generic-method-constraining-t-to-an-enum
            if (!enumType.IsEnum)
            {
                throw new ArgumentException($"Type {enumType.Name} is not an enum type.", nameof(enumType));
            }

            string itemSourceName = DropDownItemSourceName(fieldName);
            this.Model[itemSourceName] = Enum.GetValues(enumType);

            ComboBox cb = new ComboBox();

            // item source binding
            Helper_BindField(itemSourceName, cb, ComboBox.ItemsSourceProperty);

            // selected item binding
            Helper_BindField(fieldName, cb, ComboBox.SelectedItemProperty, BindingMode.TwoWay);

            this.Helper_AddRowToHost(cb, fieldName);

            return this;
        }
        
        
        
    }
}