using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Web;
using System.Web.Caching;
using Exilesoft.Models;
using Exilesoft.MyTime.Helpers;
using Exilesoft.MyTime.Mappings;
using Exilesoft.MyTime.ViewModels;

namespace Exilesoft.MyTime.Repositories
{
    public class PrivilegesRepository
    {
        private static Context dbContext = new Context();
        private const string PrivilegesCache = "Privileges";

        internal static List<PrivilegeViewModel> GetAllPrivilegeList()
        {
            if (HttpContext.Current.Cache[PrivilegesCache] == null)
            {
                dbContext = new Context();
                HttpContext.Current.Cache.Insert(PrivilegesCache,
                    (new PrivilegeViewModelMapper()).Map(dbContext.Privileges.OrderBy(s => s.Name).ToList()), null,
                    Utility.GetDateTimeNow().AddDays(1), TimeSpan.Zero);
            }

            return (List<PrivilegeViewModel>)HttpContext.Current.Cache[PrivilegesCache];
        }
    }
}