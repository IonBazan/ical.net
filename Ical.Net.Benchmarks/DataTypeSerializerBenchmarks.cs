//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using BenchmarkDotNet.Attributes;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using Ical.Net.Serialization.DataTypes;

namespace Ical.Net.Benchmarks;

[MemoryDiagnoser]
public class DataTypeSerializerBenchmarks
{
    private static readonly DurationSerializer durationSerializer = new();
    private static readonly PeriodSerializer periodSerializer = new();
    private static readonly UtcOffsetSerializer utcOffsetSerializer = new();
    private static readonly ParameterSerializer parameterSerializer = new();
    private static readonly PropertySerializer propertySerializer = new();

    private static readonly Duration Duration = new(days: -1, hours: -2, minutes: -3, seconds: -4);
    private static readonly Period Period = new(new CalDateTime(2025, 1, 2, 3, 4, 5, CalDateTime.UtcTzId), Duration.FromHours(1));
    private static readonly UtcOffset UtcOffset = new(TimeSpan.FromHours(-5.5));
    private static readonly CalendarParameter Parameter = new("TZID", "Europe/Vienna");
    private static readonly CalendarProperty Property = new("SUMMARY", "Weekly planning meeting");

    [Benchmark]
    public string? SerializeDuration() => durationSerializer.SerializeToString(Duration);

    [Benchmark]
    public string? SerializePeriod() => periodSerializer.SerializeToString(Period);

    [Benchmark]
    public string? SerializeUtcOffset() => utcOffsetSerializer.SerializeToString(UtcOffset);

    [Benchmark]
    public string? SerializeParameter() => parameterSerializer.SerializeToString(Parameter);

    [Benchmark]
    public string? SerializeProperty() => propertySerializer.SerializeToString(Property);
}
