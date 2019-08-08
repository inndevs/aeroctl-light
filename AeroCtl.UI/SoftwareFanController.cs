using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace AeroCtl.UI
{
	public enum FanSchedulingMode
	{
		AsyncTask,
		NormalThread,
		AboveNormalThread,
		HighestThread,
	}

	public class SoftwareFanController
	{
		private readonly CancellationTokenSource cts;
		private readonly FanPoint[] curve;
		private readonly FanConfig config;
		private readonly ISoftwareFanProvider provider;
		
		private readonly Task task;
		private readonly Thread thread;

		private readonly Stopwatch watch;
		private double currentSpeed;

		public SoftwareFanController(FanPoint[] curve, FanConfig config, ISoftwareFanProvider provider)
		{
			this.curve = curve ?? throw new ArgumentNullException(nameof(curve));
			if (this.curve.Length == 0)
				throw new ArgumentException("Invalid curve.", nameof(curve));

			this.config = config;
			this.provider = provider ?? throw new ArgumentNullException(nameof(provider));

			this.watch = new Stopwatch();
			this.currentSpeed = double.NaN;

			this.cts = new CancellationTokenSource();

			switch (config.SchedulingMode)
			{
				case FanSchedulingMode.AsyncTask:
					this.task = this.runAsync(this.cts.Token);
					break;

				case FanSchedulingMode.NormalThread:
					this.thread = new Thread(() => this.runThread(this.cts.Token))
					{
						Priority = ThreadPriority.Normal
					};
					break;

				case FanSchedulingMode.AboveNormalThread:
					this.thread = new Thread(() => this.runThread(this.cts.Token))
					{
						Priority = ThreadPriority.AboveNormal
					};
					break;

				case FanSchedulingMode.HighestThread:
					this.thread = new Thread(() => this.runThread(this.cts.Token))
					{
						Priority = ThreadPriority.Highest
					};
					break;
			}

			this.thread?.Start();
		}

		public async Task StopAsync()
		{
			this.cts.Cancel();

			try
			{
				if (this.task != null)
					await this.task;
			}
			catch (OperationCanceledException)
			{

			}

			this.thread?.Join();
		}

		private async ValueTask update(CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			double secondsPassed = this.watch.Elapsed.TotalSeconds;
			this.watch.Restart();

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

				if (diff > 0.0)
				{
					diff = Math.Min(diff, this.config.RampUpSpeed * secondsPassed);
				}
				else if (diff < 0.0)
				{
					diff = -Math.Min(-diff, this.config.RampDownSpeed * secondsPassed);
				}

				currentSpeed += diff;
			}

			await this.provider.SetSpeedAsync(currentSpeed, cancellationToken);
		}

		private async Task runAsync(CancellationToken cancellationToken)
		{
			for (;;)
			{
				await Task.Delay(this.config.Interval, cancellationToken);
				await this.update(cancellationToken);
			}
		}

		private void runThread(CancellationToken cancellationToken)
		{
			try
			{
				while (!cancellationToken.IsCancellationRequested)
				{
					Thread.Sleep(this.config.Interval);
					this.update(cancellationToken).AsTask().Wait(cancellationToken);
				}
			}
			catch (OperationCanceledException)
			{

			}
		}
	}
}
