namespace IDP
{
    public static class Utils
    {
        public static string PromptText(string prompt, ConsoleColor color = ConsoleColor.White, bool newline = true)
        {
            if (newline)
                Console.WriteLine();

            Console.ForegroundColor = color;
            Console.WriteLine(prompt);

            string? input;
            do
            {
                input = Console.ReadLine();
                if (string.IsNullOrEmpty(input))
                    Console.WriteLine("provide a value");
            }
            while (string.IsNullOrEmpty(input));

            Console.ResetColor();
            return input;
        }

        public static int PromptNumber(string prompt, ConsoleColor color = ConsoleColor.White, bool newline = true)
        {
            if (newline)
                Console.WriteLine();

            Console.ForegroundColor = color;
            Console.WriteLine(prompt);

            int input;
            while (true)
            {
                int? nereyda = int.TryParse(Console.ReadLine() ?? string.Empty, out int i) ? i : null;
                if (nereyda is null)
                {
                    Console.WriteLine("provide a number");
                    continue;
                }
                input = (int)nereyda;
                break;
            }

            Console.ResetColor();
            return input;
        }
    }
}
