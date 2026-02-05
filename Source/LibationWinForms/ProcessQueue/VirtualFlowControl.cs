using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Windows.Forms;

namespace LibationWinForms.ProcessQueue;

internal partial class VirtualFlowControl : UserControl
{
	/// <summary>
	/// Triggered when one of the <see cref="ProcessBookControl"/>'s buttons has been clicked
	/// </summary>
	public event EventHandler<string>? ButtonClicked;

	/// <summary>
	/// Gets the index of the first realized element, or -1 if no elements are realized.
	/// </summary>
	public int FirstRealizedIndex { get; private set; } = -1;

	/// <summary>
	/// Gets the index of the last realized element, or -1 if no elements are realized.
	/// </summary>
	public int LastRealizedIndex { get; private set; } = -1;

	public IList? Items { get; private set; }

	private object? m_OldContext;
	protected override void OnDataContextChanged(EventArgs e)
	{
		if (m_OldContext is INotifyCollectionChanged oldNotify)
			oldNotify.CollectionChanged -= Items_CollectionChanged;

		if (DataContext is INotifyCollectionChanged newNotify)
		{
			m_OldContext = newNotify;
			newNotify.CollectionChanged += Items_CollectionChanged;
		}

		Items = DataContext as IList;
		base.OnDataContextChanged(e);
	}

	private void Items_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
	{
		RefreshDisplay();
	}

	public void RefreshDisplay()
	{
		if (InvokeRequired)
		{
			Invoke((MethodInvoker)delegate
			{
				AdjustScrollBar();
				DoVirtualScroll();
			});
		}
		else
		{
			AdjustScrollBar();
			DoVirtualScroll();
		}
	}

	#region Dynamic Properties

	/// <summary>
	/// The number of virtual <see cref="ProcessBookControl"/>s in the <see cref="VirtualFlowControl"/>
	/// </summary>
	public int VirtualControlCount => Items?.Count ?? 0;

	int ScrollValue => Math.Max(vScrollBar1.Value, 0);
	/// <summary>
	/// Amount the control moves with a small scroll change
	/// </summary>
	private int SmallScrollChange => VirtualControlHeight * SMALL_SCROLL_CHANGE_MULTIPLE;
	/// <summary>
	/// Amount the control moves with a large scroll change. Equal to the number of whole <see cref="ProcessBookControl"/>s in the panel, less 1.
	/// </summary>
	private int LargeScrollChange => Math.Max(DisplayHeight / VirtualControlHeight - 1, SMALL_SCROLL_CHANGE_MULTIPLE) * VirtualControlHeight;
	/// <summary>
	/// Virtual height of all virtual controls within this <see cref="VirtualFlowControl"/>
	/// </summary>
	private int VirtualHeight => (VirtualControlCount + NUM_BLANK_SPACES_AT_BOTTOM) * VirtualControlHeight - DisplayHeight + 2 * TopMargin;
	/// <summary>
	/// Index of the first virtual <see cref="ProcessBookControl"/>
	/// </summary>
	private int FirstVisibleVirtualIndex => ScrollValue / VirtualControlHeight;
	/// <summary>
	/// The display height of this <see cref="VirtualFlowControl"/>
	/// </summary>
	private int DisplayHeight => DisplayRectangle.Height;

	#endregion

	#region Instance variables

	/// <summary>
	/// The total height, including margins, of the repeated <see cref="ProcessBookControl"/>
	/// </summary>
	private readonly int VirtualControlHeight;
	/// <summary>
	/// Margin between the top <see cref="ProcessBookControl"/> and the top of the Panel, and the bottom <see cref="ProcessBookControl"/> and the bottom of the panel
	/// </summary>
	private readonly int TopMargin;

	private readonly List<ProcessBookControl> BookControls = new();

	#endregion

	#region Global behavior settings

	/// <summary>
	/// Total number of actual controls added to the panel. 23 is sufficient up to a 4k monitor height.
	/// </summary>
	private const int NUM_ACTUAL_CONTROLS = 23;
	/// <summary>
	/// Multiple of <see cref="VirtualControlHeight"/> that is moved for each small scroll change
	/// </summary>
	private const int SMALL_SCROLL_CHANGE_MULTIPLE = 1;
	/// <summary>
	/// Amount of space at the bottom of the <see cref="VirtualFlowControl"/>, in multiples of <see cref="VirtualControlHeight"/>
	/// </summary>
	private const int NUM_BLANK_SPACES_AT_BOTTOM = 2;

	#endregion

	public VirtualFlowControl()
	{
		InitializeComponent();

		panel1.Resize += (_, _) => RefreshDisplay();

		var control = InitControl(0);
		VirtualControlHeight = this.DpiUnscale(control.Height + control.Margin.Top + control.Margin.Bottom);
		TopMargin = control.Margin.Top;

		BookControls.Add(control);
		panel1.Controls.Add(control);

		for (int i = 1; i < NUM_ACTUAL_CONTROLS; i++)
		{
			control = InitControl(VirtualControlHeight * i);
			BookControls.Add(control);
			panel1.Controls.Add(control);
		}

		vScrollBar1.Scroll += (_, s) => SetScrollPosition(s.NewValue);
		vScrollBar1.SmallChange = SmallScrollChange;
		panel1.Height += this.DpiScale(NUM_BLANK_SPACES_AT_BOTTOM * VirtualControlHeight);
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
	/// Handles all button clicks from all <see cref="ProcessBookControl"/>, detects which one sent the click, and fires <see cref="ButtonClicked"/> to notify the model of the click
	/// </summary>
	private void ControlButton_Click(object? sender, EventArgs e)
	{
		Control? button = sender as Control;
		Control? form = button?.Parent;
		while (form is not null and not ProcessBookControl)
			form = form?.Parent;

		if (form is not null && button?.Name is string buttonText)
			ButtonClicked?.Invoke(form, buttonText);
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
			vScrollBar1.Value = 0;

			for (int i = VirtualControlCount; i < NUM_ACTUAL_CONTROLS; i++)
				BookControls[i].Visible = false;
		}
		else
		{
			vScrollBar1.Enabled = true;

			//https://stackoverflow.com/a/2882878/3335599
			int newMaximum = VirtualHeight + LargeScrollChange - 1;
			if (newMaximum < vScrollBar1.Maximum)
				vScrollBar1.Value = Math.Max(vScrollBar1.Value - (vScrollBar1.Maximum - newMaximum), 0);
			vScrollBar1.Maximum = newMaximum;
			vScrollBar1.LargeChange = LargeScrollChange;
		}
	}

	/// <summary>
	/// Scrolls the specified item into view.
	/// </summary>
	/// <param name="index">The index of the item.</param>
	public void ScrollIntoView(int index)
	{
		if (index < 0 || index >= VirtualControlCount)
			throw new ArgumentOutOfRangeException(nameof(index));
		int firstVisible = FirstVisibleVirtualIndex;
		int lastVisible = firstVisible + (DisplayHeight / VirtualControlHeight) - 1;
		if (index < firstVisible)
		{
			SetScrollPosition(index * VirtualControlHeight);
		}
		else if (index > lastVisible)
		{
			int newScrollPos = (index - (lastVisible - firstVisible)) * VirtualControlHeight;
			SetScrollPosition(newScrollPos);
		}
	}

	/// <summary>
	/// Calculated the virtual controls that are in view at the current scroll position and windows size,
	/// positions <see cref="panel1"/> to simulate scroll activity, then fires updates the controls with
	/// the context corresponding to the virtual scroll position
	/// </summary>
	private void DoVirtualScroll()
	{
		int firstVisible = FirstVisibleVirtualIndex;

		int position = ScrollValue % VirtualControlHeight;
		panel1.Location = new Point(0, -position);

		int numVisible = DisplayHeight / VirtualControlHeight;

		if (DisplayHeight % VirtualControlHeight != 0)
			numVisible++;

		numVisible = Math.Min(numVisible, VirtualControlCount);
		numVisible = Math.Min(numVisible, VirtualControlCount - firstVisible);

		if (Items is IList items)
		{
			for (int i = 0; i < numVisible; i++)
				BookControls[i].DataContext = items[firstVisible + i];
		}

		for (int i = 0; i < BookControls.Count; i++)
		{
			BookControls[i].Visible = i < numVisible;
		}

		FirstRealizedIndex = firstVisible;
		LastRealizedIndex = firstVisible + numVisible - 1;
	}

	/// <summary>
	/// Set scroll value to an integral multiple of <see cref="SmallScrollChange"/>
	/// </summary>
	private void SetScrollPosition(int value)
	{
		if (!vScrollBar1.Enabled) return;

		int newPos = (int)Math.Round((double)value / SmallScrollChange) * SmallScrollChange;
		if (vScrollBar1.Value != newPos)
		{
			//https://stackoverflow.com/a/2882878/3335599
			vScrollBar1.Value = Math.Min(newPos, vScrollBar1.Maximum - vScrollBar1.LargeChange + 1);
			DoVirtualScroll();
		}
	}


	private const int WM_MOUSEWHEEL = 522;
	private const int WHEEL_DELTA = 120;
	protected override void WndProc(ref Message m)
	{
		//Capture mouse wheel movement and interpret it as a scroll event
		if (m.Msg == WM_MOUSEWHEEL)
		{
			//https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-mousewheel
			int wheelDelta = -(short)(((ulong)m.WParam) >> 16 & ushort.MaxValue);

			int numSmallPositionMoves = Math.Abs(wheelDelta) / WHEEL_DELTA;

			int scrollDelta = Math.Sign(wheelDelta) * numSmallPositionMoves * SmallScrollChange;

			int newScrollPosition;

			if (scrollDelta > 0)
				newScrollPosition = Math.Min(vScrollBar1.Value + scrollDelta, vScrollBar1.Maximum);
			else
				newScrollPosition = Math.Max(vScrollBar1.Value + scrollDelta, vScrollBar1.Minimum);

			SetScrollPosition(newScrollPosition);
		}

		base.WndProc(ref m);
	}
}
