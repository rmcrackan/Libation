using System;
using System.Collections.Generic;
using System.Linq;
using AudibleUtilities;
using DataLayer;
using Dinah.Core;

namespace DtoImporterService
{
	public abstract class ImporterBase<T>
	{
		protected LibationContext DbContext { get; }

		protected ImporterBase(LibationContext context)
		{
			ArgumentValidator.EnsureNotNull(context, nameof(context));
			DbContext = context;
		}

		/// <summary>LONG RUNNING. call with await Task.Run</summary>
		public int Import(T param) => Run(DoImport, param);

		public TResult Run<TResult>(Func<T, TResult> func, T param)
		{
			try
			{
				var exceptions = Validate(param);
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

		protected abstract int DoImport(T elements);
		public abstract IEnumerable<Exception> Validate(T param);
	}

	public abstract class ItemsImporterBase : ImporterBase<IEnumerable<ImportItem>>
	{
		protected ItemsImporterBase(LibationContext context) : base(context) { }

		protected abstract IValidator Validator { get; }
        public sealed override IEnumerable<Exception> Validate(IEnumerable<ImportItem> importItems)
			=> Validator.Validate(importItems.Select(i => i.DtoItem));
	}
}
