using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibationUiBase
{
	public class SampleRateSelection
	{
		public AAXClean.SampleRate SampleRate { get; }
		public SampleRateSelection(AAXClean.SampleRate sampleRate)
		{
			SampleRate = sampleRate;
		}
		public override string ToString() => $"{(int)SampleRate} Hz";
	}
}
