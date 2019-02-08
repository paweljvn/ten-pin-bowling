using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TenPinBowling
{
    class Program
    {
        const int MAX_PLAYERS = 6;
        const int ROUNDS = 10;
        const int PINS = 10;
        const bool USE_RAND = true; // todo set it from start menu
        const int BONUS = 10;
        #region Score table constants
        static readonly int WIDTH = Console.LargestWindowWidth - 5;
        const int PLAYER_COL_WIDTH_RATIO = 14;
        const int FRAME_COL_WITH_RATIO = 8;
        const int TOTAL_COL_WIDTH_RATIO = 6;
        const string PLAYER_NAME = "Player Name";
        const string TOTAL = "Total";
        const string HORIZONTAL_LINE = "-";
        const string VERTICAL_LINE = "|";
        const string BLANK = " ";
        #endregion

        #region String Constants
        const string BACKSPACE = " \b";
        const string SCORE_STRING = " {0}|{1}";
        const string SPARE_STRING = " {0}|{1}({2})Spare";
        const string STRIKE_STRING = " {0}({1}){2}";
        #endregion
        static Random superExtraPinsKnockingDevice = new Random();

        // todo add more strike names, remember to add it to IsStrike()
        enum SpecialScore
        {
            None,
            Spare,
            Strike,
            Rhino,
            Turkey
        }

        struct Score
        {
            public int firstThrow;
            public int secondThrow;
            public int bonusPoints;
            public SpecialScore scoreEvent;
        }

        struct Winner
        {
            public string name;
            public int total;
        }

        static void DrawPlayersSetupMenu(ICollection<String> usersNames)
        {
            Console.WriteLine("Please provide player names.");
            Console.WriteLine("After typing player name press ENTER.");
            Console.WriteLine("Note that you are able to add up to " + MAX_PLAYERS + " players.");
            Console.WriteLine("If you are done press ESC");
            if (usersNames.Count > 0)
            {
                Console.WriteLine("Users: " + String.Join(", ", usersNames));
            }
        }

        static IDictionary<int, String> SetupPlayers()
        {
            IDictionary<int, String> players = new Dictionary<int, String>();

            Console.Clear();
            DrawPlayersSetupMenu(players.Values);

            IList<char> nameChars = new List<char>();
            int playerId = 0;

            Console.Write(playerId + 1 + ". ");
            ConsoleKeyInfo keyInfo = Console.ReadKey();

            while (!ConsoleKey.Escape.Equals(keyInfo.Key))
            {
                if (ConsoleKey.Enter.Equals(keyInfo.Key))
                {
                    if (nameChars.Count == 0)
                    {
                        // if no player name was given then don't do anything
                        continue;
                    }
                    String playerName = String.Join("", nameChars.ToArray());
                    players.Add(playerId, playerName);
                    nameChars.Clear();
                    Console.Clear();
                    playerId++;
                    if (playerId >= MAX_PLAYERS)
                    {
                        break;
                    }
                    DrawPlayersSetupMenu(players.Values);
                    Console.Write(playerId + 1 + ". ");
                }
                else if (ConsoleKey.Backspace == keyInfo.Key)
                {
                    Console.Write(BACKSPACE);
                    nameChars.RemoveAt(nameChars.Count - 1);
                }
                else
                {
                    nameChars.Add(keyInfo.KeyChar);
                }
                keyInfo = Console.ReadKey();
            }

            return players;
        }

        static IDictionary<int, IDictionary<int, Score>> InitPlayersScores(int playersCount)
        {
            IDictionary<int, IDictionary<int, Score>> playersScores = new Dictionary<int, IDictionary<int, Score>>();
            for (int p = 0; p < playersCount; p++)
            {
                IDictionary<int, Score> rounds = new Dictionary<int, Score>();
                for (int r = 0; r < ROUNDS; r++)
                {
                    rounds.Add(r, new Score());
                }
                playersScores.Add(p, rounds);
            }
            return playersScores;
        }

        static void DrawScoreTable(IDictionary<int, String> players, IDictionary<int, IDictionary<int, Score>> playersScores)
        {
            Console.Clear();
            DrawTableHeader();
            for (int i = 0; i < players.Count; i++)
            {
                DrawPlayerRow(i + 1, players[i], playersScores[i]);
            }
        }

        static bool IsStrike(Score score)
        {
            SpecialScore scoreEvent = score.scoreEvent;
            return scoreEvent == SpecialScore.Strike
                || scoreEvent == SpecialScore.Turkey
                || scoreEvent == SpecialScore.Rhino;
        }

        static bool IsSpare(Score score)
        {
            return score.scoreEvent == SpecialScore.Spare;
        }

        static int FirstThrow()
        {
            return ThrowBall(PINS);
        }

        static int SecondThrow(int pinsLeft)
        {
            return ThrowBall(pinsLeft);
        }

        static int ExtraThrow()
        {
            return FirstThrow();
        }

        static int ThrowBall(int pinsToKnockDown)
        {
            if (USE_RAND)
            {
                return superExtraPinsKnockingDevice.Next(0, pinsToKnockDown + 1);
            }
            return HandleManualThrow(pinsToKnockDown);
        }

        static int HandleManualThrow(int pinsToKnockDown)
        {
            Console.WriteLine("How many pins were knocked down?");
            string pinsProvided = Console.ReadLine();
            int pins = 0;
            bool isNumber;
            isNumber = int.TryParse(pinsProvided, out pins);
            if (!isNumber)
            {
                Console.WriteLine("It wasn't a number. No pins knocked :(");
            }
            return pins;
        }

        static bool AllPinsKnocked(int pinsKnocked)
        {
            return PINS - pinsKnocked == 0;
        }

        static Winner GetWinner(IDictionary<int, String> players, IDictionary<int, IDictionary<int, Score>> playersScores)
        {
            Winner winner = new Winner();
            winner.total = -1;
            // todo draw case
            for (int p = 0; p < players.Count; p++)
            {
                int total = 0;
                for (int i = 0; i < ROUNDS; i++)
                {
                    int firstThrow = playersScores[p][i].firstThrow;
                    int secondThrow = playersScores[p][i].secondThrow;
                    int bonus = playersScores[p][i].bonusPoints;
                    total += firstThrow + secondThrow + bonus;
                }
                if (total > winner.total)
                {
                    winner.total = total;
                    winner.name = players[p];
                }
            }
            return winner;
        }

        static SpecialScore GetStrike(int previusRound, IDictionary<int, Score> playerScores)
        {
            if (IsStrike(playerScores[previusRound]))
            {
                return GetNextEvent(playerScores[previusRound].scoreEvent);
            }
            return SpecialScore.Strike;
        }

        static bool IsLastRound(int round)
        {
            return round + 1 == ROUNDS;
        }

        static void CountStrikes(int currentRound, IDictionary<int, Score> playerScores)
        {
            if (currentRound <= 0)
            {
                DrawUnexpectedError();
            }
            int firstArg = playerScores[currentRound].firstThrow;
            int secondArg = playerScores[currentRound].secondThrow;
            int prevRound = currentRound - 1;
            Score score = playerScores[prevRound];
            while (IsStrike(score))
            {
                score.firstThrow = BONUS;
                score.bonusPoints = firstArg + secondArg;
                score.secondThrow = 0;
                playerScores[prevRound] = score;
                secondArg = firstArg;
                firstArg = BONUS;
                prevRound--;
                if (prevRound < 0)
                {
                    break;
                }
                score = playerScores[prevRound];
            }
        }

        static void HandleStrikeInLastRound(IDictionary<int, Score> playerScores)
        {
            int extraPins = ExtraThrow();
            Score score = playerScores[ROUNDS - 1];
            score.firstThrow = extraPins;
            if (AllPinsKnocked(extraPins))
            {
                score.secondThrow = extraPins;
                score.bonusPoints = extraPins;
            }
            playerScores[ROUNDS - 1] = score;
        }

        static void CountSpares(int currentRound, IDictionary<int, Score> playerScores)
        {
            if (currentRound <= 0)
            {
                DrawUnexpectedError();
            }
            int nextBall = playerScores[currentRound].firstThrow;
            Score score = playerScores[currentRound - 1];
            score.bonusPoints = nextBall;
            playerScores[currentRound - 1] = score;
        }

        static void PlayBowling(IDictionary<int, String> players, IDictionary<int, IDictionary<int, Score>> playersScores)
        {
            for (int round = 0; round < ROUNDS; round++)
            {
                for (int playerId = 0; playerId < players.Count; playerId++)
                {
                    DrawScoreTable(players, playersScores);
                    DisplayActualPlayer(players[playerId]);
                    int previousRound = round > 0 ? round - 1 : 0;
                    bool previousStrike = IsStrike(playersScores[playerId][previousRound]);
                    bool previousSpare = IsSpare(playersScores[playerId][previousRound]);

                    if (previousSpare && previousStrike)
                    {
                        DrawUnexpectedError();
                    }

                    int pinsKnockedFirstThrow = FirstThrow();

                    Score score = playersScores[playerId][round];

                    if (AllPinsKnocked(pinsKnockedFirstThrow))
                    {
                        score.scoreEvent = GetStrike(previousRound, playersScores[playerId]);
                        if (IsLastRound(round))
                        {
                            HandleStrikeInLastRound(playersScores[playerId]);
                        }
                    }
                    else
                    {
                        score.firstThrow = pinsKnockedFirstThrow;
                        int pinsKnockedSecondThrow = SecondThrow(PINS - pinsKnockedFirstThrow);
                        score.secondThrow = pinsKnockedSecondThrow;
                        if (AllPinsKnocked(pinsKnockedFirstThrow + pinsKnockedSecondThrow))
                        {
                            score.scoreEvent = SpecialScore.Spare;
                            if (IsLastRound(round))
                            {
                                score.bonusPoints = ExtraThrow();
                            }
                        }
                    }
                    playersScores[playerId][round] = score;

                    if (previousStrike)
                    {
                        CountStrikes(round, playersScores[playerId]);
                    }
                    if (previousSpare)
                    {
                        CountSpares(round, playersScores[playerId]);
                    }
                }
            }
            // todo "play again?" prompt
            DrawScoreTable(players, playersScores);
            DisplayEndGame(GetWinner(players, playersScores));
        }

        static void Main(string[] args)
        {
            Console.SetWindowPosition(0, 0);
            Console.SetWindowSize(Console.LargestWindowWidth, Console.LargestWindowHeight);
            Console.WriteLine("Let's Play Bowling!");

            Console.WriteLine("Press ENTER to Start");
            Console.ReadLine(); // wait for User to press enter

            // PlayerId, PlayerName
            IDictionary<int, String> players = SetupPlayers();

            if (players.Count == 0)
            {
                Environment.Exit(0);
            }

            // PlayerId, Round, Score
            IDictionary<int, IDictionary<int, Score>> playersScores = InitPlayersScores(players.Count);

            PlayBowling(players, playersScores);
            // todo play again prompt

            Console.ReadKey();
        }

        #region Helper Methods

        static SpecialScore GetNextEvent(SpecialScore events)
        {
            bool shouldTakeNext = false;
            foreach (SpecialScore eventItem in Enum.GetValues(typeof(SpecialScore)))
            {
                if (shouldTakeNext)
                {
                    return eventItem;
                }
                if (events == eventItem)
                {
                    shouldTakeNext = true;
                }
            }
            return events;
        }

        static void DrawUnexpectedError()
        {
            // todo make it fun :P 
            Console.Clear();
            Console.WriteLine("Holy Cow !!!! Something bad happend !!!! Brace yourself ....");
            Console.ReadKey();
        }

        #endregion

        #region Base Score Table Drawing

        static void DrawPlayerRow(int playerNumber, String playerName, IDictionary<int, Score> playerScores)
        {
            // todo draw player number
            DrawPlayerName(playerName);
            int total = 0;
            for (int i = 0; i < ROUNDS; i++)
            {
                Score score = playerScores[i];
                if (IsStrike(score))
                {
                    DrawStrike(score.firstThrow, score.bonusPoints, score.scoreEvent);
                }
                else if (IsSpare(score))
                {
                    DrawSpare(score.firstThrow, score.secondThrow, score.bonusPoints);
                }
                else
                {
                    DrawFrameScore(score.firstThrow, score.secondThrow);
                }
                total += score.firstThrow + score.secondThrow + score.bonusPoints;
            }
            DrawTotalScore(total);
            DrawFullHorizontalLine();
        }

        static void DrawTableHeader()
        {
            DrawFullHorizontalLine();
            DrawPlayerHeader();
            DrawRoundFramesHeader();
            DrawTotalHeader();
            DrawFullHorizontalLine();
        }

        static void DrawPlayerHeader()
        {
            DrawStringAndFillWithBlanks(BLANK + PLAYER_NAME, PLAYER_COL_WIDTH_RATIO);
        }

        static void DrawPlayerName(String playerName)
        {
            DrawStringAndFillWithBlanks(BLANK + playerName, PLAYER_COL_WIDTH_RATIO);
        }

        static void DrawRoundFramesHeader()
        {
            for (int i = 1; i <= ROUNDS; i++)
            {
                DrawStringAndFillWithBlanks(BLANK + i, FRAME_COL_WITH_RATIO);
            }
        }

        static void DrawFrameScore(int firstThrow, int secondThrow)
        {
            DrawStringAndFillWithBlanks(String.Format(SCORE_STRING, firstThrow, secondThrow),
                FRAME_COL_WITH_RATIO);
        }

        static void DrawSpare(int firstThrow, int secondThrow, int bonus)
        {
            DrawStringAndFillWithBlanks(String.Format(SPARE_STRING, firstThrow, secondThrow, bonus),
                FRAME_COL_WITH_RATIO);
        }

        static void DrawStrike(int score, int bonus, SpecialScore strike)
        {
            DrawStringAndFillWithBlanks(String.Format(STRIKE_STRING, score, bonus, strike.ToString()),
                FRAME_COL_WITH_RATIO);
        }

        static void DrawTotalHeader()
        {
            DrawStringAndFillWithBlanks(BLANK + TOTAL, TOTAL_COL_WIDTH_RATIO);
            DrawVerticalLine();
        }

        static void DrawTotalScore(int total)
        {
            DrawStringAndFillWithBlanks(BLANK + total, TOTAL_COL_WIDTH_RATIO);
            DrawVerticalLine();
        }

        #endregion

        #region Base shapes drawing

        static int DrawHorizontal(String toDraw, int times)
        {
            for (int i = 0; i < times; i++)
            {
                Console.Write(toDraw);
            }
            return times;
        }

        static int DrawHorizontal(String toDraw)
        {
            Console.Write(toDraw);
            return toDraw.Length;
        }

        static void DrawFullHorizontalLine()
        {
            NewLine();
            DrawHorizontal(HORIZONTAL_LINE, WIDTH);
            NewLine();
        }

        static int DrawVerticalLine()
        {
            Console.Write(VERTICAL_LINE);
            return 1;
        }

        static void NewLine()
        {
            Console.WriteLine();
        }

        static void DrawStringAndFillWithBlanks(String str, int ratio)
        {
            int charsLeft = WIDTH * ratio / 100;
            charsLeft -= DrawVerticalLine();
            charsLeft -= DrawHorizontal(str);
            charsLeft -= DrawHorizontal(BLANK, charsLeft);
        }
        #endregion

        #region UI display

        static void DisplayActualPlayer(String playerName)
        {
            Console.WriteLine(playerName + " is now playing");
        }

        static void DisplayEndGame(Winner winner)
        {
            Console.WriteLine("Game ended! The winner is " + winner.name + " with score: " + winner.total);
        }

        #endregion
    }
}
