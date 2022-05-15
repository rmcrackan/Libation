using ApplicationServices;
using LibationFileManager;
using LibationWinForms.ProcessQueue;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LibationWinForms
{
	public partial class Form1
	{
		private void Configure_ProcessQueue()
		{
			productsGrid.LiberateClicked += (_, lb) => processBookQueue1.AddDownloadDecrypt(lb);
			processBookQueue1.popoutBtn.Click += ProcessBookQueue1_PopOut;

			Task.Run(() =>
			{
				Task.Delay(3000).Wait();
				var lb = ApplicationServices.DbContexts.GetLibrary_Flat_NoTracking().Select(lb => lb.Book).ToList();

				foreach (var b in lb)
				{
					b.UserDefinedItem.BookStatus = DataLayer.LiberatedStatus.NotLiberated;
				}
				LibraryCommands.UpdateUserDefinedItem(lb);

			});
			
		}

		int WidthChange = 0;
		private void HideQueueBtn_Click(object sender, EventArgs e)
		{
			if (splitContainer1.Panel2Collapsed)
			{
				WidthChange = WidthChange == 0 ? splitContainer1.Panel2.Width + splitContainer1.SplitterWidth : WidthChange;
				Width += WidthChange;
				splitContainer1.Panel2.Controls.Add(processBookQueue1);
				splitContainer1.Panel2Collapsed = false;
				processBookQueue1.popoutBtn.Visible = true;
				hideQueueBtn.Text = "❰❰❰";
			}
			else
			{
				WidthChange = splitContainer1.Panel2.Width + splitContainer1.SplitterWidth;
				splitContainer1.Panel2.Controls.Remove(processBookQueue1);
				splitContainer1.Panel2Collapsed = true;
				Width -= WidthChange;
				hideQueueBtn.Text = "❱❱❱";
			}
		}

		private void ProcessBookQueue1_PopOut(object sender, EventArgs e)
		{
			ProcessBookForm dockForm = new();
			dockForm.WidthChange = splitContainer1.Panel2.Width + splitContainer1.SplitterWidth;
			dockForm.RestoreSizeAndLocation(Configuration.Instance);
			dockForm.FormClosing += DockForm_FormClosing;
			splitContainer1.Panel2.Controls.Remove(processBookQueue1);
			splitContainer1.Panel2Collapsed = true;
			processBookQueue1.popoutBtn.Visible = false;
			dockForm.PassControl(processBookQueue1);
			dockForm.Show();
			this.Width -= dockForm.WidthChange;
			hideQueueBtn.Visible = false;
			int deltax = filterBtn.Margin.Right + hideQueueBtn.Width + hideQueueBtn.Margin.Left;
			filterBtn.Location= new System.Drawing.Point(filterBtn.Location.X + deltax, filterBtn.Location.Y);
			filterSearchTb.Location = new System.Drawing.Point(filterSearchTb.Location.X + deltax, filterSearchTb.Location.Y);
		}

		private void DockForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (sender is ProcessBookForm dockForm)
			{
				this.Width += dockForm.WidthChange;
				splitContainer1.Panel2.Controls.Add(dockForm.RegainControl());
				splitContainer1.Panel2Collapsed = false;
				processBookQueue1.popoutBtn.Visible = true;
				dockForm.SaveSizeAndLocation(Configuration.Instance);
				this.Focus();
				hideQueueBtn.Visible = true;
				int deltax = filterBtn.Margin.Right + hideQueueBtn.Width + hideQueueBtn.Margin.Left;
				filterBtn.Location = new System.Drawing.Point(filterBtn.Location.X - deltax, filterBtn.Location.Y);
				filterSearchTb.Location = new System.Drawing.Point(filterSearchTb.Location.X - deltax, filterSearchTb.Location.Y);
			}
		}
	}
}
