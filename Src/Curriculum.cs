using System;
using RT.Util;

namespace KtaneStuff
{
    internal class Curriculum
    {
        internal static void Simulations()
        {
            var conditions = Ut.NewArray<(string name, Func<Edgework, bool> fnc)>(
                ("Last digit of the serial number is 0.", e => e.SerialNumber.EndsWith("0")),
                ("No indicators present.", e => e.GetNumIndicators() == 0),
                ("Five or more ports present.", e => e.GetNumPorts() >= 5),
                ("Four or more batteries present.", e => e.GetNumBatteries() >= 4),
                ("Empty port plate present.", e => e.GetNumEmptyPortPlates() >= 1),
                ("Number of modules on the bomb divisible by 3.", e => Rnd.Next(0, 3) == 0),
                ("More lit than unlit indicators.", e => e.GetNumLitIndicators() > e.GetNumUnlitIndicators()),
                ("Otherwise", e => true)

                //("Band practice — 3 or more batteries", e => e.GetNumBatteries() >= 3),
                //("Sleepy Gary — No indicators present", e => e.GetNumIndicators() == 0),
                //("Mathlete — Last digit of the serial is 0", e => e.SerialNumber.EndsWith("0")),
                //("Part-timer — More than 4 other modules present", e => true),
                //("BYOB — Less than 2 ports present", e => e.GetNumPorts() < 2),
                //("Freshman Year — Otherwise", e => true)
            );
            var stats = new int[conditions.Length];
            const int iterations = 1000;
            for (int iter = 0; iter < iterations; iter++)
            {
                var ew = Edgework.Generate(5, 5, true);
                for (int i = 0; i < conditions.Length; i++)
                    if (conditions[i].fnc(ew))
                    {
                        stats[i]++;
                        break;
                    }
            }

            for (int i = 0; i < conditions.Length; i++)
                Console.WriteLine($"{i + 1}. {conditions[i].name} = {stats[i] * 100 / (double) iterations}%");
        }
    }
}