using EvEnergyApi.Models;
using EvEnergyApi.Services;
using System.Net;
using System.Text.Json;
using Moq;
using Xunit;

namespace EvEnergyApi.Tests;

public class CarbonIntensityServiceTests
{
    private GenerationSlot MakeSlot(string from, string to, params (string fuel, double perc)[] mix) =>
        new GenerationSlot(from, to, mix.Select(m => new GenerationMixItem(m.fuel, m.perc)).ToArray());

    [Fact]
    public void CleanEnergyPercent_SumsCorrectFuels()
    {
        var slots = new[]
        {
            MakeSlot("2026-06-10T00:00Z", "2026-06-10T00:30Z",
                ("wind", 30), ("solar", 20), ("nuclear", 10),
                ("hydro", 5), ("biomass", 5), ("gas", 20), ("coal", 10))
        };
        var cleanFuels = new[] { "biomass", "nuclear", "hydro", "wind", "solar" };
        var cleanPercent = slots
            .SelectMany(s => s.Generationmix)
            .Where(m => cleanFuels.Contains(m.Fuel))
            .Sum(m => m.Perc);

        Assert.Equal(70, cleanPercent);
    }

    [Fact]
    public void GroupByDate_GroupsCorrectlyInUtc()
    {
        var slots = new[]
        {
            MakeSlot("2026-06-10T22:00Z", "2026-06-10T22:30Z", ("wind", 40)),
            MakeSlot("2026-06-10T23:00Z", "2026-06-10T23:30Z", ("wind", 60)),
            MakeSlot("2026-06-11T00:00Z", "2026-06-11T00:30Z", ("wind", 50)),
        };

        var grouped = slots
            .GroupBy(slot => DateTimeOffset.Parse(slot.From).UtcDateTime.Date)
            .ToList();
        Assert.Equal(2, grouped.Count);
        Assert.Equal(2, grouped[0].Count()); 
        Assert.Equal(1, grouped[1].Count());
    }

    [Fact]
    public void BestChargingWindow_SelectsHighestCleanEnergyWindow()
    {
        var cleanPerSlot = new double[] { 40, 40, 80, 80, 50, 50 };
        var windowSize = 2;
        var bestIndex = 0;
        var bestAverage = 0.0;

        for (int i = 0; i <= cleanPerSlot.Length - windowSize; i++)
        {
            var average = cleanPerSlot.Skip(i).Take(windowSize).Average();
            if (average > bestAverage)
            {
                bestAverage = average;
                bestIndex = i;
            }
        }

        Assert.Equal(2, bestIndex);
        Assert.Equal(80.0, bestAverage);
    }

    [Fact]
    public void AverageMix_CalculatesCorrectlyPerFuel()
    {
        var slots = new[]
        {
            MakeSlot("2026-06-10T00:00Z", "2026-06-10T00:30Z", ("wind", 40), ("gas", 60)),
            MakeSlot("2026-06-10T00:30Z", "2026-06-10T01:00Z", ("wind", 60), ("gas", 40)),
        };

        var avgWind = slots
            .SelectMany(s => s.Generationmix)
            .Where(m => m.Fuel == "wind")
            .Average(m => m.Perc);

        Assert.Equal(50, avgWind);
    }
}