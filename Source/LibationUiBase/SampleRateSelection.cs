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
