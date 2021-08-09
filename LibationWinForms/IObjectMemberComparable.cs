using System;
using System.Collections;

namespace LibationWinForms
{
	interface IObjectMemberComparable
	{
		IComparer GetComparer(Type propertyType);
		object GetMemberValue(string valueName);
	}
}
