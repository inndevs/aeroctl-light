using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Point = System.Windows.Point;

namespace AeroCtl.UI
{
	/// <summary>
	/// Interaction logic for FanCurveEditor.xaml
	/// </summary>
	public partial class FanCurveEditor : Window
	{
		#region Fields

		private readonly IList<FanPoint> curve;
		
		#endregion

		#region Properties

		public Canvas Canvas => this.canvas;
		public FanGraphPoint[] Points { get; }
		public FanGraphPoint[] GridPoints { get; }

		#endregion

		#region Constructors

		public FanCurveEditor(IList<FanPoint> curve)
		{
			this.InitializeComponent();

			#region Grid

			this.GridPoints = new FanGraphPoint[11];
			for (int i = 0; i < this.GridPoints.Length; ++i)
			{
				this.GridPoints[i] = new FanGraphPoint(this)
				{
					Temperature = i * 10.0,
					FanSpeed = i * 0.1
				};
			}

			for (int i = 0; i < this.GridPoints.Length; ++i)
			{
				Line vLine = new Line
				{
					Stroke = Brushes.Gray
				};

				this.canvas.Children.Add(vLine);

				vLine.SetBinding(Line.X1Property, $"GridPoints[{i}].X");
				vLine.Y1 = 0.0;
				vLine.SetBinding(Line.X2Property, $"GridPoints[{i}].X");
				vLine.SetBinding(Line.Y2Property, $"Canvas.ActualHeight");

				Line hLine = new Line
				{
					Stroke = Brushes.Gray
				};

				this.canvas.Children.Add(hLine);

				hLine.X1 = 0.0;
				hLine.SetBinding(Line.Y1Property, $"GridPoints[{i}].Y");
				hLine.SetBinding(Line.X2Property, $"Canvas.ActualWidth");
				hLine.SetBinding(Line.Y2Property, $"GridPoints[{i}].Y");

				TextBlock text = new TextBlock();
				this.canvas.Children.Add(text);
				text.SetBinding(TextBlock.TextProperty, $"GridPoints[{i}].Temperature");
				text.SetBinding(Canvas.LeftProperty, $"GridPoints[{i}].X");
				Canvas.SetBottom(text, 0.0);

			}

			#endregion

			this.curve = curve;
			this.canvas.DataContext = this;

			this.Points = new FanGraphPoint[curve.Count];
			for (int i = 0; i < this.Points.Length; ++i)
			{
				this.Points[i] = new FanGraphPoint(this)
				{
					Temperature = curve[i].Temperature,
					FanSpeed = curve[i].FanSpeed
				};
			}


			Brush ellipseBrush = new SolidColorBrush(Colors.Black);
			Brush lineBrush = new SolidColorBrush(Colors.DodgerBlue);

			for (int i = 0; i < this.Points.Length; ++i)
			{
				int j = i;
				Ellipse ellipse = new Ellipse
				{
					Fill = new SolidColorBrush(Colors.Black)
				};

				ellipse.SetBinding(Canvas.LeftProperty, $"Points[{i}].EllipseX");
				ellipse.SetBinding(Canvas.TopProperty, $"Points[{i}].EllipseY");
				ellipse.SetBinding(Ellipse.WidthProperty, $"Points[{i}].EllipseW");
				ellipse.SetBinding(Ellipse.HeightProperty, $"Points[{i}].EllipseH");

				if (i < this.Points.Length - 1)
				{
					Line line1 = new Line()
					{
						StrokeThickness = 2,
						Stroke = lineBrush,
					};
					this.canvas.Children.Add(line1);

					Line line2 = new Line()
					{
						StrokeThickness = 2,
						Stroke = lineBrush,
					};
					this.canvas.Children.Add(line2);

					line1.SetBinding(Line.X1Property, $"Points[{i}].X");
					line1.SetBinding(Line.Y1Property, $"Points[{i}].Y");
					line1.SetBinding(Line.X2Property, $"Points[{i + 1}].X");
					line1.SetBinding(Line.Y2Property, $"Points[{i}].Y");

					line2.SetBinding(Line.X1Property, $"Points[{i + 1}].X");
					line2.SetBinding(Line.Y1Property, $"Points[{i}].Y");
					line2.SetBinding(Line.X2Property, $"Points[{i + 1}].X");
					line2.SetBinding(Line.Y2Property, $"Points[{i + 1}].Y");
				}

				this.canvas.Children.Add(ellipse);

				ellipse.MouseDown += (s, e) =>
				{
					ellipse.CaptureMouse();
				};

				ellipse.MouseUp += (s, e) =>
				{
					ellipse.ReleaseMouseCapture();
				};

				ellipse.MouseMove += (s, e) =>
				{
					if (!ellipse.IsMouseCaptured)
						return;

					Point p = this.canvasToPoint(e.GetPosition(this.canvas));
					p.X = Math.Max(0.0, Math.Min(100.0, p.X));
					p.Y = Math.Max(0.0, Math.Min(1.0, p.Y));

					if (j == 0)
					{
						p.X = 0.0;
					}

					if (j > 0)
					{
						Point p2 = this.Points[j - 1].Point;
						if (p.X < p2.X)
							p.X = p2.X;
						//if (p.Y < p2.Y)
						//	p.Y = p2.Y;
					}

					if (j < this.Points.Length - 1)
					{
						Point p2 = this.Points[j + 1].Point;
						if (p.X > p2.X)
							p.X = p2.X;
						//if (p.Y > p2.Y)
						//	p.Y = p2.Y;
					}

					this.Points[j].Point = p;
				};
			}
		}

		#endregion

		#region Methods

		protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
		{
			base.OnRenderSizeChanged(sizeInfo);

			for (int i = 0; i < this.GridPoints.Length; ++i)
			{
				this.GridPoints[i].Invalidate();
			}

			for (int i = 0; i < this.Points.Length; ++i)
			{
				this.Points[i].Invalidate();
			}
		}

		private Point getGraphSize()
		{
			return new Point(this.canvas.ActualWidth, this.canvas.ActualHeight);
		}

		private Point pointToCanvas(Point p)
		{
			Point s = this.getGraphSize();
			return new Point(p.X / 100 * s.X, (1.0 - p.Y) * s.Y);
		}

		private Point canvasToPoint(Point p)
		{
			Point s = this.getGraphSize();
			return new Point(p.X / s.X * 100, 1.0 - p.Y / s.Y);
		}

		public event EventHandler CurveApplied;

		private void applyButton_OnClick(object sender, RoutedEventArgs e)
		{
			for (int i = 0; i < this.Points.Length; ++i)
			{
				this.curve[i] = new FanPoint
				{
					Temperature = this.Points[i].Temperature,
					FanSpeed = this.Points[i].FanSpeed
				};
			}

			this.CurveApplied?.Invoke(this, EventArgs.Empty);
		}

		#endregion

		#region Nested Types

		public class FanGraphPoint : INotifyPropertyChanged
		{
			private readonly FanCurveEditor editor;

			private double temperature;
			private double fanSpeed;

			public double Temperature
			{
				get => this.temperature;
				set
				{
					this.temperature = value;
					this.OnPropertyChanged();
					this.OnPropertyChanged(nameof(X));
					this.OnPropertyChanged(nameof(EllipseX));
				}
			}

			public double FanSpeed
			{
				get => this.fanSpeed;
				set
				{
					this.fanSpeed = value;
					this.OnPropertyChanged();
					this.OnPropertyChanged(nameof(Y));
					this.OnPropertyChanged(nameof(EllipseY));
				}
			}

			public Point Point
			{
				get => new Point(this.Temperature, this.FanSpeed);
				set
				{
					this.Temperature = value.X;
					this.FanSpeed = value.Y;
				}
			}

			public double X => this.editor.pointToCanvas(this.Point).X;

			public double Y => this.editor.pointToCanvas(this.Point).Y;

			public double EllipseX => this.X - this.EllipseW * 0.5;

			public double EllipseY => this.Y - this.EllipseH * 0.5;

			public double EllipseW => 10.0;

			public double EllipseH => 10.0;


			public FanGraphPoint(FanCurveEditor editor)
			{
				this.editor = editor;
			}

			public void Invalidate()
			{
				this.OnPropertyChanged(nameof(X));
				this.OnPropertyChanged(nameof(Y));
				this.OnPropertyChanged(nameof(EllipseX));
				this.OnPropertyChanged(nameof(EllipseY));
			}

			public event PropertyChangedEventHandler PropertyChanged;
			protected virtual void OnPropertyChanged(string propertyName = null)
			{
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		#endregion
	}
}
