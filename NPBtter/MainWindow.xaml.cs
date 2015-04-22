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

		private TweetSet Tws { get; set; }
		private TweetSet lTws { get; set; }
		private TweetSet rTws { get; set; }

		private IDisposable conn;
		private bool isStreaming = false;

		public MainWindow()
		{
			InitializeComponent();

			this.tokens = null;
			this.Tws = new TweetSet();
			Tws.Items.CollectionChanged += (s, e) =>
			{
				//var t = new Tweet(Tws.Items[0]);
				var t = Tws.Items[0];
				if(t.Text.Length % 2 == 0)
				{
					lTws.Set(t);
				}
				if(t.Text.Length % 3 == 0)
				{
					rTws.Set(t);
				}
			};
			lTws = new TweetSet();
			rTws = new TweetSet();

			// load twitter token
			if(!string.IsNullOrEmpty(Properties.Settings.Default.AccessToken)
				&& !string.IsNullOrEmpty(Properties.Settings.Default.AccessTokenSecret))
			{
				tokens = Tokens.Create(
					Properties.Resources.APIKEY,
					Properties.Resources.APISECRET,
					Properties.Settings.Default.AccessToken,
					Properties.Settings.Default.AccessTokenSecret);

				foreach(var status in tokens.Statuses.HomeTimeline(count => 10).Reverse())
				{
					Tws.Set(status);
				}
			}

			if(this.tokens == null)
			{
				var optionWindow = new Option();
				optionWindow.ShowDialog();

				this.tokens = optionWindow.tokens;
				if(this.tokens == null)
				{
					this.Close();
				}
			}

			this.mainList.ItemsSource = this.Tws.Items;
			this.leftList.ItemsSource = this.lTws.Items;
			this.rightList.ItemsSource = this.rTws.Items;
		}



		

		


		private void Button_Click(object sender, RoutedEventArgs e)
		{
			Tws.Items.Insert(0, new Tweet()
			{
				Name = "name",
				Text = new String('A', DateTime.Now.Millisecond % 140),
				CreatedAt = DateTime.Now
			});
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

		private void startMenu_Click(object sender, RoutedEventArgs e)
		{
			if(!isStreaming)
			{
				try
				{
					var stream = this.tokens.Streaming.StartObservableStream(StreamingType.User).Publish();
					stream.OfType<StatusMessage>()
						.Select(message => message.Status)
						.ObserveOn(SynchronizationContext.Current)
						.Subscribe(status => this.Tws.Set(status));

					this.conn = stream.Connect();
					this.isStreaming = true;
				}
				catch(Exception ex)
				{
					Console.WriteLine(ex.Message);
				}
			}
			else
			{
				this.conn.Dispose();
				this.isStreaming = false;
			}

			this.startMenu.Header = (isStreaming) ? "Stop" : "Start";
		}

		private void viewMenu_Click(object sender, RoutedEventArgs e)
		{
			if(this.mainColumn.Width == new GridLength(0))
			{
				this.mainList.Visibility = System.Windows.Visibility.Visible;
				this.mainColumn.Width = new GridLength(1,GridUnitType.Star);
				this.viewMenu.Header = "3 Columns";
			}
			else
			{
				this.mainList.Visibility = System.Windows.Visibility.Hidden;
				this.mainColumn.Width = new GridLength(0);
				this.viewMenu.Header = "2 Columns";
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
		public void Set(Tweet tweet)
		{
			this.Items.Insert(0, tweet);
		}
	}

	public class MainTweetSet : TweetSet
	{
		public List<SubTweetSet> SubSets { get; private set; }

		public MainTweetSet()
		{
			SubSets = new List<SubTweetSet>();
		}
	}

	public class SubTweetSet : TweetSet
	{
		public List<string> NameList { get; set; }

		public SubTweetSet()
		{
			NameList = new List<string>();
		}
	}

	public class Tweet
	{
		public long Id { get; set; }
		public string Name { get; set; }
		public string ScreenName { get; set; }
		public long? RtId { get; set; }
		public string RtName { get; set; }
		public string RtScreenName { get; set; }
		public string Text { get; set; }
		public DateTimeOffset CreatedAt { get; set; }
		public Uri ProfileImageUrl { get; set; }

		public string DisplayText
		{
			get
			{
				return Name + " / @" + ScreenName + "\n" + Text + "\n" + CreatedAt;
			}
		}


		public Tweet()
		{
		}
		public Tweet(Status status)
		{
			if(status.RetweetedStatus == null)
			{
				Id = status.Id;
				Name = status.User.Name;
				ScreenName = status.User.ScreenName;
				RtId = null;
				RtName = null;
				RtScreenName = null;
				Text = status.Text;
				CreatedAt = status.CreatedAt;
				ProfileImageUrl = status.User.ProfileImageUrl;
			}
			else
			{
				var rt = status.RetweetedStatus;

				Id = rt.Id;
				Name = rt.User.Name;
				ScreenName = rt.User.ScreenName;
				RtId = status.Id;
				RtName = status.User.Name;
				RtScreenName = status.User.ScreenName;
				Text = rt.Text;
				CreatedAt = rt.CreatedAt;
				ProfileImageUrl = rt.User.ProfileImageUrl;
			}
			
		}
		
	}
}
