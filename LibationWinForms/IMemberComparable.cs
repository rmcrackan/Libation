using System;
using System.Collections;

namespace LibationWinForms
{
	internal interface IMemberComparable
	{
		IComparer GetMemberComparer(Type memberType);
		object GetMemberValue(string memberName);
	}
}
