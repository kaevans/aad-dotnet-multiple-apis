﻿using System.Web;
using System.Web.Mvc;

namespace aad_dotnet_webapi_onbehalfof
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
