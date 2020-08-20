using System;
using System.Collections.Generic;
using System.Linq;
using AudibleApiDTOs;
using DataLayer;
using Dinah.Core;
using InternalUtilities;

namespace DtoImporterService
{
	public abstract class ImporterBase<T>
	{
		protected LibationContext DbContext { get; }
		protected Account Account { get; }

		protected ImporterBase(LibationContext context, Account account)
		{
			ArgumentValidator.EnsureNotNull(context, nameof(context));
			ArgumentValidator.EnsureNotNull(account, nameof(account));

			DbContext = context;
			Account = account;
		}

		/// <summary>LONG RUNNING. call with await Task.Run</summary>
		public int Import(T param) => Run(DoImport, param);

		public TResult Run<TResult>(Func<T, TResult> func, T param)
		{
			try
			{
				var exceptions = Validate(param);
				if (exceptions != null && exceptions.Any())
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

	public abstract class ItemsImporterBase : ImporterBase<IEnumerable<Item>>
	{
		protected ItemsImporterBase(LibationContext context, Account account) : base(context, account) { }
	}
}
