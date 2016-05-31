using System;

namespace Dates
{
    /// <summary>
    /// A struct similar to <see cref="TimeSpan" /> that stores the elapsed time between two dates,
    /// but does so in a way that respects the number of actual days in the elapsed years and months.
    /// </summary>
    [Serializable]
    public struct DateSpan
    { 
        /// <param name="start">The start date</param>
        /// <param name="end">The end date</param>
        /// <param name="excludeEndDate">If true, the span is exclusive of the end date</param>
        public DateSpan(DateTime start, DateTime end, bool excludeEndDate = true) : this()
        {
            start = start.ToUniversalTime();
            end = end.ToUniversalTime();

            if (start > end)
            {
                var temp = start;
                start = end;
                end = temp;
            }

            CalculateYears(start, end);
            CalculateMonths(start, end);
            CalculateDays(start, end, excludeEndDate);
            CalculateHours(start, end);
            CalculateMinutes(start, end);
            CalculateSeconds(start, end);
        }

        /// <summary>
        /// The number of discrete years occurring in this span
        /// </summary>
        public int Years { get; private set; }

        /// <summary>
        /// The number of discrete months occurring in this span
        /// </summary>
        public int Months { get; private set; }

        /// <summary>
        /// The number of discrete weeks occurring in this span
        /// </summary>
        public int Weeks { get; private set; }

        /// <summary>
        /// The number of discrete days occurring in this span
        /// </summary>
        public int Days { get; private set; }

        /// <summary>
        /// The number of discrete hours occurring in this span
        /// </summary>
        public int Hours { get; private set; }

        /// <summary>
        /// The number of discrete minutes occurring in this span
        /// </summary>
        public int Minutes { get; private set; }
        
        /// <summary>
        /// The number of discrete seconds occurring in this span
        /// </summary>
        public int Seconds { get; private set; }

        /// <summary>
        /// Gets the scalar difference between two dates given a <see cref="DateInterval"/> value.
        /// </summary>
        /// <param name="interval">The interval to calculate</param>
        /// <param name="start">The start date</param>
        /// <param name="end">The end date</param>
        /// <param name="excludeEndDate">If true, the difference is exclusive of the end date</param>
        /// <returns></returns>
        public static int GetDifference(DateInterval interval, DateTime start, DateTime end, bool excludeEndDate = false)
        {
            var sum = 0;
            var span = new DateSpan(start, end);

            switch (interval)
            {
                case DateInterval.Years:
                    sum += span.Years;
                    break;
                case DateInterval.Months:
                    if (span.Years > 0)
                    {
                        sum += span.Years * 12;
                    }
                    sum += span.Months;
                    sum += span.Weeks / 4; // Helps resolve lower resolution
                    break;
                case DateInterval.Weeks:
                    sum = GetDifferenceInDays(start, span, excludeEndDate) / 7;
                    break;
                case DateInterval.Days:
                    sum = GetDifferenceInDays(start, span, excludeEndDate);
                    break;
                case DateInterval.Hours:
                    sum = GetDifferenceInDays(start, span, excludeEndDate) * 24;
                    sum += span.Hours;
                    break;
                case DateInterval.Minutes:
                    sum = GetDifferenceInDays(start, span, excludeEndDate) * 24 * 60;
                    sum += span.Hours * 60;
                    sum += span.Minutes;
                    break;
                case DateInterval.Seconds:
                    sum = GetDifferenceInDays(start, span, excludeEndDate) * 24 * 60 * 60;
                    sum += span.Hours * 60 * 60;
                    sum += span.Minutes * 60;
                    sum += span.Seconds;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("interval");
            }

            return sum;
        }

        private static int GetDifferenceInDays(DateTime start, DateSpan span, bool excludeEndDate = true)
        {
            var sum = 0;

            if (span.Years > 0)
            {
                for (var i = 0; i < span.Years; i++)
                {
                    var year = start.Year + i;
                    sum += DateTime.IsLeapYear(year) ? 366 : 365;
                }
            }

            if (span.Months > 0)
            {
                for (var i = 1; i <= span.Months; i++)
                {
                    var month = start.Month + i;
                    if (month >= 13)
                    {
                        month -= 12;
                    }
                    sum += DateTime.DaysInMonth(start.Year + span.Years, month);
                }
            }

            sum += span.Weeks*7;

            sum += span.Days;
            
            if(excludeEndDate)
            {
                sum--;
            }

            return sum;
        }

        private void CalculateSeconds(DateTime start, DateTime end)
        {
            Seconds = end.Second - start.Second;

            if (end.Second < start.Second)
            {
                Seconds = 60 - start.Second + end.Second;
            }
        }

        private void CalculateMinutes(DateTime start, DateTime end)
        {
            Minutes = end.Minute - start.Minute;

            if (end.Minute < start.Minute)
            {
                Minutes = 60 - start.Minute + end.Minute;
            }

            if (Minutes <= 0)
            {
                return;
            }

            if (end.Second < start.Second)
            {
                Minutes--;
            }
        }

        private void CalculateHours(DateTime start, DateTime end)
        {
            Hours = end.Hour - start.Hour;

            if (end.Hour < start.Hour)
            {
                Hours = 24 - start.Hour + end.Hour;
            }

            if (Hours <= 0)
            {
                return;
            }

            if (end.Minute >= start.Minute)
            {
                if (end.Minute != start.Minute ||
                    end.Second >= start.Second)
                {
                    return;
                }

                Hours--;
            }
            else
            {
                Hours--;
            }
        }

        private void CalculateDays(DateTime start, DateTime end, bool excludeEndDate = false)
        {
            Days = end.Day - start.Day;

            if (end.Day < start.Day)
            {
                Days = DateTime.DaysInMonth(start.Year, start.Month) - start.Day + end.Day;
            }

            if (Days <= 0)
            {
                return;
            }

            if (end.Hour < start.Hour)
            {
                Days--;
            }
            else if (end.Hour == start.Hour)
            {
                if (end.Minute >= start.Minute)
                {
                    if (end.Minute == start.Minute && end.Second < start.Second)
                    {
                        Days--;
                    }
                }
                else
                {
                    Days--;
                }
            }

            Weeks = Days/7;

            Days = Days % 7;

            if(!excludeEndDate)
            {
                Days++;
            }
        }

        private void CalculateMonths(DateTime start, DateTime end)
        {
            Months = end.Month - start.Month;

            if (end.Month < start.Month || (end.Month <= start.Month && Years > 1))
            {
                Months = 12 - start.Month + end.Month;
            }

            if (Months <= 0)
            {
                return;
            }

            if (end.Day < start.Day)
            {
                Months--;
            }
            else if (end.Day == start.Day)
            {
                if (end.Hour < start.Hour)
                {
                    Months--;
                }
                else if (end.Hour == start.Hour)
                {
                    if (end.Minute >= start.Minute)
                    {
                        if (end.Minute != start.Minute || end.Second >= start.Second)
                        {
                            return;
                        }

                        Months--;
                    }
                    else
                    {
                        Months--;
                    }
                }
            }
        }

        private void CalculateYears(DateTime start, DateTime end)
        {
            Years = end.Year - start.Year;

            if (Years <= 0)
            {
                return;
            }

            if (end.Month < start.Month)
            {
                Years--;
            }
            else if (end.Month == start.Month)
            {
                if (end.Day >= start.Day)
                {
                    if (end.Day != start.Day)
                    {
                        return;
                    }

                    if (end.Hour < start.Hour)
                    {
                        Years--;
                    }

                    else if (end.Hour == start.Hour)
                    {
                        if (end.Minute >= start.Minute)
                        {
                            if (end.Minute != start.Minute)
                            {
                                return;
                            }

                            if (end.Second >= start.Second)
                            {
                                return;
                            }

                            Years--;
                        }
                        else
                        {
                            Years--;
                        }
                    }
                }
                else
                {
                    Years--;
                }
            }
        }
    }
}