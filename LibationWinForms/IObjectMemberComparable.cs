using System;
using System.Collections;

namespace LibationWinForms
{
	internal interface IObjectMemberComparable
	{
		IComparer GetMemberComparer(Type memberType);
		object GetMemberValue(string memberName);
	}
}
