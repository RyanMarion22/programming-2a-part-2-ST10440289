using System;
using System.Collections.Generic;
using System.Media;
using System.Threading;

namespace CybersecurityAwarenessBot
{
    class Program
    {
        // Delegate for chatbot command handlers
        delegate void ChatResponse(string input, string userName);

        // Keyword-based responses
        static Dictionary<string, List<string>> keywordResponses = new Dictionary<string, List<string>>
        {
            ["phishing"] = new List<string>
            {
                "Be cautious of emails asking for personal information.",
                "Check sender addresses for subtle spelling mistakes.",
                "Avoid clicking on suspicious links in emails."
            },
            ["password"] = new List<string>
            {
                "Use strong, unique passwords with letters, numbers, and symbols.",
                "Avoid using names or birthdays in passwords.",
                "Consider using a password manager to store passwords securely."
            },
            ["privacy"] = new List<string>
            {
                "Review your privacy settings regularly.",
                "Limit personal info you share online.",
                "Use secure messaging apps for private conversations."
            },
            ["scam"] = new List<string> // Added "scam" keyword
            {
                "Scammers often pose as trusted entities; always verify before acting.",
                "Watch for urgent demands for money or personal details.",
                "Report suspicious activity to authorities immediately."
            }
        };

        // Sentiment keywords
        static Dictionary<string, string> sentimentResponses = new Dictionary<string, string>
        {
            ["worried"] = "It's completely understandable to feel that way. Let me share some tips to ease your mind.",
            ["curious"] = "Curiosity is great! Here's some info to dive deeper.",
            ["frustrated"] = "No problem — cybersecurity can be tricky. I'm here to help with clear advice."
        };

        // Command handlers using delegates
        static Dictionary<string, ChatResponse> commandHandlers = new Dictionary<string, ChatResponse>
        {
            ["security tips"] = (input, name) => ShowSecurityTips(),
            ["how are you"] = (input, name) => Console.WriteLine($"I'm doing great, {name}! Ready to help you stay safe online."),
            ["what can i ask"] = (input, name) => ShowOptions()
        };

        // Memory for user preferences
        static Dictionary<string, string> userMemory = new Dictionary<string, string>();

        static void Main()
        {
            Console.Title = "Cybersecurity Awareness Bot";
            PlayVoiceGreeting();
            DisplayAsciiArt();
            PrintDivider();

            string userName = GetUserName();
            TypeEffect($"\n👋 Hello {userName}! I'm your Cybersecurity Awareness Bot.", 40);
            PrintDivider();
            ShowOptions();

            while (true)
            {
                Console.Write("\nAsk me something (or type 'exit' to quit): ");
                string input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input)) continue;

                if (input.ToLower() == "exit")
                {
                    Console.WriteLine("\nGoodbye! Stay safe online!");
                    break;
                }

                HandleUserQuestion(input, userName);
            }
        }

        static void HandleUserQuestion(string input, string userName)
        {
            // Normalize input to lowercase for consistent matching
            input = input.ToLower().Trim();
            Console.ForegroundColor = ConsoleColor.Green;

            // Handle user interest memory
            if (input.Contains("i'm interested in"))
            {
                string interest = input.Substring(input.IndexOf("in") + 3).Trim();
                userMemory["interest"] = interest;
                Console.WriteLine($"Great! I'll remember that you're interested in {interest}.");
                SuggestRelatedTip(interest); // Suggest a related tip
                Console.ResetColor();
                return;
            }

            if (input.Contains("remind me"))
            {
                if (userMemory.ContainsKey("interest"))
                {
                    Console.WriteLine($"You told me you're interested in {userMemory["interest"]}. Here's something related...");
                    SuggestRelatedTip(userMemory["interest"]);
                }
                else
                {
                    Console.WriteLine("I don't think you've told me your interest yet.");
                }
                Console.ResetColor();
                return;
            }

            // Check for sentiment and combine with keyword if applicable
            if (HandleSentiment(input, userName))
            {
                // Sentiment handled; check for keyword to combine responses
                string keyword = HandleKeyword(input, userName, true);
                if (!string.IsNullOrEmpty(keyword))
                {
                    ShowFollowUpPrompt(keyword); // Offer follow-up after keyword response
                }
                Console.ResetColor();
                return;
            }

            // Check for keyword-based responses
            string matchedKeyword = HandleKeyword(input, userName, false);
            if (!string.IsNullOrEmpty(matchedKeyword))
            {
                ShowFollowUpPrompt(matchedKeyword); // Offer follow-up after keyword response
                Console.ResetColor();
                return;
            }

            // Check for specific commands (via delegate handlers)
            foreach (var handler in commandHandlers)
            {
                if (input.Contains(handler.Key))
                {
                    handler.Value(input, userName);
                    Console.ResetColor();
                    return;
                }
            }

            // Fallback for unknown input
            Console.WriteLine("I didn't quite understand that. Could you rephrase?");
            Console.ResetColor();
        }

        // Handle sentiment detection
        static bool HandleSentiment(string input, string userName)
        {
            foreach (var sentiment in sentimentResponses)
            {
                if (input.Contains(sentiment.Key))
                {
                    Console.WriteLine($"{sentiment.Value}");
                    return true;
                }
            }
            return false;
        }

        // Handle keyword detection and response
        static string HandleKeyword(string input, string userName, bool isSentimentContext)
        {
            foreach (var pair in keywordResponses)
            {
                if (input.Contains(pair.Key))
                {
                    Random rnd = new Random();
                    int index = rnd.Next(pair.Value.Count);
                    string response = pair.Value[index];

                    // Personalize response if user has an interest
                    if (userMemory.ContainsKey("interest") && userMemory["interest"].Contains(pair.Key))
                    {
                        response = $"Since you're interested in {pair.Key}, here's a tip: {response}";
                    }

                    if (!isSentimentContext)
                    {
                        Console.WriteLine(response);
                    }
                    else
                    {
                        Console.WriteLine($"Here's a tip on {pair.Key}: {response}");
                    }
                    return pair.Key;
                }
            }
            return null;
        }

        // Suggest a tip related to the user's interest
        static void SuggestRelatedTip(string interest)
        {
            foreach (var pair in keywordResponses)
            {
                if (interest.Contains(pair.Key))
                {
                    Random rnd = new Random();
                    int index = rnd.Next(pair.Value.Count);
                    Console.WriteLine($"Here's a tip related to {pair.Key}: {pair.Value[index]}");
                    ShowFollowUpPrompt(pair.Key);
                    return;
                }
            }
            Console.WriteLine("I couldn't find a specific tip for that interest, but feel free to ask about phishing, passwords, privacy, or scams!");
        }

        // Show follow-up prompt to encourage deeper conversation
        static void ShowFollowUpPrompt(string topic)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"\nWant more tips on {topic}? Just say 'more about {topic}' or ask something else!");
            Console.ResetColor();
        }

        static string GetUserName()
        {
            Console.Write("Please enter your name: ");
            string name = Console.ReadLine();

            while (string.IsNullOrWhiteSpace(name))
            {
                Console.Write("Name cannot be empty. Try again: ");
                name = Console.ReadLine();
            }

            return name;
        }

        static void ShowOptions()
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("You can ask me about:");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("    Phishing");
            Console.WriteLine("    Password Safety");
            Console.WriteLine("    Privacy");
            Console.WriteLine("    Scams"); // Added scams to options
            Console.WriteLine("    Security Tips");
            Console.WriteLine("    You can also say how you're feeling or what you're interested in.");
            Console.ResetColor();
        }

        static void ShowSecurityTips()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Here are 3 quick security tips:");
            Console.WriteLine("   1. Keep your software and OS updated.");
            Console.WriteLine("   2. Avoid using public Wi-Fi for sensitive info.");
            Console.WriteLine("   3. Use 2FA on your accounts.");
            Console.ResetColor();
        }

        static void DisplayAsciiArt()
        {
            string asciiArt = @"
       .--------.
      / .------. \
     / /        \ \
     | |        | |
    _| |________| |_
  .' |_|        |_| '.
  '._____ ____ _____.' 
  |     .'____'.     |
  '.__.'.'    '.'.__.'  
  '.__  | LOCK |  __.' 
  |   '.'.____.'.'   |
  '.____'.____.'____.'
  '.________________.'
 Your Cybersecurity Awareness Assistant
";
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(asciiArt);
            Console.ResetColor();
        }

        static void PrintDivider()
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(new string('-', 60));
            Console.ResetColor();
        }

        static void TypeEffect(string text, int delay = 20)
        {
            foreach (char c in text)
            {
                Console.Write(c);
                Thread.Sleep(delay);
            }
            Console.WriteLine();
        }

        static void PlayVoiceGreeting()
        {
            try
            {
                SoundPlayer player = new SoundPlayer("CyberBot Audio.wav");
                player.PlaySync();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Could not play greeting audio: {ex.Message}. Make sure 'CyberBot Audio.wav' is in the correct directory.");
                Console.ResetColor();
            }
        }
    }
}