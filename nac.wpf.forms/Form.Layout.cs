﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;

namespace nac.wpf.forms;

public partial class Form
{
    public Form HorizontalGroup(Action<Form> populateHorizontalGroup,
        string isVisiblePropertyName = null)
    {
        var horizontalGroupForm = new Form(_parentForm: this);

        populateHorizontalGroup(horizontalGroupForm);

        // take all the child items of host and put them in a grid with equal space between?
        Grid horiontalGroup = new Grid();
        var gridRow = new RowDefinition();
        horiontalGroup.RowDefinitions.Add(gridRow);
        int rowIndex = 0;
        int columnIndex = 0;
        foreach (UIElement child in horizontalGroupForm.Host.Children
                                        .OfType<UIElement>().ToList()
                                        )
        {
            // called ToList above so we can remove this guy from the host children without breaking the foreach
            horizontalGroupForm.Host.Children.Remove(child); // we have to remove it from the host to be able to add it to the Grid
            var col = new ColumnDefinition();
            horiontalGroup.ColumnDefinitions.Add(col);

            Grid.SetRow(child, rowIndex);
            Grid.SetColumn(child, columnIndex);
            horiontalGroup.Children.Add(child);

            ++columnIndex; // last statement
        }

        if (!string.IsNullOrWhiteSpace(isVisiblePropertyName))
        {
            addVisibilityTrigger(horiontalGroup, isVisiblePropertyName);
        }

        Helper_AddRowToHost(horiontalGroup);
        return this;
    }


    /*
     For grid info see these two links
     https://www.c-sharpcorner.com/Resources/676/how-to-create-a-grid-in-wpf-dynamically.aspx
     https://www.wpf-tutorial.com/panels/gridsplitter/
    */
    public Form VerticalGroupSplit(Action<Form> populateVerticalGroup)
    {
        var vertGroupForm = new Form(_parentForm: this);

        populateVerticalGroup(vertGroupForm);

        Grid vertGroup = new Grid();
        var gridCol = new ColumnDefinition();
        vertGroup.ColumnDefinitions.Add(gridCol);
        int rowIndex = 0;
        int columnIndex = 0;
        var childControls = vertGroupForm.Host.Children.OfType<UIElement>().ToList();
        foreach (var child in childControls)
        {
            vertGroupForm.Host.Children.Remove(child); // get the child out of the form so we can move it to the grid

            // 1 row for the child control
            var row = new RowDefinition();
            vertGroup.RowDefinitions.Add(row);

            // set child into the grid
            Grid.SetRow(child, rowIndex);
            Grid.SetColumn(child, columnIndex);
            vertGroup.Children.Add(child);

            // 1 row for grid splitter (If not last child)
            if (child != childControls.Last())
            {
                ++rowIndex; // we are on the next row because we are going to add a row definition
                int splitterHeight = 5;
                var gridSplitRow = new RowDefinition();
                gridSplitRow.Height = new GridLength(splitterHeight);
                vertGroup.RowDefinitions.Add(gridSplitRow);

                var gridSplitter = new GridSplitter();
                gridSplitter.HorizontalAlignment = HorizontalAlignment.Stretch;
                gridSplitter.Height = splitterHeight;
                Grid.SetRow(gridSplitter, rowIndex);
                Grid.SetColumn(gridSplitter, columnIndex);
                vertGroup.Children.Add(gridSplitter);
            }

            ++rowIndex; // make sure this is last statement
        }

        Helper_AddRowToHost(vertGroup, rowAutoHeight: false);
        return this;
    }


    public Form VerticalGroup(Action<Form> populateVerticalGroup,
        string isVisiblePropertyName = null)
    {
        var vertGroupForm = new Form(_parentForm: this);

        populateVerticalGroup(vertGroupForm);

        var vertGroup = new DockPanel();

        if (!string.IsNullOrWhiteSpace(isVisiblePropertyName))
        {
            addVisibilityTrigger(vertGroup, isVisiblePropertyName);
        }

        var childControls = vertGroupForm.Host.Children.OfType<UIElement>().ToList();
        foreach (var child in childControls)
        {
            vertGroupForm.Host.Children.Remove(child); // get the child out of the form so we can move it to the grid
            DockPanel.SetDock(child, Dock.Top);
            vertGroup.Children.Add(child);
        }

        Helper_AddRowToHost(vertGroup, rowAutoHeight: false);
        return this;
    }



    public Form HorizontalGroupSplit(Action<Form> populateHorizontalGroup)
    {
        var horizontalGroupForm = new Form(_parentForm: this);

        populateHorizontalGroup(horizontalGroupForm);

        // take all the child items of host and put them in a grid with equal space between?
        Grid horiontalGroup = new Grid();
        var gridRow = new RowDefinition();
        horiontalGroup.RowDefinitions.Add(gridRow);
        int rowIndex = 0;
        int columnIndex = 0;
        var childControls = horizontalGroupForm.Host.Children
                                        .OfType<UIElement>().ToList();
        foreach (var child in childControls)
        {
            // called ToList above so we can remove this guy from the host children without breaking the foreach
            horizontalGroupForm.Host.Children.Remove(child); // we have to remove it from the host to be able to add it to the Grid
            var col = new ColumnDefinition();
            horiontalGroup.ColumnDefinitions.Add(col);

            Grid.SetRow(child, rowIndex);
            Grid.SetColumn(child, columnIndex);
            horiontalGroup.Children.Add(child);

            // 1 column for grid splitter (If not last child)
            if (child != childControls.Last())
            {
                ++columnIndex; // incriment because we are going to add a new column below
                int splitterWidth = 5;
                var gridSplitColumn = new ColumnDefinition();
                gridSplitColumn.Width = new GridLength(splitterWidth);
                horiontalGroup.ColumnDefinitions.Add(gridSplitColumn);

                var gridSplitter = new GridSplitter();
                gridSplitter.HorizontalAlignment = HorizontalAlignment.Stretch;
                gridSplitter.Width = splitterWidth;
                Grid.SetRow(gridSplitter, rowIndex);
                Grid.SetColumn(gridSplitter, columnIndex);
                horiontalGroup.Children.Add(gridSplitter);
            }

            ++columnIndex; // last statement
        }

        Helper_AddRowToHost(horiontalGroup);
        return this;
    } 
}
