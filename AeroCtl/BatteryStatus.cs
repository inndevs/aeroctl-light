namespace AeroCtl
{
	public readonly struct BatteryStatus
	{
		/// <summary>
		/// Gets the current battery charge in Wh.
		/// </summary>
		public double Charge { get; }

		/// <summary>
		/// Gets the (estimated) charge percent.s
		/// </summary>
		public int ChargePercent { get; }

		/// <summary>
		/// Gets the charge rate in W.
		/// </summary>
		public double ChargeRate { get; }

		/// <summary>
		/// Gets the discharge rate in W.
		/// </summary>
		public double DischargeRate { get; }

		/// <summary>
		/// Gets the voltage in V.
		/// </summary>
		public double Voltage { get; }

		public BatteryStatus(double charge, int chargePercent, double chargeRate, double dischargeRate, double voltage)
		{
			this.Charge = charge;
			this.ChargePercent = chargePercent;
			this.ChargeRate = chargeRate;
			this.DischargeRate = dischargeRate;
			this.Voltage = voltage;
		}
	}
}
