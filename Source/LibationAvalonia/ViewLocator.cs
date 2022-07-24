using Avalonia.Controls;
using Avalonia.Controls.Templates;
using LibationAvalonia.ViewModels;
using System;

namespace LibationAvalonia
{
	public class ViewLocator : IDataTemplate
	{
		public IControl Build(object data)
		{
			var name = data.GetType().FullName!.Replace("ViewModel", "View");
			var type = Type.GetType(name);

			if (type != null)
			{
				return (Control)Activator.CreateInstance(type)!;
			}
			else
			{
				return new TextBlock { Text = "Not Found: " + name };
			}
		}

		public bool Match(object data)
		{
			return data is ViewModelBase;
		}
	}
}
