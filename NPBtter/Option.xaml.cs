using CoreTweet;
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
	/// Option.xaml の相互作用ロジック
	/// </summary>
	public partial class Option : Window
	{
		public Tokens tokens { get; private set; }
		private OAuth.OAuthSession session;

		public Option()
		{
			InitializeComponent();
		}

		private void startSettingButton_Click(object sender, RoutedEventArgs e)
		{
			session = OAuth.Authorize(Properties.Resources.APIKEY, Properties.Resources.APISECRET);
			System.Diagnostics.Process.Start(session.AuthorizeUri.ToString());
		}

		private void pinButton_Click(object sender, RoutedEventArgs e)
		{
			if(String.IsNullOrEmpty(pinTextbox.Text)) { return; }

			tokens = session.GetTokens(pinTextbox.Text);
			if(tokens != null)
			{
				MessageBox.Show("complete!");
			}
		}

		private void closeButton_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}
	}
}
