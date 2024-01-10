using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Detenidos.Utilidades.Filters
{
	public class ExceptionFilter : ExceptionFilterAttribute
	{
		private readonly ILogger<ExceptionFilter> logger;
		//readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public ExceptionFilter(ILogger<ExceptionFilter> logger)
		{

			this.logger = logger;
		}

		public override void OnException(ExceptionContext context)
		{
			logger.LogError(context.Exception, context.Exception.Message);
			base.OnException(context);
		}
	}
}
