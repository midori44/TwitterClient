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
	/// EditWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class EditWindow : Window
	{
		public string Text { get; private set; }
		public bool Successed { get; private set; }

		public EditWindow(string name = "")
		{
			InitializeComponent();

			nameTextBox.Text = name;
			Successed = false;

			nameTextBox.Focus();
		}

		private void okButton_Click(object sender, RoutedEventArgs e)
		{
			if(!String.IsNullOrWhiteSpace(nameTextBox.Text))
			{
				Text = nameTextBox.Text;
				Successed = true;
				this.Close();
			}
		}

		private void cancelButton_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}


	}
}
