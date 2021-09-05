using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using AeroCtl.UI.SoftwareFan;
using Application = System.Windows.Application;

namespace AeroCtl.UI
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private bool supressShutdownOnClose;

		public AeroController Controller { get; }

		public string GitInfo => $"AeroCtl v{ThisAssembly.Git.BaseTag}";

		public MainWindow(AeroController controller)
		{
			this.Controller = controller;
			this.InitializeComponent();
		}

		protected override void OnStateChanged(EventArgs e)
		{
			base.OnStateChanged(e);

			if (this.WindowState == WindowState.Minimized)
			{
				// Minimizing should close the window, but not exist the app.
				this.supressShutdownOnClose = true;
				this.Close();
			}
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			if (this.supressShutdownOnClose)
				return;

			Application.Current.Shutdown();
		}

		private void onEditHwCurveClicked(object sender, RoutedEventArgs e)
		{
			FanCurve curve = this.Controller.Aero.Fans.GetFanCurve();
			FanPoint[] clone = curve.ToArray();
			FanCurveEditor editor = new FanCurveEditor(clone, FanCurveKind.Step);
			editor.CurveApplied += (s, e2) =>
			{
				for (int i = 0; i < curve.Count; ++i)
				{
					curve[i] = clone[i];
				}
			};
			editor.ShowDialog();
		}

		private void onEditSwCurveClicked(object sender, RoutedEventArgs e)
		{
			FanConfig cfg = new FanConfig(this.Controller.SoftwareFanConfig);
			List<FanPoint> curve = new List<FanPoint>(cfg.Curve);
			FanCurveEditor editor = new FanCurveEditor(curve, FanCurveKind.Linear);
			editor.CurveApplied += (s, e2) =>
			{
				cfg.Curve = curve.ToImmutableArray();
				this.Controller.SoftwareFanConfig = cfg;
			};
			editor.ShowDialog();
		}

		private void onEditSwConfigClicked(object sender, RoutedEventArgs e)
		{
			FanConfig cfg = new FanConfig(this.Controller.SoftwareFanConfig);
			FanConfigEditor editor = new FanConfigEditor(cfg);

			if (editor.ShowDialog() == true)
				this.Controller.SoftwareFanConfig = cfg;
		}

		private async void onResetKeyboardClicked(object sender, RoutedEventArgs e)
		{
			var messageBoxResult = MessageBox.Show(
				"This will reset all keyboard settings (e.g. RGB LED colors). Are you sure?",
				"Reset keyboard",
				MessageBoxButton.YesNo,
				MessageBoxImage.Question);
			if (messageBoxResult == MessageBoxResult.Yes)
				await this.Controller.ResetKeyboard();
		}
	}
}
