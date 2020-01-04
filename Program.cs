using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SolverFoundation.Services;


namespace lab2_algorythm_4
{
    internal static class Program
    {
        /// <summary>
        /// Maksimalnaya koordinata matrici po X.
        /// </summary>
        private const int MaxX = 2;
        /// <summary>
        /// Maksimalnaya koordinata matrici po y.
        /// </summary>
        private const int MaxY = 2;
        /// <summary>
        /// Maksimalnoe kolichestvo iteraciy igri.
        /// </summary>
        private const int MaxPartiesCount = 100;
        /// <summary>
        /// Maksimalnoe sluchainoe chislo.
        /// </summary>
        private const int MinRandomValue = 0;
        /// <summary>
        /// Minimalnoe sluchainoe chislo.
        /// </summary>
        private const int MaxRandomValue = 1;
        /// <summary>
        /// Ekzemplyar klassa Random, dlya polucheniya sluchainogo chisla.
        /// </summary>
        private static Random _randomizer;

        /// <summary>
        /// Osnovnaya programma.
        /// </summary>
        private static void Main()
        {
            _randomizer = new Random();
            var arr = GetDefaultData();

            Console.WriteLine("Ishodnaya matrica:");
            DisplayArray(arr);

            var (a, b) = GetMaxAndMin(arr);

            Console.WriteLine($"a = [{a}], b = [{b}]");
            Console.WriteLine("a != b, sledovatelono, igra ne imeet sedlovoy tochki, reshenie budet v smeshannih strategiyah");
            Console.WriteLine();

            Console.WriteLine("Naidem optimalnuyu strategiyu:");
            var (p1, p2, v1) = GetOptimalStrategy(arr);
            Console.WriteLine($"p1 = [{p1:F2}], p2 = [{p2:F2}], v = [{v1:F2}]");
            Console.WriteLine();

            Console.WriteLine("Naidem smeshannuyu strategiyu:");
            var (q1, q2, v2) = GeMixedStrategy(arr);
            Console.WriteLine($"q1 = [{q1:F2}], q2 = [{q2:F2}], v = [{v2:F2}]");
            Console.WriteLine();

            Console.WriteLine("100 partiy igri:");
            var parties = Game(arr, p1, q1);
            Console.WriteLine($"|{"#",3}" +
                              $"|{nameof(Party.RandomA),7}" +
                              $"|{nameof(Party.StrategyA),9}" +
                              $"|{nameof(Party.RandomB),7}" +
                              $"|{nameof(Party.StrategyB),9}" +
                              $"|{nameof(Party.WinA),4}" +
                              $"|{nameof(Party.SumWinA),7}" +
                              $"|{nameof(Party.AvgWinA),7}|");

            Console.WriteLine("--------------------------------------------------------------");
            foreach (var party in parties)
            {
                Console.WriteLine($"|{party.PartyNumber,3}" +
                              $"|{party.RandomA,7:F3}" +
                              $"|{party.StrategyA,9}" +
                              $"|{party.RandomB,7:F3}" +
                              $"|{party.StrategyB,9}" +
                              $"|{party.WinA,4}" +
                              $"|{party.SumWinA,7}" +
                              $"|{party.AvgWinA,7:F2}");
            }
            Console.WriteLine();

            var a1Count = parties.Count(x => x.StrategyA == StrategyA.A1);
            var a2Count = parties.Count(x => x.StrategyA == StrategyA.A2);
            var b1Count = parties.Count(x => x.StrategyB == StrategyB.B1);
            var b2Count = parties.Count(x => x.StrategyB == StrategyB.B2);
            Console.WriteLine("Statistika:");
            Console.WriteLine($"Strategiya A1 ispolzovana {a1Count} raz.");
            Console.WriteLine($"Strategiya A2 ispolzovana {a2Count} raz.");
            Console.WriteLine($"Strategiya B1 ispolzovana {b1Count} raz.");
            Console.WriteLine($"Strategiya B2 ispolzovana {b2Count} raz.");
            Console.WriteLine();

            var freqP1 = (double)a1Count / MaxPartiesCount;
            var freqP2 = (double)a2Count / MaxPartiesCount;
            var freqQ1 = (double)b1Count / MaxPartiesCount;
            var freqQ2 = (double)b2Count / MaxPartiesCount;
            Console.WriteLine("Chastoti ispolzovaniya:");
            Console.WriteLine($"p = ({freqP1:F2}; {freqP2:F2}), q = ({freqQ1:F2}; {freqQ2:F2})");
            Console.WriteLine();

            Console.WriteLine($"Sredniy viygrish = {parties.LastOrDefault()?.AvgWinA ?? 0:F}");


            Console.ReadKey();
        }

        /// <summary>
        /// Metod dlya polucheniya iznachalnoi matrici.
        /// </summary>
        /// <returns></returns>
        private static int[,] GetDefaultData()
        {
            var arr = new int[MaxX, MaxY];
            arr[0, 0] = 9; arr[0, 1] = 13;
            arr[1, 0] = 21; arr[1, 1] = 8;
            return arr;
        }

        /// <summary>
        /// Metod dlya polucheniya nijnei i verhnei ceni igri.
        /// </summary>
        /// <param name="arr"></param>
        /// <returns></returns>
        private static (int a, int b) GetMaxAndMin(int[,] arr)
        {
            var maxInColumn = new Dictionary<int, int>();
            var minInRow = new Dictionary<int, int>();

            for (var i = 0; i < MaxX; i++)
            {
                for (var j = 0; j < MaxY; j++)
                {
                    var val = arr[i, j];

                    if (!maxInColumn.ContainsKey(i)) maxInColumn[i] = int.MinValue;
                    if (maxInColumn[i] < val) maxInColumn[i] = val;

                    if (!minInRow.ContainsKey(j)) minInRow[j] = int.MaxValue;
                    if (minInRow[j] > val) minInRow[j] = val;
                }
            }

            var a = minInRow.Values.Max();
            var b = maxInColumn.Values.Min();

            return (a, b);
        }

        /// <summary>
        /// Metod dlya polucheniya optimalnoy strategii.
        /// </summary>
        /// <param name="arr"></param>
        /// <returns></returns>
        private static (double p1, double p2, double v) GetOptimalStrategy(int[,] arr)
        {
            const string p1Param = "p1";
            const string p2Param = "p2";
            const string vParam = "v";

            const string eq1Param = "eq1";
            const string eq2Param = "eq2";
            const string eq3Param = "eq3";

            var context = SolverContext.GetContext();
            var model = context.CreateModel();

            var p1Dec = new Decision(Domain.Real, p1Param);
            var p2Dec = new Decision(Domain.Real, p2Param);
            var vDec = new Decision(Domain.Real, vParam);
            model.AddDecisions(p1Dec, p2Dec, vDec);
            model.AddConstraint(eq1Param, arr[0, 0] * p1Dec + arr[1, 0] * p2Dec == vDec);
            model.AddConstraint(eq2Param, arr[0, 1] * p1Dec + arr[1, 1] * p2Dec == vDec);
            model.AddConstraint(eq3Param, p1Dec + p2Dec == 1);
            var solution = context.Solve();

            double p1 = 0;
            double p2 = 0;
            double v = 0;
            foreach (var decision in solution.Decisions)
            {
                switch (decision.Name)
                {
                    case p1Param:
                        p1 = (double)(decision.GetValues()
                                                .FirstOrDefault()
                                                ?.FirstOrDefault() ?? 0);
                        break;
                    case p2Param:
                        p2 = (double)(decision.GetValues()
                                                .FirstOrDefault()
                                                ?.FirstOrDefault() ?? 0);
                        break;
                    case vParam:
                        v = (double)(decision.GetValues()
                                               .FirstOrDefault()
                                               ?.FirstOrDefault() ?? 0);
                        break;
                }
            }

            context.ClearModel();

            return (p1, p2, v);
        }

        /// <summary>
        /// Metod dlya polucheniya smeshannoy strategii.
        /// </summary>
        /// <param name="arr"></param>
        /// <returns></returns>
        private static (double q1, double q2, double v) GeMixedStrategy(int[,] arr)
        {
            const string q1Param = "q1";
            const string p2Param = "q2";
            const string vParam = "v";

            const string eq1Param = "eq1";
            const string eq2Param = "eq2";
            const string eq3Param = "eq3";

            var context = SolverContext.GetContext();
            var model = context.CreateModel();

            var q1Dec = new Decision(Domain.Real, q1Param);
            var q2Dec = new Decision(Domain.Real, p2Param);
            var vDec = new Decision(Domain.Real, vParam);
            model.AddDecisions(q1Dec, q2Dec, vDec);
            model.AddConstraint(eq1Param, arr[0, 0] * q1Dec + arr[0, 1] * q2Dec == vDec);
            model.AddConstraint(eq2Param, arr[1, 0] * q1Dec + arr[1, 1] * q2Dec == vDec);
            model.AddConstraint(eq3Param, q1Dec + q2Dec == 1);
            var solution = context.Solve();

            double q1 = 0;
            double q2 = 0;
            double v = 0;
            foreach (var decision in solution.Decisions)
            {
                switch (decision.Name)
                {
                    case q1Param:
                        q1 = (double)(decision.GetValues()
                                                .FirstOrDefault()
                                                ?.FirstOrDefault() ?? 0);
                        break;
                    case p2Param:
                        q2 = (double)(decision.GetValues()
                                                .FirstOrDefault()
                                                ?.FirstOrDefault() ?? 0);
                        break;
                    case vParam:
                        v = (double)(decision.GetValues()
                                               .FirstOrDefault()
                                               ?.FirstOrDefault() ?? 0);
                        break;
                }
            }

            context.ClearModel();

            return (q1, q2, v);
        }

        /// <summary>
        /// Metod dlya zapuska igri.
        /// </summary>
        /// <param name="arr"></param>
        /// <returns></returns>
        private static List<Party> Game(int[,] arr, double p1, double q1)
        {
            var parties = new List<Party>();
            var sumWinA = 0;
            for (var i = 1; i <= MaxPartiesCount; i++)
            {
                var randomA = GetRandomNumber(MinRandomValue, MaxRandomValue);
                var strategyA = randomA < p1 ? StrategyA.A1 : StrategyA.A2;
                var randomB = GetRandomNumber(MinRandomValue, MaxRandomValue);
                var strategyB = randomB < q1 ? StrategyB.B1 : StrategyB.B2;

                var indexA = 0;
                switch (strategyA)
                {
                    case StrategyA.A1:
                        indexA = 0;
                        break;
                    case StrategyA.A2:
                        indexA = 1;
                        break;
                }

                var indexB = 0;
                switch (strategyB)
                {
                    case StrategyB.B1:
                        indexB = 0;
                        break;
                    case StrategyB.B2:
                        indexB = 1;
                        break;
                }

                var val = arr[indexA, indexB];
                sumWinA += val;
                var avgWinA = (double)sumWinA / i;

                var party = new Party
                {
                    PartyNumber = i,
                    RandomA = randomA,
                    StrategyA = strategyA,
                    RandomB = randomB,
                    StrategyB = strategyB,
                    WinA = val,
                    SumWinA = sumWinA,
                    AvgWinA = avgWinA
                };
                parties.Add(party);
            }
            return parties;
        }

        /// <summary>
        /// Metod dlya polucheniya sluchainogo chisla.
        /// </summary>
        /// <param name="minimum"></param>
        /// <param name="maximum"></param>
        /// <returns></returns>
        private static double GetRandomNumber(double minimum, double maximum)
        {
            return _randomizer.NextDouble() * (maximum - minimum) + minimum;
        }

        /// <summary>
        /// Metod dlya otobrazheniya iznachalnoy matrici.
        /// </summary>
        /// <param name="arr"></param>
        private static void DisplayArray(int[,] arr)
        {
            for (var i = 0; i < MaxX; i++)
            {
                Console.Write("|");
                for (var j = 0; j < MaxY; j++)
                {
                    var val = arr[i, j];
                    Console.Write($"{val,3}|");
                }
                Console.WriteLine();
            }
            Console.WriteLine("---------------------------");
        }
    }

    /// <summary>
    /// Enumeraciya strategiy igroka A.
    /// </summary>
    public enum StrategyA
    {
        A1,
        A2
    }

    /// <summary>
    /// Enumeraciya strategiy igroka B.
    /// </summary>
    public enum StrategyB
    {
        B1,
        B2
    }

    /// <summary>
    /// Class, obertka dlya odnoi iteraciy igri.
    /// </summary>
    public class Party
    {
        public int PartyNumber { get; set; }
        public double RandomA { get; set; }
        public StrategyA StrategyA { get; set; }
        public double RandomB { get; set; }
        public StrategyB StrategyB { get; set; }
        public int WinA { get; set; }
        public int SumWinA { get; set; }
        public double AvgWinA { get; set; }
    }
}
