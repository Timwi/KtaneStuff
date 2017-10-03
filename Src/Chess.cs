using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace KtaneStuff
{
    static class Chess
    {
        private enum ChessPieceType
        {
            Knight,
            Queen,
            King,
            Bishop,
            Rook
        }
        private enum BoardState
        {
            Empty,
            Covered,
            Filled
        }

        private class ChessPiece
        {
            public int xCoord, yCoord;
            public ChessPieceType piece;

            public static bool getWhite(int x, int y)
            {
                return x % 2 != y % 2;
            }

            public ChessPiece(int x, int y, ChessPieceType p, ref BoardState[,] board)
            {
                xCoord = x;
                yCoord = y;
                piece = p;
                board[x, y] = BoardState.Filled;
            }
            public static void getRookTouchable(int x, int y, ref BoardState[,] myList, bool tempKing = false)
            {


                for (int i = x + 1; i < 6; i++)
                {
                    if (myList[i, y] == BoardState.Empty)
                    {
                        myList[i, y] = BoardState.Covered;
                    }
                    else if (myList[i, y] == BoardState.Filled)
                    {
                        break;
                    }
                    if (tempKing) break;

                }

                for (int i = x - 1; i >= 0; i--)
                {
                    if (myList[i, y] == BoardState.Empty)
                    {
                        myList[i, y] = BoardState.Covered;
                    }
                    else if (myList[i, y] == BoardState.Filled)
                    {
                        break;
                    }
                    if (tempKing) break;

                }

                for (int i = y + 1; i < 6; i++)
                {
                    if (myList[x, i] == BoardState.Empty)
                    {
                        myList[x, i] = BoardState.Covered;
                    }
                    else if (myList[x, i] == BoardState.Filled)
                    {
                        break;
                    }
                    if (tempKing) break;

                }

                for (int i = y - 1; i >= 0; i--)
                {
                    if (myList[x, i] == BoardState.Empty)
                    {
                        myList[x, i] = BoardState.Covered;
                    }
                    else if (myList[x, i] == BoardState.Filled)
                    {
                        break;
                    }
                    if (tempKing) break;

                }
            }

            public static void getBishopTouchable(int x, int y, ref BoardState[,] myList, bool tempKing = false)
            {


                for (int i = x + 1, j = y + 1; j < 6 && i < 6; i++, j++)
                {
                    if (myList[i, j] == BoardState.Empty)
                    {
                        myList[i, j] = BoardState.Covered;
                    }
                    else if (myList[i, j] == BoardState.Filled)
                    {
                        break;
                    }
                    if (tempKing) break;

                }

                for (int i = x - 1, j = y + 1; j < 6 && i >= 0; i--, j++)
                {
                    if (myList[i, j] == BoardState.Empty)
                    {
                        myList[i, j] = BoardState.Covered;
                    }
                    else if (myList[i, j] == BoardState.Filled)
                    {
                        break;
                    }
                    if (tempKing) break;

                }

                for (int i = x + 1, j = y - 1; j >= 0 && i < 6; i++, j--)
                {
                    if (myList[i, j] == BoardState.Empty)
                    {
                        myList[i, j] = BoardState.Covered;
                    }
                    else if (myList[i, j] == BoardState.Filled)
                    {
                        break;
                    }
                    if (tempKing) break;

                }

                for (int i = x - 1, j = y - 1; j >= 0 && i >= 0; i--, j--)
                {
                    if (myList[i, j] == BoardState.Empty)
                    {
                        myList[i, j] = BoardState.Covered;
                    }
                    else if (myList[i, j] == BoardState.Filled)
                    {
                        break;
                    }
                    if (tempKing) break;

                }
            }


            public static void getQueenTouchable(int x, int y, ref BoardState[,] myList)
            {
                getRookTouchable(x, y, ref myList);
                getBishopTouchable(x, y, ref myList);
            }

            public static void getKnightTouchable(int x, int y, ref BoardState[,] myList)
            {
                int newX, newY;
                newX = x + 2;
                newY = y + 1;
                if (newX >= 0 && newX < 6 && newY < 6 && newY >= 0 && myList[newX, newY] == BoardState.Empty)
                {
                    myList[newX, newY] = BoardState.Covered;
                }
                newX = x - 2;
                newY = y + 1;
                if (newX >= 0 && newX < 6 && newY < 6 && newY >= 0 && myList[newX, newY] == BoardState.Empty)
                {
                    myList[newX, newY] = BoardState.Covered;
                }
                newX = x + 2;
                newY = y - 1;
                if (newX >= 0 && newX < 6 && newY < 6 && newY >= 0 && myList[newX, newY] == BoardState.Empty)
                {
                    myList[newX, newY] = BoardState.Covered;
                }
                newX = x - 2;
                newY = y - 1;
                if (newX >= 0 && newX < 6 && newY < 6 && newY >= 0 && myList[newX, newY] == BoardState.Empty)
                {
                    myList[newX, newY] = BoardState.Covered;
                }
                newX = x + 1;
                newY = y + 2;
                if (newX >= 0 && newX < 6 && newY < 6 && newY >= 0 && myList[newX, newY] == BoardState.Empty)
                {
                    myList[newX, newY] = BoardState.Covered;
                }
                newX = x - 1;
                newY = y + 2;
                if (newX >= 0 && newX < 6 && newY < 6 && newY >= 0 && myList[newX, newY] == BoardState.Empty)
                {
                    myList[newX, newY] = BoardState.Covered;
                }
                newX = x + 1;
                newY = y - 2;
                if (newX >= 0 && newX < 6 && newY < 6 && newY >= 0 && myList[newX, newY] == BoardState.Empty)
                {
                    myList[newX, newY] = BoardState.Covered;
                }
                newX = x - 1;
                newY = y - 2;
                if (newX >= 0 && newX < 6 && newY < 6 && newY >= 0 && myList[newX, newY] == BoardState.Empty)
                {
                    myList[newX, newY] = BoardState.Covered;
                }

            }

            public static void getKingTouchable(int x, int y, ref BoardState[,] myList)
            {
                getRookTouchable(x, y, ref myList, true);
                getBishopTouchable(x, y, ref myList, true);
            }


            public void getTouchable(ref BoardState[,] myList)
            {
                switch (piece)
                {
                    case ChessPieceType.Queen:
                        getQueenTouchable(xCoord, yCoord, ref myList);
                        break;
                    case ChessPieceType.King:
                        getKingTouchable(xCoord, yCoord, ref myList);
                        break;
                    case ChessPieceType.Bishop:
                        getBishopTouchable(xCoord, yCoord, ref myList);
                        break;
                    case ChessPieceType.Knight:
                        getKnightTouchable(xCoord, yCoord, ref myList);
                        break;
                    case ChessPieceType.Rook:
                        getRookTouchable(xCoord, yCoord, ref myList);
                        break;
                }
            }

        }

        public static bool TestRand(ref string[][] generatedPairs, int Sign, int MyModuleId, out string BoardString)
        {
            BoardString = "";
            string[] RandPieces = new string[6];
            string TestString = "";
            int ji = 0;
            while (ji < 6)
            {
                string temp = (char) (Rnd.Next(0, 6) + 'a') + "" + (Rnd.Next(0, 6) + 1);
                if (!TestString.Contains(temp))
                {
                    RandPieces[ji] = temp;
                    TestString += temp;
                    ji++;
                }
            }
            BoardState[,] chessBoard = new BoardState[6, 6];
            ChessPiece[] myPieces;
            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    chessBoard[i, j] = BoardState.Empty;
                }
            }

            myPieces = new ChessPiece[6];
            myPieces[1] = new ChessPiece((RandPieces[1][0]) - 'a', (RandPieces[1][1]) - '1', (Sign == 0 ? ChessPieceType.Knight : ChessPieceType.Rook), ref chessBoard);
            myPieces[3] = new ChessPiece(RandPieces[3][0] - 'a', RandPieces[3][1] - '1', ChessPieceType.Rook, ref chessBoard);
            if (ChessPiece.getWhite(RandPieces[4][0] - 'a', RandPieces[4][1] - '1'))
            {
                myPieces[4] = new ChessPiece(RandPieces[4][0] - 'a', RandPieces[4][1] - '1', ChessPieceType.Queen, ref chessBoard);
                myPieces[0] = new ChessPiece(RandPieces[0][0] - 'a', RandPieces[0][1] - '1', ChessPieceType.King, ref chessBoard);
            }
            else
            {
                myPieces[4] = new ChessPiece(RandPieces[4][0] - 'a', RandPieces[4][1] - '1', ChessPieceType.Rook, ref chessBoard);
                myPieces[0] = new ChessPiece(RandPieces[0][0] - 'a', RandPieces[0][1] - '1', ChessPieceType.Bishop, ref chessBoard);
            }

            if (Sign == 1 || myPieces[4].piece == ChessPieceType.Rook)
            {
                myPieces[2] = new ChessPiece(RandPieces[2][0] - 'a', RandPieces[2][1] - '1', ChessPieceType.King, ref chessBoard);
            }
            else
            {
                myPieces[2] = new ChessPiece(RandPieces[2][0] - 'a', RandPieces[2][1] - '1', ChessPieceType.Queen, ref chessBoard);
            }

            bool hasQueen = false;
            bool hasKnight = false;
            for (int i = 0; i < 5; i++)
            {
                if (myPieces[i].piece == ChessPieceType.Queen)
                {
                    hasQueen = true;
                }
                if (myPieces[i].piece == ChessPieceType.Knight)
                {
                    hasKnight = true;
                }
            }
            if (!hasQueen)
            {
                myPieces[5] = new ChessPiece(RandPieces[5][0] - 'a', RandPieces[5][1] - '1', ChessPieceType.Queen, ref chessBoard);
            }
            else if (!hasKnight)
            {
                myPieces[5] = new ChessPiece(RandPieces[5][0] - 'a', RandPieces[5][1] - '1', ChessPieceType.Knight, ref chessBoard);
            }
            else
            {
                myPieces[5] = new ChessPiece(RandPieces[5][0] - 'a', RandPieces[5][1] - '1', ChessPieceType.Bishop, ref chessBoard);
            }
            foreach (ChessPiece i in myPieces)
            {
                i.getTouchable(ref chessBoard);
            }

            int empty = 0;
            List<string> emptyArr = new List<string>();
            for (int i = 5; i >= 0 && empty < 2; i--)
            {

                for (int j = 0; j < 6 && empty < 2; j++)
                {
                    if (chessBoard[j, i] == BoardState.Empty)
                    {
                        empty++;
                        emptyArr.Add((char) (j + 'a') + "" + (i + 1));
                    }

                }
            }
            if (empty == 1)
            {
                for (int i = 0; i < 6; i++)
                {
                    generatedPairs[Sign][i] = RandPieces[i];
                }
                generatedPairs[Sign][6] = emptyArr[0];
                int[] Numbers = new int[6];
                int SolutionNumber;
                {
                    string s1 = generatedPairs[Sign][6];
                    SolutionNumber = ((s1[0] - 'a')) + 6 * (s1[1] - '1');
                    for (int i = 0; i < 6; i++)
                    {
                        s1 = generatedPairs[Sign][i];
                        Numbers[i] = ((s1[0] - 'a')) + 6 * (s1[1] - '1');
                    }
                }
                StringBuilder s = new StringBuilder("┌───┬───┬───┬───┬───┬───┐");

                for (int i = 0; i < 11; i++)
                {
                    s.Append("\n");
                    if (i % 2 == 0)
                    {
                        int v = 5 - (i / 2);
                        for (int j = 0; j < 6; j++)
                        {
                            s.Append("│");
                            int o = Array.FindIndex(Numbers, x => x == v * 6 + j);
                            s.Append(" ");
                            if (o != -1)
                            {
                                ChessPieceType c = myPieces[o].piece;
                                char pieceChar = ' ';
                                switch (c)
                                {
                                    case ChessPieceType.Bishop:
                                        pieceChar = 'B';
                                        break;
                                    case ChessPieceType.King:
                                        pieceChar = 'K';
                                        break;
                                    case ChessPieceType.Queen:
                                        pieceChar = 'Q';
                                        break;
                                    case ChessPieceType.Knight:
                                        pieceChar = 'N';
                                        break;
                                    case ChessPieceType.Rook:
                                        pieceChar = 'R';
                                        break;
                                }
                                s.Append(pieceChar);
                            }
                            else
                            {

                                if (v * 6 + j == SolutionNumber)
                                    s.Append('×');
                                else
                                    s.Append(" ");

                            }
                            s.Append(" ");
                        }
                        s.Append("│");
                    }
                    else
                    {
                        s.Append("├───┼───┼───┼───┼───┼───┤");
                    }
                }
                s.Append("\n");
                s.Append("└───┴───┴───┴───┴───┴───┘");

                BoardString = s.ToString();

                return true;
            }
            return false;
        }

        public static string[] GetSolution(bool serialNumberOdd, out int numAttempts, out string boardString)
        {
            numAttempts = 1;
            var generatedPairs = new string[2][];
            for (int i = 0; i < 2; i++)
                generatedPairs[i] = new string[7];
            while (!TestRand(ref generatedPairs, serialNumberOdd ? 1 : 0, 0, out boardString))
                numAttempts++;
            return generatedPairs[serialNumberOdd ? 1 : 0];
        }

        public static void Practice()
        {
            while (true)
            {
                var odd = Rnd.Next(0, 2) == 0;
                var result = GetSolution(odd, out var numAttempts, out string board);
                Console.WriteLine(odd ? "Serial number is odd." : "Serial number is even.");
                Console.WriteLine(result.Take(6).JoinString(" | "));
                Console.WriteLine("Answer?");

                var correct = false;
                for (int attempt = 0; attempt < 3 && !correct; attempt++)
                {
                    var answer = Console.ReadLine();
                    if (answer == "exit")
                        return;

                    if (answer == result[6])
                        correct = true;
                    else if (attempt < 2)
                        Console.WriteLine("Wrong, try again?");
                }
                Console.WriteLine(correct ? "Correct." : "Wrong, should have been " + result[6]);
                Console.WriteLine(board);
                Console.WriteLine(new string('─', 50));
            }
        }
    }
}
