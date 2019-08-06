using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace AeroCtl.UI
{
	public struct FanConfig
	{
		/// <summary>
		/// Time between updates.
		/// </summary>
		public TimeSpan Interval;

		/// <summary>
		/// Maximum fan ramp up speed per second.
		/// </summary>
		public double RampUpSpeed;

		/// <summary>
		/// Maximum fan ramp down speed per second.
		/// </summary>
		public double RampDownSpeed;

		public static readonly FanConfig Default = new FanConfig
		{
			Interval = TimeSpan.FromSeconds(0.8),
			RampUpSpeed = 0.25,
			RampDownSpeed = 0.03,
		};
	}

	public class SoftwareFanController
	{
		private readonly CancellationTokenSource cts;
		private readonly FanPoint[] curve;
		private readonly FanConfig config;
		private readonly ISoftwareFanProvider provider;
		private readonly Task task;

		public SoftwareFanController(FanPoint[] curve, FanConfig config, ISoftwareFanProvider provider)
		{
			this.curve = curve ?? throw new ArgumentNullException(nameof(curve));
			if (this.curve.Length == 0)
				throw new ArgumentException("Invalid curve.", nameof(curve));

			this.config = config;
			this.provider = provider ?? throw new ArgumentNullException(nameof(provider));

			this.cts = new CancellationTokenSource();
			this.task = this.runAsync(this.cts.Token);
		}

		public async Task StopAsync()
		{
			this.cts.Cancel();

			try
			{
				await this.task;
			}
			catch (OperationCanceledException)
			{

			}
		}

		private async Task runAsync(CancellationToken cancellationToken)
		{
			await Task.Yield();

			double currentSpeed = double.NaN;
			Stopwatch watch = Stopwatch.StartNew();

			for (;;)
			{
				cancellationToken.ThrowIfCancellationRequested();

				TimeSpan sleep = this.config.Interval - watch.Elapsed;
				if (sleep < TimeSpan.Zero)
					sleep = TimeSpan.Zero;
				watch.Restart();
				await Task.Delay(sleep, cancellationToken);

				double temperature = await this.provider.GetTemperatureAsync(cancellationToken);
				double newTarget;

				int index = 0;

				for (int i = 0; i < this.curve.Length; ++i)
				{
					if (this.curve[i].Temperature < temperature)
					{
						index = i;
					}
				}

				if (index < this.curve.Length - 1)
				{
					double t = (temperature - this.curve[index].Temperature) / (this.curve[index + 1].Temperature - this.curve[index].Temperature);
					newTarget = this.curve[index].FanSpeed * (1.0 - t) + this.curve[index + 1].FanSpeed * t;
				}
				else
				{
					newTarget = this.curve[index].FanSpeed;
				}

				if (double.IsNaN(currentSpeed))
				{
					currentSpeed = newTarget;
				}
				else
				{
					double diff = newTarget - currentSpeed;
					double rampScale = watch.Elapsed.TotalSeconds;

					if (diff > 0.0)
					{
						diff = Math.Min(diff, this.config.RampUpSpeed * rampScale);
					}
					else if (diff < 0.0)
					{
						diff = -Math.Min(-diff, this.config.RampDownSpeed * rampScale);
					}

					currentSpeed += diff;
				}

				await this.provider.SetSpeedAsync(currentSpeed, cancellationToken);
			}
		}
	}
}
