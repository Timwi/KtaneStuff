using System;
using System.Collections.Generic;
using System.Linq;
using RT.Util.ExtensionMethods;
using RT.Util.Text;

namespace KtaneStuff
{
    static class Tennis
    {
        sealed class SetScores
        {
            public int Player1Score { get; private set; }
            public int Player2Score { get; private set; }
            public SetScores() { Player1Score = 0; Player2Score = 0; }
            public SetScores(int player1Score, int player2Score) { Player1Score = player1Score; Player2Score = player2Score; }
            public int ScoreOf(bool player1) { return player1 ? Player1Score : Player2Score; }
            public SetScores Inc(bool player1) { return new SetScores(Player1Score + (player1 ? 1 : 0), Player2Score + (player1 ? 0 : 1)); }
            public override string ToString() { return $"[{Player1Score}-{Player2Score}]"; }
        }
        abstract class GameState
        {
            public static GameState GetInitial(bool isMensPlay, bool isWimbledon) { return new GameStateScores(isMensPlay, isWimbledon, new SetScores[] { new SetScores() }); }
        }
        sealed class GameStateScores : GameState
        {
            public SetScores[] Sets { get; private set; }

            // In normal play: [0,2] = 0–30; or [3,3] = 40–40; [4,3] = advantage Player 1; [4,4] = deuce
            // In tie break: normal tiebreak scores
            public int Player1Score { get; private set; }
            public int Player2Score { get; private set; }
            public bool IsTieBreak { get; private set; }

            public bool IsMensPlay { get; private set; }
            public bool IsWimbledon { get; private set; }

            public GameStateScores(bool isMensPlay, bool isWimbledon, SetScores[] sets) { IsMensPlay = isMensPlay; IsWimbledon = isWimbledon; Sets = sets; }
            public GameStateScores(bool isMensPlay, bool isWimbledon, SetScores[] sets, int player1Score, int player2Score, bool tiebreak = false) { IsMensPlay = isMensPlay; IsWimbledon = isWimbledon; Sets = sets; Player1Score = player1Score; Player2Score = player2Score; IsTieBreak = tiebreak; }

            public bool IsPlayer1Serving
            {
                get
                {
                    return (Sets.Sum(set => set.Player1Score + set.Player2Score) % 2 == 0) ^ (IsTieBreak && (Player1Score + Player2Score + 1) % 4 >= 2);
                }
            }
            public GameState PlayerScores(bool server)
            {
                var isPlayer1 = !(server ^ IsPlayer1Serving);
                var thisPlayer = isPlayer1 ? Player1Score : Player2Score;
                var otherPlayer = isPlayer1 ? Player2Score : Player1Score;

                // Does player win a game?
                if ((IsTieBreak && thisPlayer >= 6 && thisPlayer > otherPlayer) ||  // winning a tie break
                    (!IsTieBreak && thisPlayer == 3 && otherPlayer < 3) ||  // winning from 40–0 to 40–30
                    (!IsTieBreak && thisPlayer == 4 && otherPlayer == 3))   // winning from Advantage
                {
                    // Does player win a set?
                    var thisPlayerSet = isPlayer1 ? Sets.Last().Player1Score : Sets.Last().Player2Score;
                    var otherPlayerSet = isPlayer1 ? Sets.Last().Player2Score : Sets.Last().Player1Score;
                    if ((thisPlayerSet >= 5 && thisPlayerSet > otherPlayerSet) || IsTieBreak)
                    {
                        // Does player win the match?
                        if (Sets.Take(Sets.Length - 1).Count(set => thisPlayerSet > otherPlayerSet) + 1 >= (IsMensPlay ? 3 : 2))
                            return new GameStateVictory { Player1Wins = isPlayer1 };

                        // Just the set
                        return new GameStateScores(IsMensPlay, IsWimbledon, Sets.Take(Sets.Length - 1).Concat(new[] { Sets.Last().Inc(isPlayer1), new SetScores() }).ToArray());
                    }

                    // Just the game.
                    // Does this start a tie break?
                    if ((!IsWimbledon || Sets.Length < (IsMensPlay ? 5 : 3)) && thisPlayerSet + 1 == 6 && otherPlayerSet == 6)
                        return new GameStateScores(IsMensPlay, IsWimbledon, Sets.Take(Sets.Length - 1).Concat(new[] { Sets.Last().Inc(isPlayer1) }).ToArray(), 0, 0, tiebreak: true);
                    return new GameStateScores(IsMensPlay, IsWimbledon, Sets.Take(Sets.Length - 1).Concat(new[] { Sets.Last().Inc(isPlayer1) }).ToArray());
                }

                // Just a point. Are we going from Deuce to Advantage?
                if (thisPlayer == 4 && otherPlayer == 4 && !IsTieBreak)
                    return new GameStateScores(IsMensPlay, IsWimbledon, Sets, isPlayer1 ? 4 : 3, isPlayer1 ? 3 : 4);
                return new GameStateScores(IsMensPlay, IsWimbledon, Sets, Player1Score + (isPlayer1 ? 1 : 0), Player2Score + (isPlayer1 ? 0 : 1), IsTieBreak);
            }

            private static readonly string[] ScoreNames = new[] { "0", "15", "30", "40" };
            public override string ToString()
            {
                if (IsTieBreak)
                    return $"•P{(IsPlayer1Serving ? "1" : "2")} {Sets.JoinString(" ")} Tie break {Player1Score}-{Player2Score}";
                if (Player1Score == 4 && Player2Score == 4)
                    return $"•P{(IsPlayer1Serving ? "1" : "2")} {Sets.JoinString(" ")} Deuce";
                if (Player1Score == 4)
                    return $"•P{(IsPlayer1Serving ? "1" : "2")} {Sets.JoinString(" ")} Advantage Player 1";
                if (Player2Score == 4)
                    return $"•P{(IsPlayer1Serving ? "1" : "2")} {Sets.JoinString(" ")} Advantage Player 2";
                return $"•P{(IsPlayer1Serving ? "1" : "2")} {Sets.JoinString(" ")} {ScoreNames[Player1Score]}-{ScoreNames[Player2Score]}";
            }
        }
        sealed class GameStateVictory : GameState
        {
            public bool Player1Wins;
            public override string ToString()
            {
                return $"Player {(Player1Wins ? 1 : 2)} wins.";
            }
        }

        public static void Play()
        {
            var tt = new TextTable { ColumnSpacing = 2 };
            var victories = 0;
            var isMensPlay = true;
            for (int seed = 0; seed < 1; seed++)
            {
                var rnd = new Random(seed);

                // Generate random starting point
                var max = isMensPlay ? rnd.Next(110, 150) : rnd.Next(65, 80);
                tryAgain:
                var initialState = GameState.GetInitial(isMensPlay: isMensPlay, isWimbledon: true);
                for (int i = 0; i < max; i++)
                {
                    initialState = ((GameStateScores) initialState).PlayerScores(rnd.Next(0, 2) == 1);
                    if (initialState is GameStateVictory)
                    {
                        max /= 2;
                        goto tryAgain;
                    }
                }

                var state = initialState;
                var edgework = Edgework.Generate(rnd: rnd);
                var binary = new List<bool>();
                for (int i = 0; i < edgework.SerialNumber.Length; i++)
                {
                    var ch = edgework.SerialNumber[i];
                    int num;
                    if (ch >= '0' && ch <= '9')
                        num = ch - '0';
                    else
                        num = 6 + ch - 'A';
                    binary.AddRange(Enumerable.Range(0, 5).Select(j => ((1 << (4 - j)) & num) != 0));
                }
                for (int i = 0; i < binary.Count && !(state is GameStateVictory); i++)
                {
                    state = ((GameStateScores) state).PlayerScores(binary[i]);
                    Console.WriteLine($"{(binary[i] ? "Ser" : "Opp")} → {state}");
                }
                if (state is GameStateVictory)
                    victories++;

                tt.SetCell(0, seed, max.ToString().Color(ConsoleColor.DarkGreen), alignment: HorizontalTextAlignment.Right);
                tt.SetCell(1, seed, initialState.ToString().Color(ConsoleColor.Magenta));
                tt.SetCell(2, seed, edgework.SerialNumber.Color(ConsoleColor.Cyan));
                tt.SetCell(3, seed, binary.Select(b => b ? "1" : "0").JoinString().Color(ConsoleColor.Blue));
                tt.SetCell(4, seed, state.ToString().Color(ConsoleColor.Green));
            }
            tt.WriteToConsole();
            Console.WriteLine($"{victories} victories");
        }
    }
}