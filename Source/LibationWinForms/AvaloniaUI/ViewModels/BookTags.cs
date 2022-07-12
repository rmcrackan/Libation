namespace LibationWinForms.AvaloniaUI.ViewModels
{
	public class BookTags
	{
		public string Tags { get; init; }
		public bool IsSeries { get; init; }
		public bool HasTags => !string.IsNullOrEmpty(Tags);
	}
}
