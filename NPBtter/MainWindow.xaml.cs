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
using System.Xml.Serialization;
using System.IO;

namespace NPBtter
{
	/// <summary>
	/// MainWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class MainWindow : Window
	{
		private Tokens tokens;
		private MainTweetList TweetList;
		private IDisposable conn;
		private bool isStreaming = false;

		public MainWindow()
		{
			InitializeComponent();

			this.tokens = null;
			this.TweetList = new MainTweetList();
			this.TweetList.SubLists.Add(new SubTweetList() { Title = "list1" });
			this.TweetList.SubLists.Add(new SubTweetList() { Title = "list2" });

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
					TweetList.Receive(status);
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

			this.mainList.ItemsSource = this.TweetList.Items;
			this.leftComboBox.ItemsSource = this.TweetList.SubLists;
			this.rightComboBox.ItemsSource = this.TweetList.SubLists;


			//for(int i = 0; i < 5; i++)
			//{
			//	Tws.SubLists[i % 2].FilterEntry("name" + i);
			//}

			//Action func = async () =>
			//{
			//	for(int i = 0; i < 100; i++)
			//	{
			//		await Task.Run(() => System.Threading.Thread.Sleep(951));
			//		Tws.Add(new Tweet()
			//		{
			//			Name = "Name",
			//			ScreenName = "name" + DateTime.Now.Millisecond % 10,
			//			Text = new String('A', DateTime.Now.Millisecond % 140),
			//			CreatedAt = DateTime.Now
			//		});
			//	}
			//};

			//func();
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if(tokens != null)
			{
				Properties.Settings.Default.AccessToken = tokens.AccessToken;
				Properties.Settings.Default.AccessTokenSecret = tokens.AccessTokenSecret;
				Properties.Settings.Default.Save();


				// XmlSerializerを使ってファイルに保存（TwitSettingオブジェクトの内容を書き込む）
				XmlSerializer serializer = new XmlSerializer(typeof(TweetSettings));

				// カレントディレクトリに"settings.xml"というファイルで書き出す
				FileStream fs = new FileStream(Directory.GetCurrentDirectory() + "\\" + "settings.xml", FileMode.Create);

				// オブジェクトをシリアル化してXMLファイルに書き込む
				serializer.Serialize(fs, this.TweetList.SubLists);
				fs.Close();
			}
		}

		

		


		private void Button_Click(object sender, RoutedEventArgs e)
		{
			TweetList.Add(new Tweet()
			{
				DisplayName = "Name @name" + DateTime.Now.Millisecond % 10,
				Text = new String('A', DateTime.Now.Millisecond % 140),
				CreatedAt = DateTime.Now
			});
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
						.Subscribe(status => this.TweetList.Receive(status));

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

		private double borderWidth = SystemParameters.ResizeFrameVerticalBorderWidth * 2;
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

				this.TweetList.SubLists[index].Entry(tw);
			}
		}
		private void rightListBox_Drop(object sender, DragEventArgs e)
		{
			int index = this.rightComboBox.SelectedIndex;
			if(index > -1)
			{
				var item = e.Data.GetData(typeof(ListBoxItem)) as ListBoxItem;
				var tw = item.Content as Tweet;

				this.TweetList.SubLists[index].Entry(tw);
			}
		}


		private void leftComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			int index = (sender as ComboBox).SelectedIndex;
			if(index > -1)
			{
				this.leftListBox.ItemsSource = this.TweetList.SubLists[index].Items;
			}
		}

		private void rightComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			int index = (sender as ComboBox).SelectedIndex;
			if(index > -1)
			{
				this.rightListBox.ItemsSource = this.TweetList.SubLists[index].Items;
			}
		}


		private void listBoxItem_MouseDoubleClick(object sender, RoutedEventArgs e)
		{
			var item = sender as ListBoxItem;
			var tweet = item.Content as Tweet;
			string url = "https://twitter.com/" + tweet.NameId + "/status/" + tweet.Id;
			System.Diagnostics.Process.Start(url);
		}

		private void leftOptionButton_Click(object sender, RoutedEventArgs e)
		{
			var listMenu = new ListMenu(this.TweetList);
			listMenu.Owner = this;
			listMenu.ShowDialog();
		}

		private void rightOptionButton_Click(object sender, RoutedEventArgs e)
		{
			var listMenu = new ListMenu(this.TweetList);
			listMenu.Owner = this;
			listMenu.ShowDialog();

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


	
		
	
}
