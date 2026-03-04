using System;
using System.Buffers;

class Program
{
    static void Main()
    {
        // Example text to search through
        string sampleText = "The quick brown fox jumps over the lazy dog. This text contains various vowels and keywords.";

        // .NET 8/9 - SearchValues for character searches
        var vowelSearchValues = SearchValues.Create("aeiouAEIOU");
        var keywordSearchValues = SearchValues.Create(new[] { "fox", "dog", "quick", "lazy" });

        // Find first vowel in text
        ReadOnlySpan<char> textSpan = sampleText.AsSpan();
        int firstVowelIndex = textSpan.IndexOfAny(vowelSearchValues);
        Console.WriteLine($"First vowel found at index: {firstVowelIndex} ('{sampleText[firstVowelIndex]}')");

        // Find all vowel positions
        Console.WriteLine("All vowel positions:");
        int currentIndex = 0;
        while (currentIndex < textSpan.Length)
        {
            int vowelIndex = textSpan.Slice(currentIndex).IndexOfAny(vowelSearchValues);
            if (vowelIndex == -1) break;
            
            int absoluteIndex = currentIndex + vowelIndex;
            Console.WriteLine($"  Vowel '{textSpan[absoluteIndex]}' at position {absoluteIndex}");
            currentIndex = absoluteIndex + 1;
        }

        // .NET 9 - SearchValues for string searches
        Console.WriteLine("\nSearching for keywords:");
        foreach (string keyword in new[] { "fox", "dog", "quick", "lazy", "elephant" })
        {
            bool found = keywordSearchValues.Contains(keyword);
            int index = sampleText.IndexOf(keyword, StringComparison.OrdinalIgnoreCase);
            Console.WriteLine($"  '{keyword}': {(found && index >= 0 ? $"Found at index {index}" : "Not found")}");
        }

        // Performance comparison example
        Console.WriteLine("\nPerformance benefit: SearchValues is optimized for repeated searches");
        Console.WriteLine("- Uses vectorization and hardware acceleration");
        Console.WriteLine("- Immutable and thread-safe");
        Console.WriteLine("- Ideal for scenarios with many search operations on the same set of values");
    }
}