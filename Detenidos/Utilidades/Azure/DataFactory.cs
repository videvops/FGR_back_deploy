using System;
using System.Linq;
using Microsoft.Rest;
using Microsoft.Azure.Management.ResourceManager;
using Microsoft.Azure.Management.DataFactory;
using Microsoft.Azure.Management.DataFactory.Models;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Extensions.Configuration;
using Detenidos.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using Detenidos.Utilidades.Helpers;

namespace Detenidos.Utilidades
{
	public class DataFactory
	{
		private readonly IConfiguration _configuration;
		private readonly ApplicationDbContext _context;

		public DataFactory(IConfiguration configuration, ApplicationDbContext context)
		{
			_configuration = configuration;
			_context = context;
		}


	}
}
