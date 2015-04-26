using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NPBtter
{
	/// <summary>
	/// ListMenu.xaml の相互作用ロジック
	/// </summary>
	public partial class ListMenu : Window
	{
		private MainTweetList TweetList;

		public ListMenu(MainTweetList list)
		{
			InitializeComponent();

			this.TweetList = list;

			this.groupListBox.ItemsSource = TweetList.SubLists;
			this.groupListBox.DisplayMemberPath = "Title";
		}

		private void groupListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var list = this.groupListBox.SelectedItem as SubTweetList;
			//this.filterListBox.ItemsSource = list.NameFilter.OrderBy(x => x.Key);
			//this.filterListBox.DisplayMemberPath = "Key";
			this.filterListBox.ItemsSource = list.NameFilter;//.OrderBy(x => x);
			
		}

		private void exitButton_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}

		private void groupAddButton_Click(object sender, RoutedEventArgs e)
		{
			var editWindow = new EditWindow();
			editWindow.Owner = this;
			editWindow.ShowDialog();
			if(editWindow.Successed)
			{
				TweetList.SubLists.Add(new SubTweetList() { Title = editWindow.Text});
			}
		}

		private void groupEditButton_Click(object sender, RoutedEventArgs e)
		{
			if(this.groupListBox.SelectedIndex > -1)
			{
				var list = this.groupListBox.SelectedItem as SubTweetList;

				var editWindow = new EditWindow(list.Title);
				editWindow.Owner = this;
				editWindow.ShowDialog();
				if(editWindow.Successed)
				{
					list.Title = editWindow.Text;
				}
			}
		}

		private void groupUpButton_Click(object sender, RoutedEventArgs e)
		{
			int index = this.groupListBox.SelectedIndex;
			if(index > -1)
			{
				TweetList.SubLists.Move(index, index + 1);
			}
		}

		private void groupDoenButton_Click(object sender, RoutedEventArgs e)
		{
			int index = this.groupListBox.SelectedIndex;
			if(index > -1)
			{
				TweetList.SubLists.Move(index, index - 1);
			}
		}

		private void groupRemoveButton_Click(object sender, RoutedEventArgs e)
		{
			int index = this.groupListBox.SelectedIndex;
			if(index > -1)
			{
				var result = MessageBox.Show("削除しますか？", "確認", MessageBoxButton.OKCancel);
				if(result == MessageBoxResult.OK)
				{
					TweetList.SubLists.RemoveAt(index);
				}
			}
		}




		private void filterRemoveButton_Click(object sender, RoutedEventArgs e)
		{
			int index = this.filterListBox.SelectedIndex;
			if(index > -1)
			{
				var result = MessageBox.Show("削除しますか？", "確認", MessageBoxButton.OKCancel);
				if(result == MessageBoxResult.OK)
				{
					int groupIndex = this.groupListBox.SelectedIndex;
					//var pair = this.filterListBox.SelectedValue as KeyValuePair<string,bool>?;
					//this.TweetList.SubLists[groupIndex].NameFilterRemove(pair.Value.Key);
					string name = this.filterListBox.SelectedValue as string;
					this.TweetList.SubLists[groupIndex].NameFilterRemove(name);
				}
			}
		}


	}
}
