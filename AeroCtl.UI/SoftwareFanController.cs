using System;
using System.Threading;
using System.Threading.Tasks;

namespace AeroCtl.UI
{
	public interface ISoftwareFanProvider
	{
		Task<double> GetTemperatureAsync(CancellationToken cancellationToken);
		Task SetSpeedAsync(double speed, CancellationToken cancellationToken);
	}

	public class SoftwareFanController
	{
		private readonly CancellationTokenSource cts;
		private readonly FanPoint[] curve;
		private readonly TimeSpan interval;
		private readonly ISoftwareFanProvider provider;
		private readonly Task task;

		public SoftwareFanController(FanPoint[] curve, TimeSpan interval, ISoftwareFanProvider provider)
		{
			this.curve = curve ?? throw new ArgumentNullException(nameof(curve));
			if (this.curve.Length == 0)
				throw new ArgumentException("Invalid curve.", nameof(curve));

			this.interval = interval;
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
			catch (TaskCanceledException)
			{

			}
		}

		private async Task runAsync(CancellationToken cancellationToken)
		{
			await Task.Yield();

			for (;;)
			{
				cancellationToken.ThrowIfCancellationRequested();

				await Task.Delay(interval, cancellationToken);

				double temperature = await this.provider.GetTemperatureAsync(cancellationToken);
				double fanSpeed;

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
					fanSpeed = this.curve[index].FanSpeed * (1.0 - t) + this.curve[index + 1].FanSpeed * t;
				}
				else
				{
					fanSpeed = this.curve[index].FanSpeed;
				}

				await this.provider.SetSpeedAsync(fanSpeed, cancellationToken);
			}
		}
	}
}
