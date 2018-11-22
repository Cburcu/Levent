using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Levent.Engine
{
    public class Game
    {
        public User TurnOwner { get; set; }

        public static readonly Dictionary<char, int> LettersPoints = new Dictionary<char, int>();
        private static List<string> Words = new List<string>();
        private User User1 { get; set; }
        private User User2 { get; set; }
        private char lastPlayedLetter;

        public Game(string user1Name, string user2Name, int x, int y)
        {
            var inValidDimension = InvalidDimension(x, y);
            if (inValidDimension)
            {
                throw new InvalidDimensionException($"Your dimension is invalid!");
            }

            User1 = new User(user1Name, x, y);
            User2 = new User(user2Name, x, y);

            TurnOwner = User1;

            LoadLetters();
            LoadWords();
        }

        public void Play(char letter, int x, int y)
        {
            letter = char.ToUpper(letter);

            if (lastPlayedLetter != '\0')
            {
                throw new Exception("Not your turn!");
            }

            var outOfSize = IsOutOfSize(TurnOwner, x, y);
            if (!outOfSize)
            {
                throw new GridCellException($"Your dimensions are out of size! Dimensions: ({x},{y})");
            }

            var empty = IsCellEmpty(TurnOwner, x, y);
            if (!empty)
            {
                throw new GridCellException($"Cell is not empty! Dimensions: ({x},{y})");
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

        public void PlayOpponentLetter(int x, int y)
        {
            if (lastPlayedLetter == '\0')
            {
                throw new Exception("Not your turn!");
            }

            var opponent = GetOpponent();
            var outOfSize = IsOutOfSize(opponent, x, y);
            if (!outOfSize)
            {
                throw new GridCellException($"Your dimensions are out of size! Dimensions: ({x},{y})");
            }

            var empty = IsCellEmpty(opponent, x, y);
            if (!empty)
            {
                throw new GridCellException($"Cell is not empty! Dimensions: ({x},{y})");
            }

            opponent.Grid[x, y] = lastPlayedLetter;
            SetTurnOwner();
            lastPlayedLetter = '\0';
        }

        public bool GameIsOver()
        {
            int grid = 0;

            for (int i = 0; i < User2.Grid.GetLength(0); i++)
            {
                for (int j = 0; j < User2.Grid.GetLength(1); j++)
                {
                    if (User2.Grid[i, j] != '\0')
                    {
                        grid++;
                    }
                }
            }

            if (grid == User2.Grid.GetLength(0) * User2.Grid.GetLength(1))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public User GetOpponent()
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

        public Result GetResult()
        {
            UserResult userResult1 = new UserResult();
            UserResult userResult2 = new UserResult();

            Result result = new Result();

            userResult1.MeaningfulWords = GetMeaningfulWords(User1.Grid);
            userResult2.MeaningfulWords = GetMeaningfulWords(User2.Grid);

            userResult1.Score = GetScore(userResult1.MeaningfulWords);
            userResult2.Score = GetScore(userResult2.MeaningfulWords);

            userResult1.User = User1;
            userResult2.User = User2;

            if (userResult1.Score >= userResult2.Score)
            {
                result.Winner = userResult1;
                result.Loser = userResult2;
            }
            else
            {
                result.Winner = userResult2;
                result.Loser = userResult1;
            }

            return result;
        }

        public void ShowGrid(char[,] grid)
        {
            string row = "";
            for (int i = 0; i < grid.GetLength(0); i++)
            {
                for (int j = 0; j < grid.GetLength(1); j++)
                {
                    if (grid[i, j] != '\0')
                    {
                        row += grid[i, j];
                        row += ' ';
                    }
                    else
                    {
                        row += '-';
                        row += ' ';
                    }
                }
                Console.WriteLine(row + "\n");
                row = "";
            }
        }

        private bool InvalidDimension(int dimensionx, int dimensiony)
        {
            return !(dimensionx >= 2 && dimensiony >= 2);
        }
        private bool IsCellEmpty(User user, int x, int y)
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

        private bool IsLetterCorrect(char letter)
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

        private bool IsOutOfSize(User user, int x, int y)
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

        private void SetTurnOwner()
        {
            var opponent = GetOpponent();
            TurnOwner = opponent;
        }

        private int GetScore(List<string> meaningfulWords)
        {
            List<char> meaningfulLetter = new List<char>();

            for (int i = 0; i < meaningfulWords.Count; i++)
            {
                for (int j = 0; j < meaningfulWords[i].Length; j++)
                {
                    meaningfulLetter.Add(meaningfulWords[i][j]);
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

            return playerScore;
        }

        private List<string> GetMeaningfulWords(char[,] grid)
        {
            UserResult userResult = new UserResult();
            string word = "";
            List<string> meaningfulwords = new List<string>();

            for (int i = 0; i < grid.GetLength(0); i++)
            {
                for (int j = 0; j < grid.GetLength(1); j++)
                {
                    char c = grid[i, j];
                    word += c.ToString();

                    if (Words.Contains(word))
                    {
                        meaningfulwords.Add(word);
                    }
                }

                word = "";
            }

            for (int i = 0; i < grid.GetLength(0); i++)
            {
                for (int j = 0; j < grid.GetLength(1); j++)
                {
                    char c = grid[j, i];
                    word += c.ToString();

                    if (Words.Contains(word))
                    {
                        meaningfulwords.Add(word);
                    }
                }

                word = "";
            }

            return meaningfulwords;
        }

        private void LoadLetters()
        {
            if (LettersPoints.Count > 0)
            {
                return;
            }

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
            if (Words.Count > 0)
            {
                return;
            }

            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Words.txt");
            string text = File.ReadAllText(path);

            string[] words = text.Split('\n', '\r');

            for (int i = 0; i < words.Length; i += 2)
            {
                string[] word = words[i].ToUpper().Split(' ');

                Words.Add(word[0]);
            }
        }
    }
}
