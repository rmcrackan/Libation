using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace LibationWinForms.ProcessQueue
{

	internal delegate void RequestDataDelegate(int firstIndex, int numVisible, IReadOnlyList<ProcessBookControl> panelsToFill);
	internal delegate void ControlButtonClickedDelegate(int itemIndex, string buttonName, ProcessBookControl panelClicked);
	internal partial class VirtualFlowControl : UserControl
	{
		/// <summary>
		/// Triggered when the <see cref="VirtualFlowControl"/> needs to update the displayed <see cref="ProcessBookControl"/>s
		/// </summary>
		public event RequestDataDelegate RequestData;
		/// <summary>
		/// Triggered when one of the <see cref="ProcessBookControl"/>'s buttons has been clicked
		/// </summary>
		public event ControlButtonClickedDelegate ButtonClicked;

		/// <summary>
		/// The number of virtual <see cref="ProcessBookControl"/>s in the <see cref="VirtualFlowControl"/>
		/// </summary>
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

		//https://stackoverflow.com/a/2882878/3335599
		int ScrollValue => Math.Max(Math.Min(VirtualHeight, vScrollBar1.Value), 0);

		/// <summary>
		/// The virtual height of all virtual controls within this <see cref="VirtualFlowControl"/>
		/// </summary>
		private int VirtualHeight => _virtualControlCount * VirtualControlHeight - vScrollBar1.Height + 2 * TopMargin;

		/// <summary>
		/// Item index of the first virtual <see cref="ProcessBookControl"/>
		/// </summary>
		private int FirstVisibleVirtualIndex => ScrollValue / VirtualControlHeight;

		/// <summary>
		/// The display height of this <see cref="VirtualFlowControl"/>
		/// </summary>
		private int DisplayHeight => DisplayRectangle.Height;

		/// <summary>
		/// The total height, inclusing margins, of the repeated <see cref="ProcessBookControl"/>
		/// </summary>
		private readonly int VirtualControlHeight; //90
		private readonly int TopMargin;

		private const int WM_MOUSEWHEEL = 522;
		/// <summary>
		/// Total number of actual controls added to the panel. 23 is sufficient up to a 4k monitor height.
		/// </summary>
		private const int NUM_ACTUAL_CONTROLS = 23;
		private const int SCROLL_SMALL_CHANGE = 120;
		private const int SCROLL_LARGE_CHANGE = 3 * SCROLL_SMALL_CHANGE;

		private readonly VScrollBar vScrollBar1;
		private readonly List<ProcessBookControl> BookControls = new();

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
			panel1.Width -= vScrollBar1.Width;

			Controls.Add(vScrollBar1);

			panel1.Resize += (_, _) =>
			{
				AdjustScrollBar();
				DoVirtualScroll();
			};

			vScrollBar1.Scroll += (_, s) => SetScrollPosition(s.NewValue);

			var control = InitControl(0);
			BookControls.Add(control);
			panel1.Controls.Add(control);
			VirtualControlHeight = control.Height + control.Margin.Top + control.Margin.Bottom;
			TopMargin = control.Margin.Top;

			if (DesignMode)
				return;

			for (int i = 1; i < NUM_ACTUAL_CONTROLS; i++)
			{
				control = InitControl(VirtualControlHeight * i);
				BookControls.Add(control);
				panel1.Controls.Add(control);
			}

			vScrollBar1.SmallChange = VirtualControlHeight;
			vScrollBar1.LargeChange = 3 * VirtualControlHeight;

			panel1.Height += 2 * VirtualControlHeight;
		}

		private ProcessBookControl InitControl(int locationY)
		{
			var control = new ProcessBookControl();
			control.Location = new Point(control.Margin.Left, locationY + control.Margin.Top);
			control.Width = panel1.ClientRectangle.Width - control.Margin.Left - control.Margin.Right;
			control.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;

			control.cancelBtn.Click += ControlButton_Click;
			control.moveFirstBtn.Click += ControlButton_Click;
			control.moveUpBtn.Click += ControlButton_Click;
			control.moveDownBtn.Click += ControlButton_Click;
			control.moveLastBtn.Click += ControlButton_Click;
			return control;
		}

		/// <summary>
		/// Handles all button clicks from all <see cref="ProcessBookControl"/>, detects which one sent the click, and fires <see cref="ButtonClicked"/>
		/// </summary>
		private void ControlButton_Click(object sender, EventArgs e)
		{
			Control button = sender as Control;
			Control form = button.Parent;
			while (form is not ProcessBookControl)
				form = form.Parent;

			int clickedIndex = BookControls.IndexOf((ProcessBookControl)form);

			ButtonClicked?.Invoke(FirstVisibleVirtualIndex + clickedIndex, button.Name, BookControls[clickedIndex]);
		}

		/// <summary>
		/// Adjusts the <see cref="vScrollBar1"/> max width and enabled status based on the <see cref="VirtualControlCount"/> and the <see cref="DisplayHeight"/>
		/// </summary>
		private void AdjustScrollBar()
		{
			int maxFullVisible = DisplayHeight / VirtualControlHeight;

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

		/// <summary>
		/// Calculated the virtual controls that are in view at the currrent scroll position and windows size, then fires <see cref="RequestData"/> for the visible controls
		/// </summary>
		private void DoVirtualScroll()
		{
			int firstVisible = FirstVisibleVirtualIndex;

			int position = ScrollValue % VirtualControlHeight;
			panel1.Location = new Point(0, -position);

			int visibleHeight = ScrollValue - firstVisible * VirtualControlHeight + DisplayHeight;

			int count = visibleHeight / VirtualControlHeight;

			if (visibleHeight % VirtualControlHeight != 0)
				count++;

			count = Math.Min(count, VirtualControlCount);

			RequestData?.Invoke(firstVisible, count, BookControls);

			for (int i = 0; i < BookControls.Count; i++)
				BookControls[i].Visible = i < count;
		}

		/// <summary>
		/// Set scroll value to an integral multiple of VirtualControlHeight
		/// </summary>
		private void SetScrollPosition(int value)
		{
			int newPos = (int)Math.Round((double)value / VirtualControlHeight) * VirtualControlHeight;
			if (vScrollBar1.Value != newPos)
			{
				vScrollBar1.Value = Math.Min(newPos, vScrollBar1.Maximum);
				DoVirtualScroll();
			}
		}

		protected override void WndProc(ref Message m)
		{
			//Capture mouse wheel movement and interpret it as a scroll event
			if (m.Msg == WM_MOUSEWHEEL)
			{
				//https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-mousewheel
				int wheelDelta = -(short)(((ulong)m.WParam) >> 16 & ushort.MaxValue);

				int newScrollPosition;

				if (wheelDelta > 0)
					newScrollPosition = Math.Min(vScrollBar1.Value + wheelDelta, vScrollBar1.Maximum);
				else
					newScrollPosition = Math.Max(vScrollBar1.Value + wheelDelta, vScrollBar1.Minimum);

				SetScrollPosition(newScrollPosition);
			}

			base.WndProc(ref m);
		}
	}
}
