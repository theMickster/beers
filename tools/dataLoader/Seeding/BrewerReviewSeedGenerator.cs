using System.Security.Cryptography;
using System.Text;
using BeersDataLoader.Infrastructure;

namespace BeersDataLoader.Seeding;

internal static class BrewerReviewSeedGenerator
{
    internal static async Task<bool> EnsureDataFileAsync(string brewersPath, string reviewsPath)
    {
        if (!File.Exists(brewersPath))
        {
            Console.WriteLine($"Unable to locate brewers data file: {brewersPath}");
            return false;
        }

        var brewers = await JsonFileStore.ReadListAsync<dynamic>(brewersPath);
        if (brewers.Count == 0)
        {
            Console.WriteLine("No brewer records found in the seed source.");
            return false;
        }

        var reviews = new List<object>();
        var now = DateTime.UtcNow;
        var reviewerNames = new[]
        {
            "Sam Taproom", "Casey Malt", "Jordan Hop", "Taylor Flight",
            "Morgan Keg", "Riley Foam", "Alex Pint", "Jamie Barrel"
        };
        var reviewTitles = new[]
        {
            "Outstanding lineup", "Great seasonal release", "Solid core beers", "Impressive consistency",
            "Fantastic tasting room", "Worth the hype", "Great value for quality", "Excellent craft portfolio"
        };
        var reviewComments = new[]
        {
            "The beer list is deep, balanced, and very well executed.",
            "Seasonals were fresh and distinct. Would recommend to friends.",
            "Core selections are consistent and easy to enjoy any time.",
            "A strong brewery identity with reliable quality across styles.",
            "Taproom service and beer quality were both top tier.",
            "Strong aroma, clean finish, and memorable flavor profile.",
            "Great quality for price; several options were standouts.",
            "Creative, polished releases with good style variety."
        };

        foreach (var brewer in brewers)
        {
            var brewerId = ((string)brewer.BrewerId)?.ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(brewerId))
            {
                continue;
            }

            for (var i = 0; i < 12; i++)
            {
                var rating = (i % 5) + 1;
                var created = now.AddDays(-(i * 3));
                var deterministicSeed = $"brewerreview|v1|{brewerId}|{i:D2}";
                reviews.Add(new
                {
                    id = CreateDeterministicGuid(deterministicSeed),
                    EntityType = PartitionKeyConstants.BrewerReview,
                    BrewerId = brewerId,
                    ReviewerName = reviewerNames[i % reviewerNames.Length],
                    Title = reviewTitles[i % reviewTitles.Length],
                    Comments = reviewComments[i % reviewComments.Length],
                    Rating = rating,
                    isDeletable = true,
                    CreatedBy = "stanley.hudson.AdventureWorks@mickletofsky.com",
                    ModifiedBy = "jim.halpert.AdventureWorks@mickletofsky.com",
                    CreatedDate = created.ToString("O"),
                    ModifiedDate = created.ToString("O")
                });
            }
        }

        await JsonFileStore.WriteIndentedAsync(reviewsPath, reviews);
        return true;
    }

    private static string CreateDeterministicGuid(string seed)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(seed));
        var bytes = new byte[16];
        Array.Copy(hash, bytes, 16);
        return new Guid(bytes).ToString().ToLowerInvariant();
    }
}
