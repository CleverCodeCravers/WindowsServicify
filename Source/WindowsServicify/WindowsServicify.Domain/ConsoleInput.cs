namespace WindowsServicify.Domain;

public static class ConsoleInput
{
    public static string ReadInput(string prompt, bool required)
    {
        Console.WriteLine(prompt);
        string input = string.Empty;

        while (string.IsNullOrWhiteSpace(input))
        {
            input = Console.ReadLine()!;

            if (string.IsNullOrWhiteSpace(input) && required)
            {
                Console.WriteLine("Sorry, but an empty input is not allowed.");
            }

            if (!required)
                break;
        }
        
        return input;
    }
}