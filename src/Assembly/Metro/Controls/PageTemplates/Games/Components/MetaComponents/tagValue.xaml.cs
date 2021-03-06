﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Assembly.Helpers.Tags;

namespace Assembly.Metro.Controls.PageTemplates.Games.Components.MetaComponents
{
	/// <summary>
	///     Interaction logic for tagValue.xaml
	/// </summary>
	public partial class tagValue : UserControl
	{
		public static RoutedCommand JumpToCommand = new RoutedCommand();

		public tagValue()
		{
			InitializeComponent();
		}

		private void cbTagClass_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			bool enable = (cbTagClass.SelectedIndex > 0);
			cbTagEntry.IsEnabled = enable;
			btnNullTag.IsEnabled = enable;
			btnJumpToTag.IsEnabled = enable;
		}

		private void cbTagEntry_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (cbTagEntry.SelectedIndex < 0 && cbTagClass.SelectedIndex > 0)
				cbTagEntry.SelectedIndex = 0;
		}

		private void CanExecuteJumpToCommand(object sender, CanExecuteRoutedEventArgs e)
		{
			// hack so that the button is enabled by default
			e.CanExecute = true;
		}

		private void btnNullTag_Click(object sender, RoutedEventArgs e)
		{
			cbTagClass.SelectedIndex = 0;
		}
	}

	/// <summary>
	///     Converts a tag class node to an index in the class list.
	/// </summary>
	[ValueConversion(typeof(TagHierarchyNode), typeof (int))]
	internal class TagClassConverter : DependencyObject, IValueConverter
	{
		public static DependencyProperty TagsSourceProperty = DependencyProperty.Register(
			"TagsSource",
			typeof (TagHierarchy),
			typeof (TagClassConverter)
			);

		public TagHierarchy TagsSource
		{
			get { return (TagHierarchy) GetValue(TagsSourceProperty); }
			set { SetValue(TagsSourceProperty, value); }
		}

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var sourceNode = (TagHierarchyNode)value;
			int index = TagsSource.Nodes.IndexOf(sourceNode);
			return index + 1;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			int adjustedIndex = (int) value - 1;
			if (adjustedIndex >= 0 && adjustedIndex < TagsSource.Nodes.Count)
				return TagsSource.Nodes[adjustedIndex];
			return null;
		}
	}

	[ValueConversion(typeof(TagHierarchyNode), typeof (ObservableCollection<TagHierarchyNode>))]
	internal class TagEntryListRetriever : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var sourceClass = value as TagHierarchyNode;
			if (sourceClass == null)
				return null;
			return sourceClass.Children;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}