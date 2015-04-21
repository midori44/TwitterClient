using CoreTweet;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NPBtter
{
	/// <summary>
	/// MainWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class MainWindow : Window
	{
		private Tokens tokens;
		private OAuth.OAuthSession session;
		private const string APIKEY = "";
		private const string APISECRET = "";

		public ObservableCollection<Tw> Tws { get; set; }
		public double widthValue { get; set; }

		public MainWindow()
		{
			InitializeComponent();

			Tws = new ObservableCollection<Tw>();
			//this.mainGrid.ItemsSource = Tws;
			this.mainList.ItemsSource = Tws;
		}

		private void startSettingButton_Click(object sender, RoutedEventArgs e)
		{
			session = OAuth.Authorize(APIKEY, APISECRET);
			this.pinUritextbox.Text = session.AuthorizeUri.ToString();
		}

		private void pinButton_Click(object sender, RoutedEventArgs e)
		{
			tokens = session.GetTokens(pinTextbox.Text);
			this.pinResultTextbox.Text = tokens.ToString();
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			var tw = new Tw() {
				Name = "name",
				Text = new String('A', DateTime.Now.Millisecond % 140),
				widthValue = (this.Width > 280) ? this.Width - 80 : 200
			};
			this.Tws.Insert(0, tw);


			//DragList.Items.Insert(0, DragList.Items.Count + 1);
		}

		private void mainList_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			//ScrollViewer itemsViewer = (ScrollViewer)FindControl(mainList, typeof(ScrollViewer));
			//WrapPanel itemsPanel = (WrapPanel)FindControl(mainList, typeof(WrapPanel));
			//if(itemsPanel != null)
			//{
			//	itemsPanel.Width = itemsViewer.ActualWidth;
			//}
		}
		private DependencyObject FindControl(DependencyObject obj, Type controlType)
		{
			if(obj == null)
				return null;
			if(obj.GetType() == controlType)
				return obj;

			int childrenCount = VisualTreeHelper.GetChildrenCount(obj);
			for(int i = 0; i < childrenCount; i++)
			{
				DependencyObject child = VisualTreeHelper.GetChild(obj, i);
				DependencyObject descendant = FindControl(child, controlType);
				if(descendant != null && descendant.GetType() == controlType)
				{
					return descendant;
				}
			}

			return null;
		}

	}



	public class Tw
	{
		public string Name { get; set; }
		public string Text { get; set; }
		public double widthValue { get; set; }
		public Tw()
		{
		}
	}
}
