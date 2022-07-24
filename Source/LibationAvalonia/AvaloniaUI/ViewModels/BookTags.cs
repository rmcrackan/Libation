namespace LibationAvalonia.AvaloniaUI.ViewModels
{
	public class BookTags
	{
		private string _tags;
		public string Tags { get => _tags; init { _tags = value; HasTags = !string.IsNullOrEmpty(_tags); } }
		public bool HasTags { get; init; }
	}
}
