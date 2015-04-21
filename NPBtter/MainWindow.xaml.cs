using CoreTweet;
using CoreTweet.Streaming;
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

using System.Reactive.Linq;
using CoreTweet.Streaming.Reactive;
using System.Threading;

namespace NPBtter
{
	/// <summary>
	/// MainWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class MainWindow : Window
	{
		private Tokens tokens;
		private OAuth.OAuthSession session;
		private const string APIKEY = "xnjHW56HzizeSeFf7C40sq8Wk";
		private const string APISECRET = "SDjWmDItipDaveIVBhLrfzs5whGb1cGiEXZ2rD4QxAW9beGlie";

		public TweetSet Tws { get; set; }
		public double widthValue { get; set; }

		public MainWindow()
		{
			InitializeComponent();

			this.Tws = new TweetSet();
			this.mainList.ItemsSource = this.Tws.Items;

			// load twitter token
			if(!string.IsNullOrEmpty(Properties.Settings.Default.AccessToken)
				&& !string.IsNullOrEmpty(Properties.Settings.Default.AccessTokenSecret))
			{
				tokens = Tokens.Create(
					APIKEY,
					APISECRET,
					Properties.Settings.Default.AccessToken,
					Properties.Settings.Default.AccessTokenSecret);

				foreach(var status in tokens.Statuses.HomeTimeline(count => 10).Reverse())
				{
					Tws.Set(status);
				}
			}

		}

		private void startSettingButton_Click(object sender, RoutedEventArgs e)
		{
			session = OAuth.Authorize(APIKEY, APISECRET);
			this.pinUritextbox.Text = session.AuthorizeUri.ToString();
			System.Diagnostics.Process.Start(session.AuthorizeUri.ToString());
		}

		private void pinButton_Click(object sender, RoutedEventArgs e)
		{
			tokens = session.GetTokens(pinTextbox.Text);
			this.pinResultTextbox.Text = tokens.ToString();
		}

		private void showTimelineButton_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				var stream = tokens.Streaming.StartObservableStream(StreamingType.User).Publish();
				stream.OfType<StatusMessage>()
					.Select(m => m.Status)
					.ObserveOn(SynchronizationContext.Current)
					.Subscribe(status => Tws.Set(status));

				var disposable = stream.Connect();
			}
			catch(Exception ex)
			{
				Console.WriteLine(ex.Message);
			}

		}

		


		private void Button_Click(object sender, RoutedEventArgs e)
		{
			
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

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if(tokens != null)
			{
				Properties.Settings.Default.AccessToken = tokens.AccessToken;
				Properties.Settings.Default.AccessTokenSecret = tokens.AccessTokenSecret;
				Properties.Settings.Default.Save();
			}
		}

	}



	public class TweetSet
	{
		public ObservableCollection<Tweet> Items { get; private set; }
		public TweetSet()
		{
			this.Items = new ObservableCollection<Tweet>();
		}
		public void Set(Status status)
		{
			this.Items.Insert(0, new Tweet(status));
		}
	}

	public class Tweet
	{
		private Status _status;

		public string Name { get { return _status.User.Name; } }
		public string ScreenName { get { return _status.User.ScreenName; } }
		public string Text { get { return _status.Text; } }
		public DateTime LocalDateTime { get { return _status.CreatedAt.LocalDateTime; } }
		public Uri ProfileImageUrl { get { return _status.User.ProfileImageUrl; } }
		public User ReUser { get { return (_status.RetweetedStatus != null) ? _status.RetweetedStatus.User : null; } }
		public double width { get; set; }

		public Uri DisplayImageUrl
		{
			get
			{
				return (ReUser == null)
					? ProfileImageUrl
					: ReUser.ProfileImageUrl;
			}
		}
		public string DisplayText
		{
			get
			{
				return (ReUser == null)
					? Name + " / @" + ScreenName + "\n" + Text + "\n" + LocalDateTime
					: ReUser.Name + " / @" + ReUser.ScreenName + " (RT:@" + ScreenName + ")" + "\n"
						+ _status.RetweetedStatus.Text + "\n" + _status.RetweetedStatus.CreatedAt.LocalDateTime;
			}
		}

		public Tweet(Status status)
		{
			_status = status;

			//Name = status.User.Name;
			//ScreenName = status.User.ScreenName;
			//Text = status.Text;
			//LocalDateTime = status.CreatedAt.LocalDateTime;
			//ProfileImageUrl = status.User.ProfileImageUrl;
			//IsRetweeted = status.IsRetweeted;
			//ReUser = (status.RetweetedStatus != null) ? status.RetweetedStatus.User : null;
			width = 500;
			
		}
	}
}
