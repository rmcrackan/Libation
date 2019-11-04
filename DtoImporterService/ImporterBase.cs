using System;
using System.Collections.Generic;
using System.Linq;
using AudibleApiDTOs;
using DataLayer;

namespace DtoImporterService
{
	public interface IContextRunner<T>
	{
		public TResult Run<TResult>(Func<T, LibationContext, TResult> func, T param, LibationContext context = null)
		{
			if (context is null)
			{
				using (context = LibationContext.Create())
				{
					var r = Run(func, param, context);
					context.SaveChanges();
					return r;
				}
			}

			var exceptions = Validate(param);
			if (exceptions != null && exceptions.Any())
				throw new AggregateException($"Device Jobs Service configuration validation failed", exceptions);

			var result = func(param, context);
			return result;
		}
		IEnumerable<Exception> Validate(T param);
	}

	public abstract class ImporterBase<T> : IContextRunner<T>
	{
		/// <summary>LONG RUNNING. call with await Task.Run</summary>
		public int Import(T param, LibationContext context = null)
			=> ((IContextRunner<T>)this).Run(DoImport, param, context);

		protected abstract int DoImport(T elements, LibationContext context);
		public abstract IEnumerable<Exception> Validate(T param);
	}

	public abstract class ItemsImporterBase : ImporterBase<IEnumerable<Item>> { }
}
