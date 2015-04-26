using CoreTweet;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPBtter
{
	public class TweetList
	{
		public ObservableCollection<Tweet> Items { get; private set; }

		public TweetList()
		{
			Items = new ObservableCollection<Tweet>();
		}

		virtual public void Add(Tweet tweet)
		{
			if(Items.Count > 100)
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

			foreach(var subList in SubLists.Where(x => x.MatchFilter(tweet.status.User.ScreenName)))
			{
				subList.Add(tweet);
			}
		}

	}

	/// <summary>
	/// 振り分け後のTweetリスト
	/// </summary>
	public class SubTweetList : TweetList, INotifyPropertyChanged
	{
		private string _Title;
		//private ObservableCollection<string> _NameFilter;
		public ObservableCollection<string> NameFilter { get; private set; }
		public ObservableCollection<string> ExcludeFilter { get; private set; }
		public string Title
		{
			get { return _Title; }
			set { _Title = value; OnPropertyChanged("Title"); OnPropertyChanged("DisplayTitle"); }
		}
		public string DisplayTitle
		{
			get { return _Title + " (" + NameFilter.Count() + ")"; }
		}
		


		public SubTweetList()
		{
			//NameFilter = new Dictionary<string, bool>();
			NameFilter = new ObservableCollection<string>();
			ExcludeFilter = new ObservableCollection<string>();
		}
		public SubTweetList(string title, IEnumerable<string> filter)
		{
			_Title = title;
			//NameFilter = new Dictionary<string, bool>();
			//foreach(var name in filter)
			//{
			//	NameFilter.Add(name, true);
			//}
			NameFilter = new ObservableCollection<string>(filter);
			ExcludeFilter = new ObservableCollection<string>();
		}

		public bool MatchFilter(string nameId)
		{
			//return NameFilter.ContainsKey(screenName) && NameFilter[screenName] == true;
			return NameFilter.Contains(nameId);
		}

		public void Entry(Tweet tweet)
		{
			base.Add(tweet);

			FilterEntry(tweet.NameId);
			OnPropertyChanged("DisplayTitle");
		}

		public void FilterEntry(string nameId)
		{
			//NameFilter[screenName] = true;
			NameFilter.Add(nameId);
			OnPropertyChanged("NameFilter");
		}

		public void FilterRemove(string nameId)
		{
			NameFilter.Remove(nameId);
			OnPropertyChanged("NameFilter");
		}

		public void FilterExclude(string nameId)
		{
			//NameFilter[screenName] = false;
			FilterRemove(nameId);
			ExcludeFilter.Add(nameId);
		}

		public void NameFilterRemove(string nameId)
		{
			NameFilter.Remove(nameId);
			OnPropertyChanged("NameFilter");
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
		public Status status { get; private set; }

		public long Id { get; set; }
		public string NameId { get; set; }
		public string Text { get; set; }
		public DateTimeOffset CreatedAt { get; set; }
		public Uri ProfileImageUrl { get; set; }

		public bool IsRetweet { get; set; }
		public string DisplayName { get; set; }


		public Tweet()
		{
		}
		public Tweet(Status status)
		{
			this.status = status;

			if(status.RetweetedStatus == null)
			{
				IsRetweet = false;
				Id = status.Id;
				NameId = status.User.ScreenName;
				ProfileImageUrl = status.User.ProfileImageUrl;
				DisplayName = status.User.Name + " @" + status.User.ScreenName;
				Text = status.Text;
				CreatedAt = status.CreatedAt;
			}
			else
			{
				var rt = status.RetweetedStatus;

				IsRetweet = true;
				Id = rt.Id;
				NameId = status.User.ScreenName;
				ProfileImageUrl = rt.User.ProfileImageUrl;
				DisplayName = rt.User.Name + " @" + rt.User.ScreenName + " (RT:@" + NameId + ")";
				Text = rt.Text;
				CreatedAt = rt.CreatedAt;
			}

		}
	}



	public class TweetSettings
	{
		public IEnumerable<string> Title { get; set; }
		public IEnumerable<string> NameFilter { get; set; }
	}
}
