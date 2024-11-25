using System;
using System.IO;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        Console.WriteLine("Program začíná běžet...");

        // Krok 1: Cesta k adresáři je nastavena napevno
        string directoryPath = @"D:\";
        Console.WriteLine($"Hledám v adresáři: {directoryPath}");

        if (Directory.Exists(directoryPath))
        {
            Console.WriteLine("Adresář nalezen.");
            string[] files = Directory.GetFiles(directoryPath, "*.1x");
            Console.WriteLine($"Nalezeno {files.Length} souborů s příponou .1x");

            foreach (string file in files)
            {
                Console.WriteLine($"\nZpracovávám soubor: {file}");
                try
                {
                    // Kontrola existence souboru
                    if (!File.Exists(file))
                    {
                        Console.WriteLine($"Soubor {file} neexistuje!");
                        continue;
                    }

                    // Čtení obsahu souboru
                    string content = File.ReadAllText(file);
                    Console.WriteLine($"Načteno znaků: {content.Length}");
                    string modifiedContent = content;
                    bool foundMatch;
                    int iterationCount = 0;
                    // Zde by měl začít cyklus, který bude zpracovávat text tak dlouho, dokud nalezne nějakou shodu - nevím přesně, jak by měl výsledek vypadat - nutno otestovat !!! 
                    do
                    {
                        iterationCount++;
                        Console.WriteLine($"\nIterace č. {iterationCount}");

                        // Pokus o nalezení a nahrazení první shody
                        var (newContent, matched, groups) = ReplaceFirstOccurrence(modifiedContent,
                            @"\\sup ([tvo])(\d+)\\sup\*\\([fx]) \\sup ([tvo])(\d+)\\sup\*((?:(?!\\f|\\x)[\s\S])*?)\\([fx])\*%(\d+)-(\d+)%");

                        foundMatch = matched;
                        if (foundMatch)
                        {
                            Console.WriteLine("Nalezena shoda!");
                            Console.WriteLine($"Zachycené skupiny: {string.Join(", ", groups)}");

                            // Uložení do dočasného souboru
                            string tempFilePath = Path.Combine(directoryPath, Path.GetFileNameWithoutExtension(file) + ".tmp");
                            File.WriteAllText(tempFilePath, newContent);
                            Console.WriteLine($"Vytvořen dočasný soubor: {tempFilePath}");

                            // Nahrazení všech výskytů druhého vzoru
                            string tempContent = File.ReadAllText(tempFilePath);
                            string finalContent = ReplaceAllOccurrences(tempContent,
                                $@"\\sup z§\\sup\*%{groups[8]}-(\d+)%",
                                match => $@"\sup {groups[1]}{groups[2]}\sup*%{groups[8]}%{match.Groups[1].Value}%");

                            // Aktualizace obsahu pro další iteraci
                            modifiedContent = finalContent;

                            // Odstranění dočasného souboru
                            File.Delete(tempFilePath);
                            Console.WriteLine("Dočasný soubor odstraněn");
                        }
                        else
                        {
                            Console.WriteLine("Žádná další shoda nebyla nalezena.");
                        }
                    } while (foundMatch);

                    // Zápis konečného výsledku
                    string newFilePath = Path.Combine(directoryPath, Path.GetFileNameWithoutExtension(file) + ".1x");
                    File.WriteAllText(newFilePath, modifiedContent);
                    Console.WriteLine($"Soubor uložen: {newFilePath}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Chyba při zpracování souboru {file}:");
                    Console.WriteLine($"Typ chyby: {ex.GetType().Name}");
                    Console.WriteLine($"Zpráva: {ex.Message}");
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                }
            }
        }
        else
        {
            Console.WriteLine("Zadaný adresář neexistuje!");
        }

        Console.WriteLine("\nProgram dokončen. Stiskněte libovolnou klávesu pro ukončení...");
        Console.ReadKey();
    }

    static (string result, bool matched, string[] groups) ReplaceFirstOccurrence(string input, string pattern)
    {
        Regex regex = new Regex(pattern);
        Match match = regex.Match(input);

        if (!match.Success)
        {
            return (input, false, Array.Empty<string>());
        }

        string[] capturedGroups = new string[match.Groups.Count];
        for (int i = 0; i < match.Groups.Count; i++)
        {
            capturedGroups[i] = match.Groups[i].Value;
        }

        string result = regex.Replace(input, @"\sup $1$2\sup*\$3 \sup $4$5\sup*$6\$7*%$8%$9%", 1);
        return (result, true, capturedGroups);
    }

    static string ReplaceAllOccurrences(string input, string pattern, MatchEvaluator evaluator)
    {
        Regex regex = new Regex(pattern);
        return regex.Replace(input, evaluator);
    }
}