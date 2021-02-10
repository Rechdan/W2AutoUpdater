using System;
using System.Collections.Generic;
using System.IO;
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
using Flurl.Http;
using LibGit2Sharp;

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

			container.Children.Add(this.GetDownloadProgress());
			container.Children.Add(this.GetStartButton());

			return container;
		}

		private TextBlock DownloadProgressText = null;
		private ProgressBar DownloadProgressBar = null;

		private void SetProgressText(string newText)
		{
			this.DownloadProgressText.Dispatcher.Invoke(() =>
			{
				this.DownloadProgressText.Text = newText;
			});
		}

		private void SetProgressBar(int value, int total)
		{
			this.DownloadProgressBar.Dispatcher.Invoke(() =>
			{
				this.DownloadProgressBar.Maximum = total;
				this.DownloadProgressBar.Value = value;
			});
		}

		private StackPanel GetDownloadProgress()
		{
			var container = new StackPanel
			{
				Orientation = Orientation.Vertical,
				Width = 720,
			};

			this.DownloadProgressText = new TextBlock
			{
				Text = "Aguardando...",
			};

			this.DownloadProgressBar = new ProgressBar
			{
				Height = 15,
			};

			container.Children.Add(this.DownloadProgressText);
			container.Children.Add(this.DownloadProgressBar);

			return container;
		}

		private StackPanel GetStartButton()
		{
			var container = new StackPanel
			{
				Orientation = Orientation.Vertical,
				VerticalAlignment = VerticalAlignment.Center,
				Width = 240,
			};

			var button = new Button
			{
				HorizontalAlignment = HorizontalAlignment.Center,
				Padding = new Thickness(20, 10, 20, 10),
				Content = new TextBlock
				{
					Text = "JOGAR"
				},
			};

			button.Click += (a, b) => this.OnStartButtonClick();

			container.Children.Add(button);

			return container;
		}

		private void OnStartButtonClick()
		{
			new Task(() =>
			{
				try
				{
					var repoUrl = "https://github.com/Rechdan/W2AutoUpdater.git";
					var localClient = Path.GetFullPath("./client");

					if (!Repository.IsValid(localClient))
					{
						this.SetProgressText("Baixando cliente");

						var options = new CloneOptions
						{
							BranchName = "client",
							OnCheckoutProgress = (file, value, total) =>
							{
								this.SetProgressText($"Copiando {file}");
								this.SetProgressBar(value, total);
							},
							RepositoryOperationCompleted = (o) =>
							{
								using (var repo = new Repository(localClient))
								{
									var signature = new LibGit2Sharp.Signature(new Identity("CLIENT", "CLIENT@local"), DateTimeOffset.Now);
									var options = new LibGit2Sharp.PullOptions
									{
										FetchOptions = new FetchOptions { }
									};

									Commands.Pull(repo, signature, options);
								}
							},
						};

						Repository.Clone(repoUrl, localClient, options);
					}
					else
					{
						// using (var repo = new Repository(localGit))
						// {
						// 	var options = new LibGit2Sharp.PullOptions
						// 	{
						// 		FetchOptions = new FetchOptions { }
						// 	};

						// 	var signature = new LibGit2Sharp.Signature(new Identity("CLIENT", "CLIENT@local"), DateTimeOffset.Now);

						// 	Commands.Pull(repo, signature, options);
						// }
					}
				}
				catch (Exception ex)
				{
					this.ShowError(ex);
				}
			}).Start();
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
