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

			var t = dragItem.Content as Tweet;
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
		private void listBox_Drop(object sender, DragEventArgs e)
		{
			var lbi = e.Data.GetData(typeof(ListBoxItem)) as ListBoxItem;
			var t = lbi.Content as Tweet;

			this.lTws.Set(t);


			//var dropPos = e.GetPosition(mainList);
			//var lbi = e.Data.GetData(typeof(ListBoxItem)) as ListBoxItem;
			//var o = lbi.DataContext as MyClass;
			//var index = MyClasses.IndexOf(o);
			//for(int i = 0; i < MyClasses.Count; i++)
			//{
			//	var item = mainList.ItemContainerGenerator.ContainerFromIndex(i) as ListBoxItem;
			//	var pos = mainList.PointFromScreen(item.PointToScreen(new Point(0, item.ActualHeight / 2)));
			//	if(dropPos.Y < pos.Y)
			//	{
			//		// i が入れ換え先のインデックス
			//		MyClasses.Move(index, (index < i) ? i - 1 : i);
			//		return;
			//	}
			//}
			//// 最後にもっていく
			//int last = MyClasses.Count - 1;
			//MyClasses.Move(index, last);
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
