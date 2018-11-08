using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Levent.Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            // Kullanıcı girişleri
            Console.WriteLine("1. kullanıcı adı giriniz");
            string username1 = Console.ReadLine().ToUpper();
            Console.WriteLine("user1 = " + username1);

            Console.WriteLine("2. kullanıcı adı giriniz");
            string username2 = Console.ReadLine().ToUpper();
            Console.WriteLine("user1 = " + username2);

            int dimention = 0;

            // Boyut kontrolü
            while (true)
            {
                if (dimention == 0)
                {
                    Console.WriteLine("boyut giriniz");
                    dimention = Convert.ToInt32(Console.ReadLine());
                }
                else if (dimention < 2)
                {
                    Console.WriteLine(" Daha büyük boyut giriniz");
                    dimention = Convert.ToInt32(Console.ReadLine());
                }
                else
                {
                    break;
                }
            }

            Console.WriteLine("dimention = " + dimention + "x" + dimention);

            // Oyun oluşturma
            var game1 = new Game(username1, username2, dimention, dimention);

            bool gameIsOver;
            var playingException = ExceptionType.None;

            do
            {
                char letter = char.MinValue;
                int dimentionx = -1;
                int dimentiony = -1;
                int opponentDimentionx = -1;
                int opponentDimentiony = -1;

                // Play
                do
                {
                    if (playingException == ExceptionType.None || playingException == ExceptionType.Letter)
                    {
                        Console.WriteLine($"{game1.TurnOwner.Username} => Enter a letter");
                        letter = char.ToUpper(Convert.ToChar(Console.ReadLine()));
                    }

                    if (playingException == ExceptionType.None || playingException == ExceptionType.Dimension)
                    {
                        Console.WriteLine("Enter x dimention");
                        dimentionx = Convert.ToInt32(Console.ReadLine());

                        Console.WriteLine("Enter y dimention");
                        dimentiony = Convert.ToInt32(Console.ReadLine());
                    }

                    try
                    {
                        game1.Play(letter, dimentionx, dimentiony);
                        playingException = ExceptionType.None;
                    }
                    catch (IncorrectLetterException exc)
                    {
                        playingException = ExceptionType.Letter;
                        Console.WriteLine(exc.Message);
                    }
                    catch (GridCellException exc)
                    {
                        playingException = ExceptionType.Dimension;
                        Console.WriteLine(exc.Message);
                    }

                } while (playingException != ExceptionType.None);

                // Grid Gösterme
                Console.WriteLine("Enter * to view your grid");

                string continueOrViewGrid = Console.ReadLine();

                if (continueOrViewGrid.Equals("*"))
                {
                    Console.WriteLine("GRİD");
                }

                // PlayOpponent
                if (!continueOrViewGrid.Equals("*"))
                {
                    do
                    {
                        if (playingException == ExceptionType.None)
                        {
                            Console.WriteLine($"{game1.GetOpponent().Username} => You have to use this letter {letter}");
                        }

                        if (playingException == ExceptionType.None || playingException == ExceptionType.Dimension)
                        {
                            Console.WriteLine("Enter x dimention");
                            opponentDimentionx = Convert.ToInt32(Console.ReadLine());

                            Console.WriteLine("Enter y dimention");
                            opponentDimentiony = Convert.ToInt32(Console.ReadLine());
                        }

                        try
                        {
                            game1.PlayOpponentLetter(opponentDimentionx, opponentDimentiony);
                            playingException = ExceptionType.None;
                        }
                        catch (GridCellException e)
                        {
                            playingException = ExceptionType.Dimension;
                            Console.WriteLine(e.Message);
                        }
                        catch (IncorrectLetterException e)
                        {
                            playingException = ExceptionType.Letter;
                            Console.WriteLine(e.Message);
                        }

                    } while (playingException != ExceptionType.None);
                }

                gameIsOver = game1.GameIsOver();

            } while (!gameIsOver);

            Result result = game1.GetResult();
            if (result != null)
            {
                Console.WriteLine("Game over");
                Console.WriteLine($"Winner is {result.Winner.User.Username}");
                Console.WriteLine($"Winner's score = {result.Winner.Score}");
                Console.WriteLine("Winner's meaningful words =");
                result.Winner.MeaningfulWords.ForEach(Console.WriteLine);
                Console.WriteLine("===============================================");
                Console.WriteLine($"Opponent is {result.Loser.User.Username}");
                Console.WriteLine($"Opponent's score = {result.Loser.Score}");
                Console.WriteLine("Winner's meaningful words =");
                result.Loser.MeaningfulWords.ForEach(Console.WriteLine);
            }

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
                throw new GridCellException($"Your dimentions are out of size! Dimentions: ({x},{y})");
            }

            var empty = IsCellEmpty(TurnOwner, x, y);
            if (!empty)
            {
                throw new GridCellException($"Cell is not empty! Dimentions: ({x},{y})");
            }
            letter = char.ToUpper(letter);
            var correctLetter = IsLetterCorrect(letter);
            if (!correctLetter)
            {
                throw new IncorrectLetterException($"You used a letter that is not in the alphabet { letter }");
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
                throw new GridCellException($"Your dimentions are out of size! Dimentions: ({x},{y})");
            }

            var empty = IsCellEmpty(opponent, x, y);
            if (!empty)
            {
                throw new GridCellException($"Cell is not empty! Dimentions: ({x},{y})");
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
            letter = char.ToUpper(letter);
            if (LettersPoints.ContainsKey(letter))
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
            Words.Add("AL");
            Words.Add("AY");
            Words.Add("ALİ");
            Words.Add("YAY");
            Words.Add("ALA");
            Words.Add("LAL");
            Words.Add("AYAK");
            Words.Add("GEZİ");
            Words.Add("KİRA");
            Words.Add("AZRA");
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

        internal Result GetResult()
        {
            Result result = new Result();
            
            UserResult winner;
            UserResult loser;

            UserResult userResult1 = GetScore(User1);
            UserResult userResult2 = GetScore(User2);

            if (userResult1.Score > userResult2.Score)
            {
                result.Winner = userResult1;
                result.Loser = userResult2;
            }
            else if (userResult1.Score < userResult2.Score)
            {
                result.Winner = userResult2;
                result.Loser = userResult1;
            }
            else
            {
                winner = null;
                loser = null;
            }
            
            return result;
        }

        internal UserResult GetScore(User user)
        {
            UserResult userResult = GetMeaningfulWords(user);
            
            List<char> meaningfulLetter = new List<char>();

            for (int i = 0; i < userResult.MeaningfulWords.Count; i++)
            {
                for (int j = 0; j < userResult.MeaningfulWords[i].Length; j++)
                {
                    meaningfulLetter.Add(userResult.MeaningfulWords[i][j]);
                }
            }
            int playerScore = 0;

            for (int i = 0; i < meaningfulLetter.Count; i++)
            {
                if (LettersPoints.ContainsKey(meaningfulLetter[i]))
                {
                    playerScore += LettersPoints[meaningfulLetter[i]];
                }
            }
            userResult.Score = playerScore;

            return userResult;
        }

        private UserResult GetMeaningfulWords(User user)
        {
            UserResult userResult = new UserResult();
            string word = "";
            List<string> meaningfulwords = new List<string>();

            for (int i = 0; i < user.Grid.GetLength(0); i++)
            {
                for (int j = 0; j < user.Grid.GetLength(1); j++)
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
            userResult.User = user;
            userResult.MeaningfulWords = meaningfulwords;

            return userResult;
        }
    }

    enum ExceptionType
    {
        None = 0,
        Dimension = 1,
        Letter = 2
    }

    internal class User
    {
        public string Username { get; set; }
        public char[,] Grid { get; set; }
        public User(string user1Name, int x, int y)
        {
            this.Username = user1Name;
            this.Grid = new char[x, y];
        }
    }
    internal class UserResult
    {
        public User User { get; set; }
        public List<string> MeaningfulWords { get; set; }
        public int Score { get; set; }
    }

    internal class Result
    {
        public UserResult Winner { get; set; }
        public UserResult Loser { get; set; }

    }

    internal class GridCellException : Exception
    {
        internal GridCellException(string message) : base(message)
        {

        }
    }

    internal class IncorrectLetterException : Exception
    {
        internal IncorrectLetterException(string message) : base(message)
        {

        }
    }
}
