namespace Nemesis.Essentials.Design;

public static class DateTimeExtensions
{
    #region Rounding
    //DateTime RoundUp2(DateTime dt, TimeSpan d) => new DateTime(((dt.Ticks + d.Ticks - 1) / d.Ticks) * d.Ticks);

    public static DateTime RoundUp(this DateTime date, TimeSpan d)
    {
        var modTicks = date.Ticks % d.Ticks;
        var delta = modTicks != 0 ? d.Ticks - modTicks : 0;
        return new DateTime(date.Ticks + delta, date.Kind);
    }

    public static DateTime RoundDown(this DateTime date, TimeSpan d)
    {
        var delta = date.Ticks % d.Ticks;
        return new DateTime(date.Ticks - delta, date.Kind);
    }

    public static DateTime RoundToNearest(this DateTime date, TimeSpan d)
    {
        var delta = date.Ticks % d.Ticks;
        bool roundUp = delta > d.Ticks / 2;
        var offset = roundUp ? d.Ticks : 0;

        return new DateTime(date.Ticks + offset - delta, date.Kind);
    }

    public static TimeSpan RoundUp(this TimeSpan date, TimeSpan d)
    {
        var modTicks = date.Ticks % d.Ticks;
        var delta = modTicks != 0 ? d.Ticks - modTicks : 0;
        return TimeSpan.FromTicks(date.Ticks + delta);
    }

    public static TimeSpan RoundDown(this TimeSpan span, TimeSpan d)
    {
        long delta = span.Ticks % d.Ticks;
        return TimeSpan.FromTicks(span.Ticks - delta);
    }

    public static TimeSpan RoundToNearest(this TimeSpan date, TimeSpan d)
    {
        var delta = date.Ticks % d.Ticks;
        bool roundUp = delta > d.Ticks / 2;
        var offset = roundUp ? d.Ticks : 0;

        return TimeSpan.FromTicks(date.Ticks + offset - delta);
    }

    #endregion

    #region Time operations

    public static TimeSpan Multiply(this TimeSpan multiplicand, long multiplier) =>
        TimeSpan.FromTicks(multiplicand.Ticks * multiplier);

    public static TimeSpan Multiply(this TimeSpan multiplicand, double multiplier) =>
        TimeSpan.FromTicks((long)(multiplicand.Ticks * multiplier));

    public static TimeSpan Divide(this TimeSpan dividend, long divisor) =>
        TimeSpan.FromTicks((long)(dividend.Ticks / (double)divisor));

    public static TimeSpan Divide(this TimeSpan dividend, double divisor) =>
        TimeSpan.FromTicks((long)(dividend.Ticks / divisor));

    public static double Divide(this TimeSpan dividend, TimeSpan divisor) =>
        dividend.Ticks / (double)divisor.Ticks;

    public static TimeSpan Modulo(this TimeSpan dividend, TimeSpan divisor) =>
        TimeSpan.FromTicks(dividend.Ticks % divisor.Ticks);

    #endregion

    #region Kind

    public static DateTime MakeLocal(this DateTime date) =>
        date.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(date, DateTimeKind.Local)
            : date.ToLocalTime();

    public static DateTime SetLocalIfUnspecified(this DateTime date) =>
        date.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(date, DateTimeKind.Local)
            : date;

    #endregion
}
