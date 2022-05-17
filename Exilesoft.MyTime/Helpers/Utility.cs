// -------------DEVELOPER COMMENT---------------------------//
//
// Filename     : Utility.cs
// Created By   : Harinda Dias
// Date         : 2013-Jul-11, Thu
// Description  : Helper class for the utitlity actions

//
// Modified By  : 
// Date         : 
// Purpose      : 
//
// ---------------------------------------------------------//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Linq.Expressions;
using System.Globalization;

namespace Exilesoft.MyTime.Helpers
{
    public static class Utility
    {
        /// <summary>
        /// Parse value to integer. Will return null if not an integer
        /// </summary>
        /// <param name="val">Parse String</param>
        /// <returns>Converted Value</returns>
        public static int? ParseInt(string val)
        {
            int convertuedValue;
            bool res = int.TryParse(val, out convertuedValue);
            if (res == false)
                return null;
            else
                return convertuedValue;
        }

        /// <summary>
        /// Parse value to Datetime. Will return null if not a date
        /// </summary>
        /// <param name="val">Parse String</param>
        /// <returns>Converted Value</returns>
        public static DateTime? ParseDate(string val)
        {
            DateTime dateTime;
            if (DateTime.TryParse(val, CultureInfo.GetCultureInfo("en-GB"), DateTimeStyles.None, out dateTime))
                return dateTime;

            return null;
        }

        public static DateTime GetDateTimeNow()
        {
            return DateTime.UtcNow.AddMinutes(330);
        }

        public static MvcHtmlString EnumDropDownList<TEnum>(this HtmlHelper htmlHelper, string name, TEnum selectedValue)
        {
            IEnumerable<TEnum> values = Enum.GetValues(typeof(TEnum))
                .Cast<TEnum>();

            IEnumerable<SelectListItem> items =
                from value in values
                select new SelectListItem
                {
                    Text = value.ToString(),
                    Value = value.ToString(),
                    Selected = (value.Equals(selectedValue))
                };

            return htmlHelper.DropDownList(
                name,
                items, "--- Select ---"
                );
        }

        public static MvcHtmlString EnumDropDownListFor<TModel, TEnum>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TEnum>> expression, object htmlAttributes)
        {
            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
            IEnumerable<TEnum> values = Enum.GetValues(typeof(TEnum)).Cast<TEnum>();

            IEnumerable<SelectListItem> items =
                values.Select(value => new SelectListItem
                {
                    Text = value.ToString(),
                    Value = value.ToString(),
                    Selected = value.Equals(metadata.Model)
                });

            return htmlHelper.DropDownListFor(
                expression,
                items, "--- Select ---", htmlAttributes
                );
        }

        public static string FormatViewString(string s)
        {
            if (s == null)
                return "-";
            else
                if (s.Trim().Equals(string.Empty))
                    return "-";
                else
                    return s;
        }

		public static bool IsUserInRole(string role)
		{
            var user = HttpContext.Current.User;

            return user.IsInRole(role);
		}

		public static int GetUserRoleInt()
		{
			if (!IsUserInRole("Employee") && !IsUserInRole("SystemAdmin"))
				return 1;//loggedUser.Privillage;
			else
				return 0;

		}

        public static string ConvertSecondsToHoursMiniuteSecond(int secs)
        {

            TimeSpan t = TimeSpan.FromSeconds(secs);

            string answer = string.Format("{0:D2}:{1:D2}:{2:D2}",
                            t.Hours,
                            t.Minutes,
                            t.Seconds);

            return answer;


        }

        public static string getTimepartFromADate(DateTime t)
        {

            string answer = string.Format("{0:D2}:{1:D2}:{2:D2}",
                            t.Hour,
                            t.Minute,
                            t.Second);

            return answer;

        }
    }
}