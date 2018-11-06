using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Levent.Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("1. kullanıcı adı giriniz");
            string username1 = Console.ReadLine().ToUpper();
            Console.WriteLine("user1 = " + username1);

            Console.WriteLine("2. kullanıcı adı giriniz");
            string username2 = Console.ReadLine().ToUpper();
            Console.WriteLine("user1 = " + username2);

            Console.WriteLine("boyut giriniz");
            int dimention = Convert.ToInt32(Console.ReadLine());
            Console.WriteLine("dimention = " + dimention + "x" + dimention);

            var game1 = new Game(username1, username2, dimention, dimention);

            bool gameIsOver;

            do
            {
                Console.WriteLine($"{game1.TurnOwner.Username} => Enter a letter");
                char letter_ = char.ToUpper(Convert.ToChar(Console.ReadLine()));

                Console.WriteLine("Enter x dimention");
                int dimentionx = Convert.ToInt32(Console.ReadLine());

                Console.WriteLine("Enter y dimention");
                int dimentiony = Convert.ToInt32(Console.ReadLine());

                game1.Play(letter_, dimentionx, dimentiony);
                Console.WriteLine("Enter * to view your grid");
                string continueOrViewGrid = Console.ReadLine();

                if (continueOrViewGrid.Equals("*"))
                {
                    Console.WriteLine("GRİD");
                }
                if (!continueOrViewGrid.Equals("*"))
                {
                    Console.WriteLine($"{game1.GetOpponent().Username} => You have to use this letter {letter_}");
                    Console.WriteLine("Enter x dimention");
                    int opponentDimentionx = Convert.ToInt32(Console.ReadLine());

                    Console.WriteLine("Enter y dimention");
                    int opponentDimentiony = Convert.ToInt32(Console.ReadLine());

                    game1.PlayOpponentLetter(opponentDimentionx, opponentDimentiony);
                }

                gameIsOver = game1.GameIsOver();

            } while (!gameIsOver);

            game1.GetResult(game1.User1);
            Console.WriteLine("===============================================");
            game1.GetResult(game1.User2);
            
            //Result result = game1.GetResult();
            //if (result != null)
            //{
            //    Console.WriteLine("Game over");
            //    Console.WriteLine($"Winner is {result.Winner}");
            //}
            
            Console.ReadLine();
        }
    }

    internal class Game
    {
        private static readonly Dictionary<char, int> LettersPoints = new Dictionary<char, int>();
        private static List<string> Words = new List<string>();
        public User User1 { get; set; }
        public User User2 { get; set; }
        public User TurnOwner { get; set; }

        private char lastPlayedLetter;

        public Game(string user1Name, string user2Name, int x, int y)
        {
            User1 = new User(user1Name, x, y);
            User2 = new User(user2Name, x, y);

            TurnOwner = User1;

            Load();
            LoadWords();
        }

        internal void Play(char letter, int x, int y)
        {
            if (lastPlayedLetter != '\0')
            {
                throw new Exception("Not your turn!");
            }

            var outOfSize = IsOutOfSize(TurnOwner, x, y);
            if (!outOfSize)
            {
                throw new Exception("Your dimentions are out of size!");
            }

            var correctLetter = IsLetterCorrect(char.ToUpper(letter));
            if (!correctLetter)
            {
                throw new Exception("You used a letter that is not in the alphabet");
            }

            var empty = IsCellEmpty(TurnOwner, x, y);
            if (!empty)
            {
                throw new Exception("Cell is not empty!");
            }

            lastPlayedLetter = letter;
            TurnOwner.Grid[x, y] = letter;
        }

        internal void PlayOpponentLetter(int x, int y)
        {
            if (lastPlayedLetter == '\0')
            {
                throw new Exception("Not your turn!");
            }

            var opponent = GetOpponent();
            var outOfSize = IsOutOfSize(opponent, x, y);
            if (!outOfSize)
            {
                throw new Exception("Your dimentions are out of size!");
            }

            var empty = IsCellEmpty(opponent, x, y);
            if (!empty)
            {
                throw new Exception("Cell is not empty!");
            }

            opponent.Grid[x, y] = lastPlayedLetter;
            SetTurnOwner();
            lastPlayedLetter = '\0';
        }

        internal bool GameIsOver()
        {
            int grid = 0;

            for (int i = 0; i < User1.Grid.GetLength(0); i++)
            {
                for (int j = 0; j < User1.Grid.GetLength(1); j++)
                {
                    if (User1.Grid[i, j] != '\0')
                    {
                        grid++;
                    }
                }
            }

            Console.WriteLine($"Not Empty Grid : {grid}");

            if (grid == User1.Grid.GetLength(0) * User1.Grid.GetLength(1))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        internal bool IsCellEmpty(User user, int x, int y)
        {
            var cellValue = user.Grid[x, y];
            if (cellValue == '\0')
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        internal bool IsLetterCorrect(char letter)
        {
            if (LettersPoints.ContainsKey(char.ToUpper(letter)))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        internal bool IsOutOfSize(User user, int x, int y)
        {
            if (x < user.Grid.GetLength(0) && x >= 0
                && y < user.Grid.GetLength(1) && y >= 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void Load()
        {
            LettersPoints.Add('A', 1);
            LettersPoints.Add('B', 3);
            LettersPoints.Add('C', 4);
            LettersPoints.Add('Ç', 4);
            LettersPoints.Add('D', 3);
            LettersPoints.Add('E', 1);
            LettersPoints.Add('F', 7);
            LettersPoints.Add('G', 5);
            LettersPoints.Add('Ğ', 8);
            LettersPoints.Add('H', 5);
            LettersPoints.Add('I', 2);
            LettersPoints.Add('İ', 1);
            LettersPoints.Add('J', 10);
            LettersPoints.Add('K', 1);
            LettersPoints.Add('L', 1);
            LettersPoints.Add('M', 2);
            LettersPoints.Add('N', 1);
            LettersPoints.Add('O', 2);
            LettersPoints.Add('Ö', 7);
            LettersPoints.Add('P', 5);
            LettersPoints.Add('R', 1);
            LettersPoints.Add('S', 2);
            LettersPoints.Add('Ş', 4);
            LettersPoints.Add('T', 1);
            LettersPoints.Add('U', 2);
            LettersPoints.Add('Ü', 3);
            LettersPoints.Add('V', 7);
            LettersPoints.Add('Y', 3);
            LettersPoints.Add('Z', 4);
        }

        private void LoadWords()
        {
            Words.Add("ALİ");
            Words.Add("EV");
            Words.Add("KAT");
            Words.Add("YAT");
            Words.Add("AD");
        }

        private void SetTurnOwner()
        {
            var opponent = GetOpponent();
            TurnOwner = opponent;
        }

        internal User GetOpponent()
        {
            if (TurnOwner == User1)
            {
                return User2;
            }
            else
            {
                return User1;
            }
        }

        internal void GetResult(User user)
        {
            Console.WriteLine("SHOW RESULT");
            string word = "";
            List<string> meaningfulwords = new List<string>();

            for (int i = 0; i < user.Grid.GetLength(0); i++)
            {
                for (int j = 0 ; j < user.Grid.GetLength(1); j++)
                {
                    char c = user.Grid[i, j];
                    word += c.ToString();

                    if (Words.Contains(word))
                    {
                        meaningfulwords.Add(word);
                    }
                }

                word = "";
            }
            
            for (int i = 0; i < user.Grid.GetLength(0); i++)
            {
                for (int j = 0; j < user.Grid.GetLength(1); j++)
                {
                    char c = user.Grid[j, i];
                    word += c.ToString();

                    if (Words.Contains(word))
                    {
                        meaningfulwords.Add(word);
                    }                    
                }

                word = "";
            }


            user.MeaningfulWords = meaningfulwords;

            user.MeaningfulWords.ForEach(Console.WriteLine); 
        }
    }

    internal class User
    {
        public string Username { get; set; }
        public char[,] Grid { get; set; }
        public List<string> MeaningfulWords { get; set; }
        public User(string user1Name, int x, int y)
        {
            this.Username = user1Name;
            this.Grid = new char[x, y];
            this.MeaningfulWords = MeaningfulWords;
        }
    }

    internal class Result
    {
        public User Winner { get; set; }
        
    }
}
