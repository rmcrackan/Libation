using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;

namespace WinFormsDesigner
{
    internal class GridEntry
    {
        [Browsable(false)]
		public string Tags { get; set; }
        [Browsable(false)]
        public IEnumerable<string> TagsEnumerated { get; set; }

		[Browsable(false)]
		public string Download_Status { get; set; }

		public Image Cover { get; set; }
		public string Title { get; set; }
		public string Authors { get; set; }
		public string Narrators { get; set; }
		public int Length { get; set; }
		public string Series { get; set; }
		public string Description { get; set; }
		public string Category { get; set; }
		public string Product_Rating { get; set; }
		public DateTime? Purchase_Date { get; set; }
		public string My_Rating { get; set; }
        public string Misc { get; set; }
	}
}
