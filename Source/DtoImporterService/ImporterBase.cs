using System;
using System.Collections.Generic;
using System.Linq;
using AudibleUtilities;
using DataLayer;
using Dinah.Core;

namespace DtoImporterService
{
	public abstract class ImporterBase<TValidate> where TValidate : IValidator
	{
		protected LibationContext DbContext { get; }

		protected ImporterBase(LibationContext context)
		{
			ArgumentValidator.EnsureNotNull(context, nameof(context));
			DbContext = context;
		}

		/// <summary>LONG RUNNING. call with await Task.Run</summary>
		public int Import(IEnumerable<ImportItem> param) => Run(DoImport, param);

		public TResult Run<TResult>(Func<IEnumerable<ImportItem>, TResult> func, IEnumerable<ImportItem> param)
		{
			try
			{
				var exceptions = TValidate.Validate(param.Select(i => i.DtoItem));
				if (exceptions is not null && exceptions.Any())
					throw new AggregateException($"Importer validation failed", exceptions);
			}
			catch (Exception ex)
			{
				Serilog.Log.Logger.Error(ex, "Import error: validation");
				throw;
			}

			try
			{
				var result = func(param);
				return result;
			}
			catch (Exception ex)
			{
				Serilog.Log.Logger.Error(ex, "Import error: post-validation importing");
				throw;
			}
		}

		protected abstract int DoImport(IEnumerable<ImportItem> elements);
	}
}
