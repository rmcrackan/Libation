using System;
using System.Collections;

namespace LibationWinForms
{
	interface IObjectMemberComparable
	{
		IComparer GetMemberComparer(Type memberType);
		object GetMemberValue(string memberName);
	}
}
