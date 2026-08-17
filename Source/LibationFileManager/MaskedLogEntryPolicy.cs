using Serilog.Core;
using Serilog.Events;

namespace LibationFileManager;

/// <summary>
/// Implemented by types that identify something private, and so must appear in a log only in masked form.
/// </summary>
public interface ILogMasked
{
	/// <summary>The only form of this object that may reach a log file.</summary>
	string MaskedLogEntry { get; }
}

/// <summary>
/// Reduces any <see cref="ILogMasked"/> to its masked entry when Serilog destructures it, so that a structured
/// log call - <c>{@Account}</c>, or an anonymous <c>{@DebugInfo}</c> object holding one - cannot publish the
/// unmasked object by accident.
/// <para>
/// This covers Serilog's own destructuring only. Serilog.Exceptions flattens a logged exception's properties
/// itself before Serilog sees them, so an exception must not carry one of these in the first place.
/// </para>
/// </summary>
public class MaskedLogEntryPolicy : IDestructuringPolicy
{
	public bool TryDestructure(object value, ILogEventPropertyValueFactory propertyValueFactory, out LogEventPropertyValue result)
	{
		if (value is ILogMasked masked)
		{
			result = new ScalarValue(masked.MaskedLogEntry);
			return true;
		}

		result = null!;
		return false;
	}
}
