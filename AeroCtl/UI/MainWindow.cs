using System.Windows.Forms;

namespace AeroCtl.UI
{
	public partial class MainWindow : Form
	{
		private readonly Aero aero;

		public MainWindow(Aero aero)
		{
			this.InitializeComponent();
			this.aero = aero;
		}
	}
}
