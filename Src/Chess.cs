using System.Text;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace KtaneStuff;

internal static class Chess
{
    private enum chessPieceType
    {
        Knight,
        Queen,
        King,
        Bishop,
        Rook
    }

    private enum boardState
    {
        Empty,
        Covered,
        Filled
    }

    private class chessPiece
    {
        public int XCoord, YCoord;
        public chessPieceType Piece;

        public static bool GetWhite(int x, int y) => x % 2 != y % 2;

        public chessPiece(int x, int y, chessPieceType p, ref boardState[,] board)
        {
            XCoord = x;
            YCoord = y;
            Piece = p;
            board[x, y] = boardState.Filled;
        }

        public static void GetRookTouchable(int x, int y, ref boardState[,] myList, bool tempKing = false)
        {
            for (var i = x + 1; i < 6; i++)
            {
                if (myList[i, y] == boardState.Empty)
                    myList[i, y] = boardState.Covered;
                else if (myList[i, y] == boardState.Filled)
                    break;
                if (tempKing)
                    break;
            }

            for (var i = x - 1; i >= 0; i--)
            {
                if (myList[i, y] == boardState.Empty)
                    myList[i, y] = boardState.Covered;
                else if (myList[i, y] == boardState.Filled)
                    break;
                if (tempKing)
                    break;
            }

            for (var i = y + 1; i < 6; i++)
            {
                if (myList[x, i] == boardState.Empty)
                    myList[x, i] = boardState.Covered;
                else if (myList[x, i] == boardState.Filled)
                    break;
                if (tempKing)
                    break;
            }

            for (var i = y - 1; i >= 0; i--)
            {
                if (myList[x, i] == boardState.Empty)
                    myList[x, i] = boardState.Covered;
                else if (myList[x, i] == boardState.Filled)
                    break;
                if (tempKing)
                    break;
            }
        }

        public static void GetBishopTouchable(int x, int y, ref boardState[,] myList, bool tempKing = false)
        {
            for (int i = x + 1, j = y + 1; j < 6 && i < 6; i++, j++)
            {
                if (myList[i, j] == boardState.Empty)
                    myList[i, j] = boardState.Covered;
                else if (myList[i, j] == boardState.Filled)
                    break;
                if (tempKing)
                    break;
            }

            for (int i = x - 1, j = y + 1; j < 6 && i >= 0; i--, j++)
            {
                if (myList[i, j] == boardState.Empty)
                    myList[i, j] = boardState.Covered;
                else if (myList[i, j] == boardState.Filled)
                    break;
                if (tempKing)
                    break;
            }

            for (int i = x + 1, j = y - 1; j >= 0 && i < 6; i++, j--)
            {
                if (myList[i, j] == boardState.Empty)
                    myList[i, j] = boardState.Covered;
                else if (myList[i, j] == boardState.Filled)
                    break;
                if (tempKing)
                    break;
            }

            for (int i = x - 1, j = y - 1; j >= 0 && i >= 0; i--, j--)
            {
                if (myList[i, j] == boardState.Empty)
                    myList[i, j] = boardState.Covered;
                else if (myList[i, j] == boardState.Filled)
                    break;
                if (tempKing)
                    break;
            }
        }

        public static void GetQueenTouchable(int x, int y, ref boardState[,] myList)
        {
            GetRookTouchable(x, y, ref myList);
            GetBishopTouchable(x, y, ref myList);
        }

        public static void GetKnightTouchable(int x, int y, ref boardState[,] myList)
        {
            int newX, newY;
            newX = x + 2;
            newY = y + 1;
            if (newX >= 0 && newX < 6 && newY < 6 && newY >= 0 && myList[newX, newY] == boardState.Empty)
                myList[newX, newY] = boardState.Covered;
            newX = x - 2;
            newY = y + 1;
            if (newX >= 0 && newX < 6 && newY < 6 && newY >= 0 && myList[newX, newY] == boardState.Empty)
                myList[newX, newY] = boardState.Covered;
            newX = x + 2;
            newY = y - 1;
            if (newX >= 0 && newX < 6 && newY < 6 && newY >= 0 && myList[newX, newY] == boardState.Empty)
                myList[newX, newY] = boardState.Covered;
            newX = x - 2;
            newY = y - 1;
            if (newX >= 0 && newX < 6 && newY < 6 && newY >= 0 && myList[newX, newY] == boardState.Empty)
                myList[newX, newY] = boardState.Covered;
            newX = x + 1;
            newY = y + 2;
            if (newX >= 0 && newX < 6 && newY < 6 && newY >= 0 && myList[newX, newY] == boardState.Empty)
                myList[newX, newY] = boardState.Covered;
            newX = x - 1;
            newY = y + 2;
            if (newX >= 0 && newX < 6 && newY < 6 && newY >= 0 && myList[newX, newY] == boardState.Empty)
                myList[newX, newY] = boardState.Covered;
            newX = x + 1;
            newY = y - 2;
            if (newX >= 0 && newX < 6 && newY < 6 && newY >= 0 && myList[newX, newY] == boardState.Empty)
                myList[newX, newY] = boardState.Covered;
            newX = x - 1;
            newY = y - 2;
            if (newX >= 0 && newX < 6 && newY < 6 && newY >= 0 && myList[newX, newY] == boardState.Empty)
                myList[newX, newY] = boardState.Covered;
        }

        public static void GetKingTouchable(int x, int y, ref boardState[,] myList)
        {
            GetRookTouchable(x, y, ref myList, true);
            GetBishopTouchable(x, y, ref myList, true);
        }

        public void GetTouchable(ref boardState[,] myList)
        {
            switch (Piece)
            {
                case chessPieceType.Queen:
                    GetQueenTouchable(XCoord, YCoord, ref myList);
                    break;
                case chessPieceType.King:
                    GetKingTouchable(XCoord, YCoord, ref myList);
                    break;
                case chessPieceType.Bishop:
                    GetBishopTouchable(XCoord, YCoord, ref myList);
                    break;
                case chessPieceType.Knight:
                    GetKnightTouchable(XCoord, YCoord, ref myList);
                    break;
                case chessPieceType.Rook:
                    GetRookTouchable(XCoord, YCoord, ref myList);
                    break;
            }
        }

    }

    public static bool TestRand(ref string[][] generatedPairs, int sign, out string boardString)
    {
        boardString = "";
        var randPieces = new string[6];
        var testString = "";
        var ji = 0;
        while (ji < 6)
        {
            var temp = (char) (Rnd.Next(0, 6) + 'a') + "" + (Rnd.Next(0, 6) + 1);
            if (!testString.Contains(temp))
            {
                randPieces[ji] = temp;
                testString += temp;
                ji++;
            }
        }
        var chessBoard = new boardState[6, 6];
        chessPiece[] myPieces;
        for (var i = 0; i < 6; i++)
        {
            for (var j = 0; j < 6; j++)
            {
                chessBoard[i, j] = boardState.Empty;
            }
        }

        myPieces = new chessPiece[6];
        myPieces[1] = new chessPiece((randPieces[1][0]) - 'a', (randPieces[1][1]) - '1', (sign == 0 ? chessPieceType.Knight : chessPieceType.Rook), ref chessBoard);
        myPieces[3] = new chessPiece(randPieces[3][0] - 'a', randPieces[3][1] - '1', chessPieceType.Rook, ref chessBoard);
        if (chessPiece.GetWhite(randPieces[4][0] - 'a', randPieces[4][1] - '1'))
        {
            myPieces[4] = new chessPiece(randPieces[4][0] - 'a', randPieces[4][1] - '1', chessPieceType.Queen, ref chessBoard);
            myPieces[0] = new chessPiece(randPieces[0][0] - 'a', randPieces[0][1] - '1', chessPieceType.King, ref chessBoard);
        }
        else
        {
            myPieces[4] = new chessPiece(randPieces[4][0] - 'a', randPieces[4][1] - '1', chessPieceType.Rook, ref chessBoard);
            myPieces[0] = new chessPiece(randPieces[0][0] - 'a', randPieces[0][1] - '1', chessPieceType.Bishop, ref chessBoard);
        }

        myPieces[2] = sign == 1 || myPieces[4].Piece == chessPieceType.Rook
            ? new chessPiece(randPieces[2][0] - 'a', randPieces[2][1] - '1', chessPieceType.King, ref chessBoard)
            : new chessPiece(randPieces[2][0] - 'a', randPieces[2][1] - '1', chessPieceType.Queen, ref chessBoard);

        var hasQueen = false;
        var hasKnight = false;
        for (var i = 0; i < 5; i++)
        {
            if (myPieces[i].Piece == chessPieceType.Queen)
            {
                hasQueen = true;
            }
            if (myPieces[i].Piece == chessPieceType.Knight)
            {
                hasKnight = true;
            }
        }
        if (!hasQueen)
        {
            myPieces[5] = new chessPiece(randPieces[5][0] - 'a', randPieces[5][1] - '1', chessPieceType.Queen, ref chessBoard);
        }
        else
        {
            myPieces[5] = !hasKnight
                ? new chessPiece(randPieces[5][0] - 'a', randPieces[5][1] - '1', chessPieceType.Knight, ref chessBoard)
                : new chessPiece(randPieces[5][0] - 'a', randPieces[5][1] - '1', chessPieceType.Bishop, ref chessBoard);
        }
        foreach (var i in myPieces)
        {
            i.GetTouchable(ref chessBoard);
        }

        var empty = 0;
        var emptyArr = new List<string>();
        for (var i = 5; i >= 0 && empty < 2; i--)
        {

            for (var j = 0; j < 6 && empty < 2; j++)
            {
                if (chessBoard[j, i] == boardState.Empty)
                {
                    empty++;
                    emptyArr.Add((char) (j + 'a') + "" + (i + 1));
                }

            }
        }
        if (empty == 1)
        {
            for (var i = 0; i < 6; i++)
            {
                generatedPairs[sign][i] = randPieces[i];
            }
            generatedPairs[sign][6] = emptyArr[0];
            var Numbers = new int[6];
            int SolutionNumber;
            {
                var s1 = generatedPairs[sign][6];
                SolutionNumber = ((s1[0] - 'a')) + 6 * (s1[1] - '1');
                for (var i = 0; i < 6; i++)
                {
                    s1 = generatedPairs[sign][i];
                    Numbers[i] = ((s1[0] - 'a')) + 6 * (s1[1] - '1');
                }
            }
            var s = new StringBuilder("┌───┬───┬───┬───┬───┬───┐");

            for (var i = 0; i < 11; i++)
            {
                s.Append('\n');
                if (i % 2 == 0)
                {
                    var v = 5 - (i / 2);
                    for (var j = 0; j < 6; j++)
                    {
                        s.Append('│');
                        var o = Array.FindIndex(Numbers, x => x == v * 6 + j);
                        s.Append(' ');
                        if (o != -1)
                        {
                            var c = myPieces[o].Piece;
                            var pieceChar = ' ';
                            switch (c)
                            {
                                case chessPieceType.Bishop:
                                    pieceChar = 'B';
                                    break;
                                case chessPieceType.King:
                                    pieceChar = 'K';
                                    break;
                                case chessPieceType.Queen:
                                    pieceChar = 'Q';
                                    break;
                                case chessPieceType.Knight:
                                    pieceChar = 'N';
                                    break;
                                case chessPieceType.Rook:
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
                                s.Append(' ');

                        }
                        s.Append(' ');
                    }
                    s.Append('│');
                }
                else
                {
                    s.Append("├───┼───┼───┼───┼───┼───┤");
                }
            }
            s.Append('\n');
            s.Append("└───┴───┴───┴───┴───┴───┘");

            boardString = s.ToString();

            return true;
        }
        return false;
    }

    public static string[] GetSolution(bool serialNumberOdd, out int numAttempts, out string boardString)
    {
        numAttempts = 1;
        var generatedPairs = new string[2][];
        for (var i = 0; i < 2; i++)
            generatedPairs[i] = new string[7];
        while (!TestRand(ref generatedPairs, serialNumberOdd ? 1 : 0, out boardString))
            numAttempts++;
        return generatedPairs[serialNumberOdd ? 1 : 0];
    }

    public static void Practice()
    {
        while (true)
        {
            var odd = Rnd.Next(0, 2) == 0;
            var result = GetSolution(odd, out _, out var board);
            Console.WriteLine(odd ? "Serial number is odd." : "Serial number is even.");
            Console.WriteLine(result.Take(6).JoinString(" | "));
            Console.WriteLine("Answer?");

            var correct = false;
            for (var attempt = 0; attempt < 3 && !correct; attempt++)
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
