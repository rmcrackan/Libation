using System;
using System.Drawing;
using System.Windows.Forms;

namespace LibationWinForms.ProcessQueue
{

	internal delegate void RequestDataDelegate(int firstIndex, int numVisible, ProcessBookControl[] panelsToFill);
	internal partial class VirtualFlowControl : UserControl
	{
		public event RequestDataDelegate RequestData;
		public int VirtualControlCount
		{
			get => _virtualControlCount;
			set
			{
				if (_virtualControlCount == 0) 
					vScrollBar1.Value = 0;

				_virtualControlCount = value;
				AdjustScrollBar();
				DoVirtualScroll();
			}
		}

		private int _virtualControlCount;
		private int VirtualHeight => _virtualControlCount * CONTROL_HEIGHT - vScrollBar1.Height;

		private readonly int PROCESSBOOKCONTROL_MARGIN;
		private readonly int CONTROL_HEIGHT;

		private const int WM_MOUSEWHEEL = 522;
		private const int NUM_ACTUAL_CONTROLS = 20;
		private const int SCROLL_SMALL_CHANGE = 120;
		private const int SCROLL_LARGE_CHANGE = 3 * SCROLL_SMALL_CHANGE;


		private readonly VScrollBar vScrollBar1;
		private readonly ProcessBookControl[] BookControls = new ProcessBookControl[NUM_ACTUAL_CONTROLS];

		public VirtualFlowControl()
		{

			InitializeComponent();

			vScrollBar1 = new VScrollBar
			{
				Minimum = 0,
				Value = 0,
				SmallChange = SCROLL_SMALL_CHANGE,
				LargeChange = SCROLL_LARGE_CHANGE,
				Dock = DockStyle.Right
			};

			Controls.Add(vScrollBar1);

			panel1.Resize += (_, _) => AdjustScrollBar();

			if (this.DesignMode)
				return;

			vScrollBar1.Scroll += (_, _) => DoVirtualScroll();

			for (int i = 0; i < NUM_ACTUAL_CONTROLS; i++)
			{
				BookControls[i] = new ProcessBookControl();

				if (i == 0)
				{
					PROCESSBOOKCONTROL_MARGIN = BookControls[i].Margin.Left + BookControls[i].Margin.Right;
					CONTROL_HEIGHT = BookControls[i].Height + BookControls[i].Margin.Top + BookControls[i].Margin.Bottom;
				}

				BookControls[i].Location = new Point(2, CONTROL_HEIGHT * i);
				BookControls[i].Width = panel1.ClientRectangle.Width - PROCESSBOOKCONTROL_MARGIN;
				BookControls[i].Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
				panel1.Controls.Add(BookControls[i]);
			}

			panel1.Height += SCROLL_SMALL_CHANGE;
		}


		private void AdjustScrollBar()
		{
			int maxFullVisible = DisplayRectangle.Height / CONTROL_HEIGHT;

			if (VirtualControlCount <= maxFullVisible)
			{
				vScrollBar1.Enabled = false;

				for (int i = VirtualControlCount; i < NUM_ACTUAL_CONTROLS; i++)
					BookControls[i].Visible = false;
			}
			else
			{
				vScrollBar1.Enabled = true;
				//https://stackoverflow.com/a/2882878/3335599
				vScrollBar1.Maximum = VirtualHeight + vScrollBar1.LargeChange - 1;
			}
		}

		private void DoVirtualScroll()
		{
			//https://stackoverflow.com/a/2882878/3335599
			int scrollValue = Math.Max(Math.Min(VirtualHeight, vScrollBar1.Value), 0);

			int position = scrollValue % CONTROL_HEIGHT;
			panel1.Location = new Point(0, -position);

			int firstVisible = scrollValue / CONTROL_HEIGHT;

			int window = DisplayRectangle.Height;

			int count = window / CONTROL_HEIGHT;

			if (window % CONTROL_HEIGHT != 0)
				count++;
			count = Math.Min(count, VirtualControlCount);

			RequestData?.Invoke(firstVisible, count, BookControls);

			for (int i = 0; i < BookControls.Length; i++)
				BookControls[i].Visible = i <= count && VirtualControlCount > 0;
		}


		protected override void WndProc(ref Message m)
		{
			if (m.Msg == WM_MOUSEWHEEL)
			{
				//https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-mousewheel
				int wheelDelta = -(short)(((ulong)m.WParam) >> 16 & 0xffff);

				if (wheelDelta > 0)
					vScrollBar1.Value = Math.Min(vScrollBar1.Value + wheelDelta, vScrollBar1.Maximum);
				else
					vScrollBar1.Value = Math.Max(vScrollBar1.Value + wheelDelta, vScrollBar1.Minimum);

				DoVirtualScroll();
			}

			base.WndProc(ref m);
		}
	}
}
