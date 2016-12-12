using System.Collections.Generic;
using RT.Util;

namespace KtaneStuff
{
    static class Chess
    {
        private class ChessPiece
        {
            public int xCoord;

            public int yCoord;

            public string name;

            public ChessPiece(int x, int y, string pieceName, ref int[,] board)
            {
                this.xCoord = x;
                this.yCoord = y;
                this.name = pieceName.ToLower();
                board[x, y] = 2;
            }

            public static bool getWhite(int x, int y)
            {
                return x % 2 != y % 2;
            }

            public static void getRookTouchable(int x, int y, ref int[,] myList, bool tempKing = false)
            {
                for (int i = x + 1; i < 6; i++)
                {
                    if (myList[i, y] == 0)
                    {
                        myList[i, y] = 1;
                    }
                    else if (myList[i, y] == 2)
                    {
                        break;
                    }
                    if (tempKing)
                    {
                        break;
                    }
                }
                for (int j = x - 1; j >= 0; j--)
                {
                    if (myList[j, y] == 0)
                    {
                        myList[j, y] = 1;
                    }
                    else if (myList[j, y] == 2)
                    {
                        break;
                    }
                    if (tempKing)
                    {
                        break;
                    }
                }
                for (int k = y + 1; k < 6; k++)
                {
                    if (myList[x, k] == 0)
                    {
                        myList[x, k] = 1;
                    }
                    else if (myList[x, k] == 2)
                    {
                        break;
                    }
                    if (tempKing)
                    {
                        break;
                    }
                }
                for (int l = y - 1; l >= 0; l--)
                {
                    if (myList[x, l] == 0)
                    {
                        myList[x, l] = 1;
                    }
                    else if (myList[x, l] == 2)
                    {
                        break;
                    }
                    if (tempKing)
                    {
                        break;
                    }
                }
            }

            public static void getBishopTouchable(int x, int y, ref int[,] myList, bool tempKing = false)
            {
                int num = x + 1;
                int num2 = y + 1;
                while (num2 < 6 && num < 6)
                {
                    if (myList[num, num2] == 0)
                    {
                        myList[num, num2] = 1;
                    }
                    else if (myList[num, num2] == 2)
                    {
                        break;
                    }
                    if (tempKing)
                    {
                        break;
                    }
                    num++;
                    num2++;
                }
                int num3 = x - 1;
                int num4 = y + 1;
                while (num4 < 6 && num3 >= 0)
                {
                    if (myList[num3, num4] == 0)
                    {
                        myList[num3, num4] = 1;
                    }
                    else if (myList[num3, num4] == 2)
                    {
                        break;
                    }
                    if (tempKing)
                    {
                        break;
                    }
                    num3--;
                    num4++;
                }
                int num5 = x + 1;
                int num6 = y - 1;
                while (num6 >= 0 && num5 < 6)
                {
                    if (myList[num5, num6] == 0)
                    {
                        myList[num5, num6] = 1;
                    }
                    else if (myList[num5, num6] == 2)
                    {
                        break;
                    }
                    if (tempKing)
                    {
                        break;
                    }
                    num5++;
                    num6--;
                }
                int num7 = x - 1;
                int num8 = y - 1;
                while (num8 >= 0 && num7 >= 0)
                {
                    if (myList[num7, num8] == 0)
                    {
                        myList[num7, num8] = 1;
                    }
                    else if (myList[num7, num8] == 2)
                    {
                        break;
                    }
                    if (tempKing)
                    {
                        break;
                    }
                    num7--;
                    num8--;
                }
            }

            public static void getQueenTouchable(int x, int y, ref int[,] myList)
            {
                ChessPiece.getRookTouchable(x, y, ref myList, false);
                ChessPiece.getBishopTouchable(x, y, ref myList, false);
            }

            public static void getKnightTouchable(int x, int y, ref int[,] myList)
            {
                int num = x + 2;
                int num2 = y + 1;
                if (num >= 0 && num < 6 && num2 < 6 && num2 >= 0 && myList[num, num2] == 0)
                {
                    myList[num, num2] = 1;
                }
                num = x - 2;
                num2 = y + 1;
                if (num >= 0 && num < 6 && num2 < 6 && num2 >= 0 && myList[num, num2] == 0)
                {
                    myList[num, num2] = 1;
                }
                num = x + 2;
                num2 = y - 1;
                if (num >= 0 && num < 6 && num2 < 6 && num2 >= 0 && myList[num, num2] == 0)
                {
                    myList[num, num2] = 1;
                }
                num = x - 2;
                num2 = y - 1;
                if (num >= 0 && num < 6 && num2 < 6 && num2 >= 0 && myList[num, num2] == 0)
                {
                    myList[num, num2] = 1;
                }
                num = x + 1;
                num2 = y + 2;
                if (num >= 0 && num < 6 && num2 < 6 && num2 >= 0 && myList[num, num2] == 0)
                {
                    myList[num, num2] = 1;
                }
                num = x - 1;
                num2 = y + 2;
                if (num >= 0 && num < 6 && num2 < 6 && num2 >= 0 && myList[num, num2] == 0)
                {
                    myList[num, num2] = 1;
                }
                num = x + 1;
                num2 = y - 2;
                if (num >= 0 && num < 6 && num2 < 6 && num2 >= 0 && myList[num, num2] == 0)
                {
                    myList[num, num2] = 1;
                }
                num = x - 1;
                num2 = y - 2;
                if (num >= 0 && num < 6 && num2 < 6 && num2 >= 0 && myList[num, num2] == 0)
                {
                    myList[num, num2] = 1;
                }
            }

            public static void getKingTouchable(int x, int y, ref int[,] myList)
            {
                ChessPiece.getRookTouchable(x, y, ref myList, true);
                ChessPiece.getBishopTouchable(x, y, ref myList, true);
            }

            public void getTouchable(ref int[,] myList)
            {
                string text = this.name;
                switch (text)
                {
                    case "queen":
                        ChessPiece.getQueenTouchable(this.xCoord, this.yCoord, ref myList);
                        break;
                    case "king":
                        ChessPiece.getKingTouchable(this.xCoord, this.yCoord, ref myList);
                        break;
                    case "bishop":
                        ChessPiece.getBishopTouchable(this.xCoord, this.yCoord, ref myList, false);
                        break;
                    case "knight":
                        ChessPiece.getKnightTouchable(this.xCoord, this.yCoord, ref myList);
                        break;
                    case "rook":
                        ChessPiece.getRookTouchable(this.xCoord, this.yCoord, ref myList, false);
                        break;
                }
            }
        }

        public static bool TestRand(ref string[] generated, bool serialNumberOdd)
        {
            string[] array = new string[6];
            string text = string.Empty;
            int i = 0;
            while (i < 6)
            {
                string text2 = (char) (Rnd.Next(0, 6) + 97) + string.Empty + (Rnd.Next(0, 6) + 1);
                if (!text.Contains(text2))
                {
                    array[i] = text2;
                    text += text2;
                    i++;
                }
            }
            int[,] array2 = new int[6, 6];
            for (int j = 0; j < 6; j++)
            {
                for (int k = 0; k < 6; k++)
                {
                    array2[j, k] = 0;
                }
            }
            ChessPiece[] array3 = new ChessPiece[6];
            array3[1] = new ChessPiece((int) (array[1][0] - 'a'), (int) (array[1][1] - '1'), serialNumberOdd ? "rook" : "knight", ref array2);
            array3[3] = new ChessPiece((int) (array[3][0] - 'a'), (int) (array[3][1] - '1'), "rook", ref array2);
            if (ChessPiece.getWhite((int) (array[4][0] - 'a'), (int) (array[4][1] - '1')))
            {
                array3[4] = new ChessPiece((int) (array[4][0] - 'a'), (int) (array[4][1] - '1'), "queen", ref array2);
                array3[0] = new ChessPiece((int) (array[0][0] - 'a'), (int) (array[0][1] - '1'), "king", ref array2);
            }
            else
            {
                array3[4] = new ChessPiece((int) (array[4][0] - 'a'), (int) (array[4][1] - '1'), "rook", ref array2);
                array3[0] = new ChessPiece((int) (array[0][0] - 'a'), (int) (array[0][1] - '1'), "bishop", ref array2);
            }
            if (serialNumberOdd || array3[4].name == "rook")
            {
                array3[2] = new ChessPiece((int) (array[2][0] - 'a'), (int) (array[2][1] - '1'), "king", ref array2);
            }
            else
            {
                array3[2] = new ChessPiece((int) (array[2][0] - 'a'), (int) (array[2][1] - '1'), "queen", ref array2);
            }
            bool flag = false;
            bool flag2 = false;
            for (int l = 0; l < 5; l++)
            {
                if (array3[l].name == "queen")
                {
                    flag = true;
                }
                if (array3[l].name == "knight")
                {
                    flag2 = true;
                }
            }
            if (!flag)
            {
                array3[5] = new ChessPiece((int) (array[5][0] - 'a'), (int) (array[5][1] - '1'), "queen", ref array2);
            }
            else if (!flag2)
            {
                array3[5] = new ChessPiece((int) (array[5][0] - 'a'), (int) (array[5][1] - '1'), "knight", ref array2);
            }
            else
            {
                array3[5] = new ChessPiece((int) (array[5][0] - 'a'), (int) (array[5][1] - '1'), "bishop", ref array2);
            }
            ChessPiece[] array4 = array3;
            for (int m = 0; m < array4.Length; m++)
            {
                ChessPiece chessPiece = array4[m];
                chessPiece.getTouchable(ref array2);
            }
            int num = 0;
            List<string> list = new List<string>();
            int num2 = 5;
            while (num2 >= 0 && num < 2)
            {
                int num3 = 0;
                while (num3 < 6 && num < 2)
                {
                    if (array2[num3, num2] == 0)
                    {
                        num++;
                        list.Add((char) (num3 + 97) + string.Empty + (num2 + 1));
                    }
                    num3++;
                }
                num2--;
            }
            if (num == 1)
            {
                for (int n = 0; n < 6; n++)
                    generated[n] = array[n];
                generated[6] = list[0];
                return true;
            }
            return false;
        }

        public static string[] GetSolution(bool serialNumberOdd, out int numAttempts)
        {
            numAttempts = 1;
            var generatedPairs = new string[7];
            while (!TestRand(ref generatedPairs, serialNumberOdd))
                numAttempts++;
            return generatedPairs;
        }
    }
}
