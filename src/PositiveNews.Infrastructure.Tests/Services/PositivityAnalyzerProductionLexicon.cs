using Microsoft.Extensions.Options;
using PositiveNews.Application.DTOs.Ingestion;
using PositiveNews.Infrastructure.Configuration;
using PositiveNews.Infrastructure.Services;

namespace PositiveNews.Infrastructure.Tests.Services;

/// <summary>
/// Builds the production-like sentiment lexicon for tests without reading appsettings from disk.
/// </summary>
internal static class PositivityAnalyzerProductionLexicon
{
    public static PositivityAnalizerKeyPhrases Create()
        => new IngestionSettingsProvider(Options.Create(BuildConfig()))
            .GetCurrentSettings()
            .PositivityAnalizerKeyPhrases;

    private static IngestionSettingsConfig BuildConfig() => new()
    {
        Common = new CommonIngestionConfig
        {
            PositivityAnalizerKeyPhrases = new PositivityAnalizerKeyPhrasesConfig
            {
                PositiveWords =
                [
                    "good", "great", "greater", "greatest", "excellent", "positive", "happiness", "happy", "success", "breakthrough",
                    "innovative", "uplifting", "joy", "joyful", "wonderful", "win", "wins", "winning", "progress", "inspiring", "inspired",
                    "cure", "cures", "heal", "healing", "hero", "heroes", "solution", "solutions", "miracle", "miracles", "triumph", "triumphant",
                    "beautiful", "lovely", "love", "loved", "hope", "hopeful", "recover", "recovery", "relief", "safe", "safer", "peace", "peaceful",
                    "grow", "growth", "improve", "improved", "better", "best", "benefit", "benefits", "rescue", "rescued", "saved", "celebrate", "celebration",
                    "donate", "donation", "charity", "breakthroughs", "discovery", "discoveries", "record", "records", "renew", "renewal",
                    "resilient", "resilience", "dignity", "cooperate", "cooperation", "cooperative", "milestone", "remarkable", "boundless", "honor", "honour", "honored", "honoured",
                    "enlighten", "enlightenment", "relax", "relaxation", "aspire", "aspirational", "rebuild", "rebuilding", "reconstruction", "innovate", "innovation",
                    "promising", "promise", "protect", "protected", "protection", "rights", "delight", "delightful", "uplift", "uplifted"
                ],
                NegativeWords =
                [
                    "bad", "worse", "worst", "terrible", "awful", "negative", "sad", "sadness", "fail", "fails", "failed", "failure",
                    "crisis", "disaster", "tragedy", "tragic", "loss", "losses", "pain", "painful", "death", "deaths", "dead", "die", "died",
                    "murder", "murdered", "war", "wars", "crash", "crashed", "devastating", "devastated", "horrible", "fear", "fears",
                    "hate", "hatred", "violence", "violent", "attack", "attacks", "outbreak", "outbreaks", "threat", "threats", "harm", "harmful",
                    "abuse", "toxic", "collapse", "ruin", "doom", "bombed", "bombing", "rubble", "destruction", "destroyed", "conflict",
                    "refugee", "refugees", "pessimistic", "pessimism", "devastation"
                ],
                PositivePhrases =
                [
                    "good news", "great news", "silver lining", "bright spot", "well done", "make a difference", "signs of hope",
                    "reason to celebrate", "step forward", "leaps forward", "record high", "all time high", "turning point", "breakthrough in",
                    "path to recovery", "on the mend", "back on track", "lives saved", "lives have been saved", "zero deaths", "no deaths",
                    "signs of cooperation", "make the world a better place", "hope can rise", "human dignity", "human rights",
                    "incredible milestone", "binding international", "near complete protection", "close to zero risk"
                ],
                NegativePhrases =
                [
                    "no hope", "worst case", "lost their life", "loss of life", "human toll", "mass shooting", "at least dead",
                    "declared dead", "fears grow", "fears mount", "out of control", "state of emergency", "on life support"
                ],
                MitigationWords =
                [
                    "zero", "prevent", "prevented", "preventing", "eliminated", "eliminating", "saved", "saving", "fewer",
                    "reduced", "reducing", "decline", "declining", "avoided", "absent"
                ],
                MitigationPhrases =
                [
                    "zero deaths", "no deaths", "lives saved", "lives have been saved", "prevented deaths",
                    "close to zero risk", "without vaccination"
                ],
                NegationWords = ["not", "no", "never", "neither", "nor", "without", "lack", "lacking", "hardly", "barely", "scarcely"],
                IntensifierWords = ["very", "extremely", "highly", "incredibly", "deeply", "truly", "really", "utterly", "remarkably", "especially", "particularly"],
                NegationLookbackTokens = 4,
                IntensifierLookbackTokens = 2,
                IntensifierMultiplier = 1.35,
                PhrasePolarityWeight = 2.0,
                MitigationLookbackTokens = 4,
                TitleWeight = 0.15,
                LedeWeight = 0.35,
                BodyWeight = 0.50,
                LedeCharCount = 500
            }
        }
    };
}
