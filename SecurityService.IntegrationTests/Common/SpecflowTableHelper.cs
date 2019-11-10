namespace SecurityService.IntergrationTests.Common
{
    using System;
    using TechTalk.SpecFlow;

    public static class SpecflowTableHelper
    {
        #region Methods

        public static Decimal GetDecimalValue(TableRow row,
                                              String key)
        {
            String field = SpecflowTableHelper.GetStringRowValue(row, key);

            return Decimal.TryParse(field, out Decimal value) ? value : -1;
        }

        public static Boolean GetBooleanValue(TableRow row,
                                              String key)
        {
            String field = SpecflowTableHelper.GetStringRowValue(row, key);

            return Boolean.TryParse(field, out Boolean value) && value;
        }

        public static Int32 GetIntValue(TableRow row,
                                        String key)
        {
            String field = SpecflowTableHelper.GetStringRowValue(row, key);

            return Int32.TryParse(field, out Int32 value) ? value : -1;
        }

        public static Int16 GetShortValue(TableRow row,
                                          String key)
        {
            String field = SpecflowTableHelper.GetStringRowValue(row, key);

            if (Int16.TryParse(field, out Int16 value))
            {
                return value;
            }
            else
            {
                return -1;
            }
        }

        public static String GetStringRowValue(TableRow row,
                                               String key)
        {
            return row.TryGetValue(key, out String value) ? value : "";
        }

        /// <summary>
        /// Gets the date for date string.
        /// </summary>
        /// <param name="dateString">The date string.</param>
        /// <param name="today">The today.</param>
        /// <returns></returns>
        public static DateTime GetDateForDateString(String dateString,
                                                    DateTime today)
        {
            switch (dateString.ToUpper())
            {
                case "TODAY":
                    return today.Date;
                case "YESTERDAY":
                    return today.AddDays(-1).Date;
                case "LASTWEEK":
                    return today.AddDays(-7).Date;
                case "LASTMONTH":
                    return today.AddMonths(-1).Date;
                case "LASTYEAR":
                    return today.AddYears(-1).Date;
                case "TOMORROW":
                    return today.AddDays(1).Date;
                default:
                    return DateTime.Parse(dateString);
            }
        }

        #endregion
    }
}