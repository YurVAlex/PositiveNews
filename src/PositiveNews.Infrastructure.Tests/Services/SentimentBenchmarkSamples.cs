namespace PositiveNews.Infrastructure.Tests.Services;

internal readonly record struct SentimentBenchmarkSample(string Text, decimal HumanScore, string? Title = null);

/// <summary>
/// Curated article-style snippets used to regression-test sentiment ranking.
/// </summary>
internal static class SentimentBenchmarkSamples
{
    public static readonly SentimentBenchmarkSample[] All =
    [
        new(
            "NVIDIA technologies power more than 400 of the world's fastest supercomputers with record growth and progress across AI and science.",
            0.52m),
        new(
            "Telecom operators have seen remarkable returns from secure automation that keeps humans in control of policy and safer network operations.",
            0.54m),
        new(
            "Europe's first exascale supercomputer enables breakthrough discoveries, inspiring progress in climate science and brain research.",
            0.56m),
        new(
            "Researchers uncovered groundbreaking technologies that will reshape healthcare, agriculture and energy with innovative support programs.",
            0.58m),
        new(
            "Eco Wave Power is developing promising technology that converts ocean waves into clean electricity using existing marine infrastructure.",
            0.62m),
        new(
            "Play favorite titles, keep progress synced, and jump back into joyful gaming sessions with a wonderful membership offer this summer.",
            0.60m),
        new(
            "Between 2020 and 2024, not one woman between the ages of 20 and 24 in England died from cervical cancer. The HPV vaccine is delivering on what it promised. Children vaccinated at ages 12 and 13 have close to zero risk of dying from cervical cancer before age 30. Since the school vaccination program began, approximately 200 lives have been saved. It is incredible to think that a single jab can almost eliminate a particular type of cancer.",
            0.85m),
        new(
            "An admin date is the simplest fix for a heavy to-do list. Psychology research shows accountability and connection can cultivate joy, healing, and better mental health while getting things done together.",
            0.68m),
        new(
            "For the first time, gig workers have binding international labour protections with minimum standards for safety, pay, and human dignity. Workers called the result a breakthrough and an incredible milestone.",
            0.72m),
        new(
            "In a globally representative study, 69 percent of participants chose to cooperate with a stranger for the common good. Lead author Armin Falk said if we were less pessimistic and more realistic, we could live in a better world.",
            0.70m),
        new(
            "Tala and Farah won the Earth Prize for Build Hope Palestine, turning rubble into reusable blocks after their home was bombed. You gave us hope we had completely lost, people wrote. Hope can rise amid destruction. They want peace, dignity, and human rights for their generation.",
            0.68m,
            "Teen sisters turn rubble into hope"),
        new(
            "Father's Day spent decades in limbo before becoming a national holiday. Today it celebrates the diverse ways fathers show love, care, and joy in family life.",
            0.58m),
        new(
            "David Daigle's forthcoming exhibition, The Death of Beauty, investigates identity and desire through layered commercial imagery and precise artistic composition.",
            0.48m,
            "The Death of Beauty"),
        new(
            "C.F. Payne's illustration work mixes draftsmanship, texture, and playful character with affection for drawing that feels direct and observational.",
            0.55m),
        new(
            "Teber is a visual artist whose work feels optimistic, polished, and surreal. He talks about seeking positivity, joy, enlightenment, relaxation, and escape through luminous composition.",
            0.72m),
        new(
            "FERC issued a major milestone on grid interconnection, helping lower energy costs, grow the industrial base, and strengthen the electrical grid.",
            0.55m),
        new(
            "GeForce NOW is making it easier than ever to get more from the cloud this summer with limited-time savings and delightful new games for members.",
            0.61m),
        new(
            "Los Alamos National Laboratory supercomputers will accelerate scientific discovery and unlock agentic AI for science with innovative Vera CPUs.",
            0.55m),
        new(
            "NVIDIA XR AI is now available in public beta, giving developers a framework for building multimodal AI agents for AR glasses and XR devices.",
            0.54m),
        new(
            "A terrible crisis caused devastating losses, widespread fear, violent attacks, and tragic deaths across the region with no hope of recovery.",
            0.30m)
    ];
}
