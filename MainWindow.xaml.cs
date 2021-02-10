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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Flurl.Http;

namespace W2AutoUpdater
{
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			try
			{
				this.SizeToContent = SizeToContent.WidthAndHeight;
				this.ResizeMode = ResizeMode.CanMinimize;
				this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
				this.WindowState = WindowState.Normal;

				this.Foreground = Brushes.White;

				this.Background = Brushes.Black;

				this.AddChild(this.GetMainContainer());

				this.InitializeComponent();

				this.Title = "W2AutoUpdater";
			}
			catch (Exception ex)
			{
				this.ShowError(ex);
			}
		}

		private StackPanel GetMainContainer()
		{
			var mainContainer = new StackPanel
			{
				Orientation = Orientation.Vertical,
				Height = 540,
				Width = 960,
			};

			mainContainer.Children.Add(this.GetTopContainer());
			mainContainer.Children.Add(this.GetBotContainer());

			return mainContainer;
		}

		private StackPanel GetTopContainer()
		{
			var topContainer = new StackPanel
			{
				Orientation = Orientation.Horizontal,
				Height = 400,
			};

			topContainer.Children.Add(this.GetNews());
			topContainer.Children.Add(this.GetChannelsStatus());

			return topContainer;
		}

		private ScrollViewer GetNews()
		{
			var scrollView = new ScrollViewer
			{
				Width = 720,
				Padding = new Thickness(15),
			};

			return scrollView;
		}

		private ScrollViewer GetChannelsStatus()
		{
			var statusUrl = "https://www.overdestiny.com.br/over/servers/serv00.htm";

			var scrollView = new ScrollViewer
			{
				Width = 240,
				Padding = new Thickness(15),
			};

			var scrollContent = new StackPanel
			{
				Orientation = Orientation.Vertical,
			};

			scrollView.Content = scrollContent;

			new Task(async () =>
			{
				while (true)
				{
					try
					{
						var channels = (await statusUrl.GetStringAsync()).Split(" ").Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s) && s != "-1").Select(s =>
							{
								int.TryParse(s, out int connections);
								return connections;
							}).ToList();

						channels.Add(100);
						channels.Add(200);
						channels.Add(300);

						await scrollContent.Dispatcher.InvokeAsync(() =>
						{
							scrollContent.Children.Clear();

							int i = 0;
							StackPanel item = null;
							channels.ForEach(connections =>
							{
								if (item != null)
								{
									var margin = item.Margin;
									margin.Bottom = 15;
									item.Margin = margin;
								}

								item = this.GetChannelItem(i++, connections);

								scrollContent.Children.Add(item);
							});
						});
					}
					catch (Exception ex)
					{
						this.ShowError(ex);
					}
					finally
					{
						await Task.Delay(TimeSpan.FromMinutes(1));
					}
				}
			}).Start();

			return scrollView;
		}

		private StackPanel GetChannelItem(int channelID, int connections)
		{
			var channelBox = new StackPanel
			{
				Orientation = Orientation.Vertical,
			};

			var text = new TextBlock
			{
				Text = $"Canal - {(channelID + 1):N0}",
				Margin = new Thickness(0, 0, 0, 5),
			};

			var progressBar = new ProgressBar
			{
				Value = connections,
				Maximum = 700,
				Height = 15,
			};

			channelBox.Children.Add(text);
			channelBox.Children.Add(progressBar);

			return channelBox;
		}

		private StackPanel GetBotContainer()
		{
			var container = new StackPanel
			{
				Orientation = Orientation.Horizontal,
				Height = 140,
			};

			return container;
		}

		private void ShowError(Exception ex)
		{
			try
			{
				this.Dispatcher.Invoke(() =>
				{
					MessageBox.Show(ex.ToString(), "Erro!", MessageBoxButton.OK, MessageBoxImage.Error);
				});
			}
			catch
			{
				Environment.Exit(0);
			}
		}
	}
}
