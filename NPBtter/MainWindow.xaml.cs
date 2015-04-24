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
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.ComponentModel;

namespace NPBtter
{
	/// <summary>
	/// MainWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class MainWindow : Window
	{
		private Tokens tokens;
		private MainTweetList Tws;
		private IDisposable conn;
		private bool isStreaming = false;

		public MainWindow()
		{
			InitializeComponent();

			this.tokens = null;
			this.Tws = new MainTweetList();
			this.Tws.SubLists.Add(new SubTweetList() { Title = "list1" });
			this.Tws.SubLists.Add(new SubTweetList() { Title = "list2" });

			// load twitter token
			if(!string.IsNullOrEmpty(Properties.Settings.Default.AccessToken)
				&& !string.IsNullOrEmpty(Properties.Settings.Default.AccessTokenSecret))
			{
				//tokens = Tokens.Create(
				//	Properties.Resources.APIKEY,
				//	Properties.Resources.APISECRET,
				//	Properties.Settings.Default.AccessToken,
				//	Properties.Settings.Default.AccessTokenSecret);

				//foreach(var status in tokens.Statuses.HomeTimeline(count => 10).Reverse())
				//{
				//	Tws.Set(status);
				//}
			}

			if(this.tokens == null)
			{
				var optionWindow = new Option();
				optionWindow.ShowDialog();

				this.tokens = optionWindow.tokens;
				if(this.tokens == null)
				{
					//this.Close();
				}
			}

			this.mainList.ItemsSource = this.Tws.Items;
			this.leftComboBox.ItemsSource = this.Tws.SubLists;
			this.rightComboBox.ItemsSource = this.Tws.SubLists;


			for(int i = 0; i < 5; i++)
			{
				Tws.SubLists[i % 2].FilterEntry("name" + i);
			}

			Action func = async () =>
			{
				for(int i = 0; i < 100; i++)
				{
					await Task.Run(() => System.Threading.Thread.Sleep(951));
					Tws.Add(new Tweet()
					{
						Name = "Name",
						ScreenName = "name" + DateTime.Now.Millisecond % 10,
						Text = new String('A', DateTime.Now.Millisecond % 140),
						CreatedAt = DateTime.Now
					});
				}
			};

			func();
		}



		

		


		private void Button_Click(object sender, RoutedEventArgs e)
		{
			Tws.Add(new Tweet()
			{
				Name = "Name",
				ScreenName = "name" + DateTime.Now.Millisecond % 10,
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
						.Subscribe(status => this.Tws.Receive(status));

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

		//private double borderWidth = SystemParameters.ResizeFrameVerticalBorderWidth * 2;
		private void viewMenu_Click(object sender, RoutedEventArgs e)
		{
			if(this.mainList.Visibility == System.Windows.Visibility.Hidden)
			{
				this.mainList.Visibility = System.Windows.Visibility.Visible;
				this.mainColumn.Width = new GridLength(1, GridUnitType.Star);
				//this.Width = (this.Width - borderWidth) / 2 * 3 + borderWidth;
				//this.Width = this.Width / 2 * 3 - (borderWidth / 3);
				this.viewMenu.Header = "3 Columns";
			}
			else
			{
				this.mainList.Visibility = System.Windows.Visibility.Hidden;
				this.mainColumn.Width = new GridLength(0);
				//this.Width = (this.Width - borderWidth) / 3 * 2 + borderWidth;
				//this.Width = this.Width / 3 * 2 + (borderWidth / 3);
				this.viewMenu.Header = "2 Columns";
			}
		}





		private ListBoxItem dragItem;
		private Point dragStartPos;
		private DragAdorner dragGhost;
		
		private void listBoxItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			// マウスダウンされたアイテムを記憶
			dragItem = sender as ListBoxItem;
			// マウスダウン時の座標を取得
			dragStartPos = e.GetPosition(dragItem);
		}
		private void listBoxItem_PreviewMouseMove(object sender, MouseEventArgs e)
		{
			var lbi = sender as ListBoxItem;
			if(e.LeftButton == MouseButtonState.Pressed && dragGhost == null && dragItem == lbi)
			{
				var nowPos = e.GetPosition(lbi);
				if(Math.Abs(nowPos.X - dragStartPos.X) > SystemParameters.MinimumHorizontalDragDistance ||
					Math.Abs(nowPos.Y - dragStartPos.Y) > SystemParameters.MinimumVerticalDragDistance)
				{
					//mainList.AllowDrop = true;

					var layer = AdornerLayer.GetAdornerLayer(mainList);
					dragGhost = new DragAdorner(mainList, lbi, 0.5, dragStartPos);
					layer.Add(dragGhost);
					DragDrop.DoDragDrop(lbi, lbi, DragDropEffects.Move);
					layer.Remove(dragGhost);
					dragGhost = null;
					dragItem = null;

					//mainList.AllowDrop = false;
				}
			}
		}
		private void listBoxItem_QueryContinueDrag(object sender, QueryContinueDragEventArgs e)
		{
			if(dragGhost != null)
			{
				var p = CursorInfo.GetNowPosition(this);
				var loc = this.PointFromScreen(mainList.PointToScreen(new Point(0, 0)));
				dragGhost.LeftOffset = p.X - loc.X;
				dragGhost.TopOffset = p.Y - loc.Y;
			}
		}
		private void leftListBox_Drop(object sender, DragEventArgs e)
		{
			int index = this.leftComboBox.SelectedIndex;
			if(index > -1)
			{
				var item = e.Data.GetData(typeof(ListBoxItem)) as ListBoxItem;
				var tw = item.Content as Tweet;

				this.Tws.SubLists[index].Entry(tw);
			}
		}
		private void rightListBox_Drop(object sender, DragEventArgs e)
		{
			int index = this.rightComboBox.SelectedIndex;
			if(index > -1)
			{
				var item = e.Data.GetData(typeof(ListBoxItem)) as ListBoxItem;
				var tw = item.Content as Tweet;

				this.Tws.SubLists[index].Entry(tw);
			}
		}


		private void leftComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			int index = (sender as ComboBox).SelectedIndex;
			if(index > -1)
			{
				this.leftListBox.ItemsSource = this.Tws.SubLists[index].Items;
			}
		}

		private void rightComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			int index = (sender as ComboBox).SelectedIndex;
			if(index > -1)
			{
				this.rightListBox.ItemsSource = this.Tws.SubLists[index].Items;
			}
		}

		













	}
	public static class CursorInfo
	{
		[DllImport("user32.dll")]
		private static extern void GetCursorPos(out POINT pt);

		[DllImport("user32.dll")]
		private static extern int ScreenToClient(IntPtr hwnd, ref POINT pt);

		private struct POINT
		{
			public UInt32 X;
			public UInt32 Y;
		}

		public static Point GetNowPosition(Visual v)
		{
			POINT p;
			GetCursorPos(out p);

			var source = HwndSource.FromVisual(v) as HwndSource;
			var hwnd = source.Handle;

			ScreenToClient(hwnd, ref p);
			return new Point(p.X, p.Y);
		}
	}
	class DragAdorner : Adorner
	{
		protected UIElement _child;
		protected double XCenter;
		protected double YCenter;

		public DragAdorner(UIElement owner) : base(owner) { }

		public DragAdorner(UIElement owner, UIElement adornElement, double opacity, Point dragPos)
			: base(owner)
		{
			var _brush = new VisualBrush(adornElement) { Opacity = opacity };
			var b = VisualTreeHelper.GetDescendantBounds(adornElement);
			var r = new Rectangle() { Width = b.Width, Height = b.Height };

			XCenter = dragPos.X;// r.Width / 2;
			YCenter = dragPos.Y;// r.Height / 2;

			r.Fill = _brush;
			_child = r;
		}


		private double _leftOffset;
		public double LeftOffset
		{
			get { return _leftOffset; }
			set
			{
				_leftOffset = value - XCenter;
				UpdatePosition();
			}
		}

		private double _topOffset;
		public double TopOffset
		{
			get { return _topOffset; }
			set
			{
				_topOffset = value - YCenter;
				UpdatePosition();
			}
		}

		private void UpdatePosition()
		{
			var adorner = this.Parent as AdornerLayer;
			if(adorner != null)
			{
				adorner.Update(this.AdornedElement);
			}
		}

		protected override Visual GetVisualChild(int index)
		{
			return _child;
		}

		protected override int VisualChildrenCount
		{
			get { return 1; }
		}

		protected override Size MeasureOverride(Size finalSize)
		{
			_child.Measure(finalSize);
			return _child.DesiredSize;
		}
		protected override Size ArrangeOverride(Size finalSize)
		{

			_child.Arrange(new Rect(_child.DesiredSize));
			return finalSize;
		}

		public override GeneralTransform GetDesiredTransform(GeneralTransform transform)
		{
			var result = new GeneralTransformGroup();
			result.Children.Add(base.GetDesiredTransform(transform));
			result.Children.Add(new TranslateTransform(_leftOffset, _topOffset));
			return result;
		}
	}


	public class TweetList
	{
		public ObservableCollection<Tweet> Items { get; private set; }

		public TweetList()
		{
			Items = new ObservableCollection<Tweet>();
		}

		virtual public void Add(Tweet tweet)
		{
			if(Items.Count > 10)
			{
				Items.Remove(Items.Last());
			}

			Items.Insert(0, tweet);
		}

	}

	public class MainTweetList : TweetList
	{
		public ObservableCollection<SubTweetList> SubLists { get; private set; }

		public MainTweetList()
		{
			SubLists = new ObservableCollection<SubTweetList>();
		}

		public void Receive(Status status)
		{
			Add(new Tweet(status));
		}

		override public void Add(Tweet tweet)
		{
			base.Add(tweet);

			foreach(var subList in SubLists.Where(x => x.MatchFilter(tweet.ScreenName)))
			{
				subList.Add(tweet);
			}
		}
	}

	public class SubTweetList : TweetList, INotifyPropertyChanged
	{
		private string _Title;
		private Dictionary<string, bool> _NameFilter;

		public string Title
		{
			get { return _Title; }
			set { _Title = value; OnPropertyChanged("DisplayTitle"); }
		}

		public string DisplayTitle
		{
			get { return _Title + " (" + _NameFilter.Count() + ")"; }
		}


		public SubTweetList()
		{
			_NameFilter = new Dictionary<string, bool>();
		}
		public SubTweetList(string title, IEnumerable<string> filter)
		{
			_Title = title;
			_NameFilter = new Dictionary<string, bool>();
			foreach(var name in filter)
			{
				_NameFilter.Add(name, true);
			}
		}

		public bool MatchFilter(string screenName)
		{
			return _NameFilter.ContainsKey(screenName) && _NameFilter[screenName] == true;
		}

		public void Entry(Tweet tweet)
		{
			base.Add(tweet);

			FilterEntry(tweet.ScreenName);
		}

		public void FilterEntry(string screenName)
		{
			_NameFilter[screenName] = true;
			OnPropertyChanged("DisplayTitle");
		}

		public void FilterRemove(string screenName)
		{
			_NameFilter.Remove(screenName);
			OnPropertyChanged("DisplayTitle");
		}

		public void FilterExclude(string screenName)
		{
			_NameFilter[screenName] = false;
		}


		// INotifyPropertyChangedの実装
		public event PropertyChangedEventHandler PropertyChanged;
		protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
		{
			if(PropertyChanged != null) PropertyChanged(this, e);
		}
		protected virtual void OnPropertyChanged(string name)
		{
			if(PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(name));
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
