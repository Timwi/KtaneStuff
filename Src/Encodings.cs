using RT.Util;

namespace KtaneStuff
{
    public static class Encodings
    {
        public static readonly (int dx, int dy)[][] semaphores = Ut.NewArray(
                        /*A*/ new (int dx, int dy)[] { (-1, 1), (0, 1) },
                        /*B*/ new (int dx, int dy)[] { (-1, 0), (0, 1) },
                        /*C*/ new (int dx, int dy)[] { (-1, -1), (0, 1) },
                        /*D*/ new (int dx, int dy)[] { (0, -1), (0, 1) },
                        /*E*/ new (int dx, int dy)[] { (1, -1), (0, 1) },
                        /*F*/ new (int dx, int dy)[] { (1, 0), (0, 1) },
                        /*G*/ new (int dx, int dy)[] { (0, 1), (1, 1) },
                        /*H*/ new (int dx, int dy)[] { (-1, 0), (-1, 1) },
                        /*I*/ new (int dx, int dy)[] { (-1, -1), (-1, 1) },
                        /*J*/ new (int dx, int dy)[] { (0, -1), (1, 0) },
                        /*K*/ new (int dx, int dy)[] { (0, -1), (-1, 1) },
                        /*L*/ new (int dx, int dy)[] { (1, -1), (-1, 1) },
                        /*M*/ new (int dx, int dy)[] { (1, 0), (-1, 1) },
                        /*N*/ new (int dx, int dy)[] { (-1, 1), (1, 1) },
                        /*O*/ new (int dx, int dy)[] { (-1, -1), (-1, 0) },
                        /*P*/ new (int dx, int dy)[] { (0, -1), (-1, 0) },
                        /*Q*/ new (int dx, int dy)[] { (1, -1), (-1, 0) },
                        /*R*/ new (int dx, int dy)[] { (-1, 0), (1, 0) },
                        /*S*/ new (int dx, int dy)[] { (-1, 0), (1, 1) },
                        /*T*/ new (int dx, int dy)[] { (-1, -1), (0, -1) },
                        /*U*/ new (int dx, int dy)[] { (-1, -1), (1, -1) },
                        /*V*/ new (int dx, int dy)[] { (0, -1), (1, 1) },
                        /*W*/ new (int dx, int dy)[] { (1, -1), (1, 0) },
                        /*X*/ new (int dx, int dy)[] { (1, -1), (1, 1) },
                        /*Y*/ new (int dx, int dy)[] { (-1, -1), (1, 0) },
                        /*Z*/ new (int dx, int dy)[] { (1, 0), (1, 1) });

        public static readonly string[] BrailleDots = { "1", "12", "14", "145", "15", "124", "1245", "125", "24", "245", "13", "123", "134", "1345", "135", "1234", "12345", "1235", "234", "2345", "136", "1236", "2456", "1346", "13456", "1356" };
    }
}