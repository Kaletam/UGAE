using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace GA
{
    class Program
    {
        // I believe this is a n out of 10,000 chance... I'll have to review my code, ack.
        private static int mutationChance = 9000; // 2 / 10000
        private static int insertionChance = 9000; // 2 / 10000
        private static int deletionChance = 9900; // 2 / 10000
        private static int swapChance = 9900; // 2 / 10000

        //private static double targetValue = 9; //

        /*
         * In lieu of an "environment" that would submit equations to pressure and selection, I use bonuses and maluses in awarding fitness.
         * This is cumbersome and awkward and offends my aesthetic sense, but I'm not sure how to improve upon this at this time.
         * For the most part, this is not *yet* intended to be an open system, allowing for arbitrary evolution, but rather for very targeted
         * solutions. So setting a goal isn't a problem - this is guided evolution.
         * 
         * How I go about configuring this for open-ended evolution remains to be seen.
         * 
         * And the way my mind works, I'm now pondering whether I can provide for evolution of *these values* in order to find an optimized set (or sets)
         * to find optimal solutions... cf. the evolution of the biological genetic code, heh.
         * 
         * Uncertain: how to account/handle situations such as the 7*X*X/7 equation I observed in the last run.
         */
        private static double bonusValue = 1.66; // Bonus to fitness relative to accurate value/answer of the equation string.
        private static double bonusLength = 1.66; // Currently unused. Was an attempt at awarding equations where validated strings were close to their genotype.
        private static double bonusGene = 2; // Uh. I forget.
        private static double bonusValid = 1.66; // Well formed equations get a bonus.

        private static double malusGene = 2.5; // I forget. Gotta review code.
        private static double malusRequiredVariable = 10; // Right now, we stipulate this variable or that. If required variables are not found, penalize the equation. This negates possibility of an equation being fit *but* for the equation... I may change that.
        private static double malusValid = 1.33; // If the equation is invalid, penalize.
        private static double malusTimesZero = 10; // We aim for simplicity. Multiplying something by zero is unlikely to be simple (or helpful).
        private static double malusTimesOne = 5; // As with the *byzero penalty. Penalized less as this is more likely to help (maybe?).
        private static double malusLength = 1.66; // If a chromosome is too long, penalize. Was relative to genotype, currently relative to an arbitrary value (20 genes).
        private static double malusSilence = 2.5; // Unused. A variation on penalizing length.

        private static Dictionary<string, Dictionary<string, double>> dict = new Dictionary<string, Dictionary<string, double>>();

        private static int numChrom = 12;

        private static int page = 0;

        // Provide an arbitrary cap on fitness, for simplicity of comparisons, etc.
        private const double maxFitness = 50000;

        // Lists of chromosomes. The latter two record the best chromosomes. The first is the best 8 of this iteration, the second is the best 8 (?) ever.
        private static List<Chromosome> chromosomes;
        private static List<Chromosome> bestChromosomes;
        private static List<Chromosome> bestEverChromosomes;

        // Since we evaluate the best chromosome strings, instead of assigning a null value, assign a value that can be evaluated but will not give us a "good" fitness.
        private static string bestChromosomeString = "NaN";
        private static double bestChromosomeFitness = 0;

        private static string bestEverChromosomeString = "NaN";
        private static string bestEverChromosomeStringValidated = "NaN";
        private static double bestEverChromosomeFitness = maxFitness;
        private static double bestEverChromosomeValue = 0;

        private static string bredChromosomeString = "NaN";
        private static double bredChromosomeFitness = 0;

        // Minimum on minimum fitness. Currently, the lower a fitness, the better. This isn't intuitive, but I didn't get to defining a better range.
        private static double minFitness = 0.001;

        private static Dictionary<string, int> genome = new Dictionary<string, int>();

        private static Random r = new Random();

        private static int t = 0;
        private static int step = 5;
        private static int solutionFoundAt = -1;

        private static bool paused = true;
        private static bool solution = false;

        private static double CalculateFitness(Chromosome c)
        {
            return CalculateFitness(c, false);
        }

        // Lower = better.
        private static double CalculateFitness(Chromosome c, bool explain)
        {
            double targetValue = 0;

            c.FitnessBonus = 1;
            c.Fitness = 0;

            // I forget why I comment out trying to get other variables.
            foreach (KeyValuePair<string, Dictionary<string, double>> kvp in dict)
            {
                double r, s, u, v, w, x, y, z;
                // For now, gotta change these when changing problem sets!
                //kvp.Value.TryGetValue("R", out r);
                //kvp.Value.TryGetValue("S", out s);
                //kvp.Value.TryGetValue("U", out u);
                //kvp.Value.TryGetValue("V", out v);
                //kvp.Value.TryGetValue("W", out w);
                kvp.Value.TryGetValue("X", out x);
                kvp.Value.TryGetValue("Y", out y);
                kvp.Value.TryGetValue("Z", out z);

                //c.R = r;
                //c.S = s;
                //targetValue = u;
                //c.U = u; // Er, we don't want this in the equation. This is our target!
                //c.V = v;
                //c.W = w;
                c.X = x; // For simple equations, X is our target.
                //c.Y = y; // For simple equations, Y is our target.
                targetValue = y;
                //targetValue = z;
                //c.Z = z;
                c.ForceCalculate();

                if (explain) Console.WriteLine(targetValue);
                if (explain) Console.WriteLine(c.X);
                if (explain) Console.WriteLine(c.Y);

                //Console.WriteLine(c.Result);
                //Console.WriteLine(c.Result / targetValue);
                //Console.WriteLine(c.Genes.Count);

                if (explain) Console.WriteLine(c.IsValid);

                if (!c.IsValid)
                {
                    c.Fitness += maxFitness;
                    break;
                }

                double tFitness = Math.Max(Math.Abs(c.Result - targetValue), minFitness);

                if (explain) Console.WriteLine("Base: " + tFitness);

                // Penalize Chromosomes that differ between real and validated lengths.
                // Because validation can increase the length of the string for evaluation purposes - turning XX into X*X, for example - we won't use this right now.
                //int ld = Math.Abs(c.GeneString.Length - c.GeneStringValidated.Length);
                int ld = 0;

                // On second thought, just penalize long strings.
                int thresholdLength = 20;

                int deltaLength = (c.GeneString.Length - thresholdLength);

                ld = (deltaLength > 0) ? deltaLength : 0;

                if (explain) Console.WriteLine("LD: " + ld);

                if (ld != 0)
                {
                    if (explain) Console.WriteLine("Length Penalty");
                    tFitness *= malusLength * ld;
                    c.FitnessBonus *= malusLength * ld;
                }
                //else
                //{
                //    tFitness /= bonusValid;
                //    c.FitnessBonus /= bonusValid;
                //}

                //deltaLength = (c.GeneString.Length - bestEverChromosomeString.Length);

                //ld = (deltaLength > 0) ? deltaLength : 0;

                //if (explain) Console.WriteLine("LD: " + ld);

                //if (ld != 0)
                //{
                //    if (explain) Console.WriteLine("Length Penalty");
                //    tFitness *= malusLength * ld;
                //    c.FitnessBonus *= malusLength * ld;
                //}

                //deltaLength = (c.GeneString.Length - c.GeneStringValidated.Length);

                //ld = (deltaLength > 0) ? deltaLength : 0;

                //if (explain) Console.WriteLine("Silence: " + ld);

                //if (ld != 0)
                //{
                //    if (explain) Console.WriteLine("Silence Penalty");
                //    tFitness *= malusSilence * ld;
                //    c.FitnessBonus *= malusSilence * ld;
                //}

                // *0 or 0* is a pain.
                // As is 0X or X0... so variables negated in such a fashion should result in penalty as well.
                if (c.GeneString.Contains("*0") || c.GeneString.Contains("0*") || c.GeneStringValidated.Contains("*0") || c.GeneStringValidated.Contains("0*"))
                {
                    if (explain) Console.WriteLine("*Zero Penalty");
                    tFitness *= malusTimesZero;
                    c.FitnessBonus *= malusTimesZero;
                }

                // For now, gotta change these when changing problem sets!
                //char[] vars = { 'V', 'W', 'X', 'Y', 'Z' };
                //char[] vars = { 'V', 'W', 'X' };
                char[] vars = { 'X' };
                char[] vars2 = { 'R', 'S', 'T', 'U', 'V', 'W', 'Y', 'Z' };
                //List<char> varsAllList = new List<char>();

                // Penalize 0X, X0, 1X, and X1.
                foreach (char ch in "RSTUVWXYZ")
                {
                    if (c.GeneString.Contains("0" + ch) || c.GeneString.Contains(ch + "0"))
                    {
                        if (explain) Console.WriteLine(ch + "*Zero Penalty");
                        tFitness *= malusTimesZero;
                        c.FitnessBonus *= malusTimesZero;
                    }
                    //varsAllList.Add(ch);
                }

                foreach (char ch in "RSTUVWXYZ")
                {
                    if (
                            c.GeneString.Contains("1" + ch) || c.GeneString.Contains(ch + "1") ||
                            c.GeneString.Contains("1*" + ch) || c.GeneString.Contains(ch + "*1") ||
                            c.GeneString.Contains(ch + "/1")
                        )
                    {
                        if (explain) Console.WriteLine(ch + "*One Penalty");
                        tFitness *= malusTimesOne;
                        c.FitnessBonus *= malusTimesOne;
                    }
                }

                // I don't recall why I penalize this.
                foreach (char ch1 in "RSTUVWYZ")
                {
                    foreach (char ch2 in "RSTUVWXYZ")
                    {
                        if (c.GeneString.Contains(ch1.ToString() + ch2.ToString()) || c.GeneString.Contains(ch2.ToString() + ch1.ToString()))
                        {
                            if (explain) Console.WriteLine(ch1 + ch2 + " Penalty");
                            tFitness *= malusTimesZero;
                            c.FitnessBonus *= malusTimesZero;
                        }
                    }
                }

                //char[] varsAll = varsAllList.ToArray();

                // Select against variables we don't want in the string.
                foreach (char ch in vars2)
                {
                    if (c.GeneString.Count(n => n.Equals(ch)) >= 1)
                    {
                        if (explain) Console.WriteLine(ch + " Penalty");
                        tFitness *= malusGene * c.GeneString.Count(n => n.Equals(ch));
                        c.FitnessBonus *= malusGene * c.GeneString.Count(n => n.Equals(ch));
                    }
                }

                foreach (char ch in vars)
                {
                    // Penalize the Chromosome if our intended variables aren't included.
                    if (!c.GeneStringValidated.Contains(ch))
                    {
                        if (explain) Console.WriteLine("Required Var Penalty");
                        tFitness *= malusRequiredVariable;
                        c.FitnessBonus *= malusRequiredVariable;
                    }
                    // Reward if our intended variable(s) are.
                    else
                    {
                        if (explain) Console.WriteLine("Required Var Bonus");
                        tFitness /= bonusGene;
                        c.FitnessBonus /= bonusGene;
                    }
                }

                if (explain) Console.WriteLine(tFitness);

                if (explain) Console.ReadLine();

                if (tFitness > maxFitness) tFitness = maxFitness;

                c.Fitness += tFitness;
            }

            return c.Fitness;
        }

        static void Main(string[] args)
        {
            if (args.Length == 1)
            {
                numChrom = Convert.ToInt32(args[0]);
            }

            DoSetupCases();

            Console.Clear();
            Console.SetWindowSize(80, 50);

            chromosomes = new List<Chromosome>();
            bestEverChromosomes = new List<Chromosome>();

            for (int i = 0; i < numChrom; i++)
            {
                chromosomes.Add(CreateRandomChromosome());

                genome.Add(chromosomes[i].GeneString, 1);
            }

            foreach (Chromosome bc in chromosomes)
            {
                CalculateFitness(bc);
                bestEverChromosomes.Add(bc);
            }

            bestEverChromosomes = bestEverChromosomes.OrderBy(n => n.Fitness).ToList();

            List<Chromosome> tc = chromosomes;

            tc = tc.OrderBy(n => n.Fitness).ToList();

            bestChromosomes = new List<Chromosome>();

            for (int i = 0; i < 8; i++)
            {
                bestChromosomes.Add(tc[i]);
            }

            Chromosome c;

            // Testing/debugging code I haven't bothered taking out yet.
            //Chromosome b;
            //c = chromosomes[0];
            //b = chromosomes[1];

            //List<Chromosome> a = Chromosome.Crossover(c, b);

            //Console.ReadLine();
            //return;

            bool quit = false;
            bool changed = false;
            bool tick = false;
            int index = 0;
            int index2 = 0;

            DisplayAll(chromosomes, index, index2, t);

            // TODO: Add help. Rework some displays.
            // For that matter, it's probably time to move this to a GUI.
            while (!quit)
            {
                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo cki = Console.ReadKey(true);

                    switch (cki.Key)
                    {
                        case ConsoleKey.B:
                            DoShowBestChromosomes();
                            changed = true;
                            break;
                        case ConsoleKey.G:
                            DoShowGenome();
                            changed = true;
                            break;
                        case ConsoleKey.PageUp:
                            page--;
                            if (page < 0) page = (int)Math.Ceiling((double)numChrom / 6) - 1;
                            changed = true;
                            break;
                        case ConsoleKey.PageDown:
                            page++;
                            if (page > (int)Math.Ceiling((double)numChrom / 6) - 1) page = 0;
                            changed = true;
                            break;
                        case ConsoleKey.E:
                            CalculateFitness(chromosomes[index], true);
                            changed = true;
                            break;
                        case ConsoleKey.Escape:
                            chromosomes = new List<Chromosome>();

                            for (int i = 0; i < numChrom; i++)
                            {
                                chromosomes.Add(CreateRandomChromosome());
                            }

                            changed = true;
                            break;
                        case ConsoleKey.UpArrow:
                            step++;
                            if (step > 25) step = 25;
                            break;
                        case ConsoleKey.DownArrow:
                            step--;
                            if (step < 1) step = 1;
                            break;
                        case ConsoleKey.RightArrow:
                            if ((cki.Modifiers & ConsoleModifiers.Shift) == 0)
                            {
                                index = (++index) % chromosomes.Count;
                            }
                            else
                            {
                                index2 = (++index2) % chromosomes.Count;
                            }
                            changed = true;

                            break;
                        case ConsoleKey.LeftArrow:
                            if ((cki.Modifiers & ConsoleModifiers.Shift) == 0)
                            {
                                --index;
                                if (index < 0) index = chromosomes.Count - 1;
                            }
                            else
                            {
                                --index2;
                                if (index2 < 0) index2 = chromosomes.Count - 1;
                            }
                            changed = true;
                            break;
                        case ConsoleKey.Z:
                            if ((cki.Modifiers & ConsoleModifiers.Shift) != 0)
                            {
                                for (int i = 0; i < 10; i++)
                                {
                                    DoGeneration();
                                    t++;
                                }
                                changed = true;
                                break;
                            }

                            if ((cki.Modifiers & ConsoleModifiers.Control) != 0)
                            {
                                for (int i = 0; i < 100; i++)
                                {
                                    DoGeneration();
                                    t++;
                                }
                                changed = true;
                                break;
                            }

                            if ((cki.Modifiers & ConsoleModifiers.Alt) != 0)
                            {
                                for (int i = 0; i < 1000; i++)
                                {
                                    DoGeneration();
                                    t++;
                                }
                                changed = true;
                                break;
                            }

                            for (int i = 0; i < step; i++)
                            {
                                DoGeneration();
                                t++;
                            }
                            changed = true;

                            break;
                        case ConsoleKey.Q:
                            quit = true;
                            break;
                        case ConsoleKey.S:
                            chromosomes[index].Swap();
                            changed = true;
                            break;
                        case ConsoleKey.M:
                            chromosomes[index].Mutate();
                            changed = true;
                            break;
                        case ConsoleKey.I:
                            chromosomes[index].Insert();
                            changed = true;
                            break;
                        case ConsoleKey.D:
                            if ((cki.Modifiers & ConsoleModifiers.Shift) != 0)
                            {
                                DisplayBestChromosomes();
                                changed = true;
                            }
                            // I believe I put a limit of three as otherwise equations, under some parameters, would rapidly shrink in size... that may only be a consequence of certain ways of awarding/penalizing length.
                            else if (chromosomes[index].Genes.Count > 3)
                            {
                                chromosomes[index].Delete();
                                changed = true;
                            }
                            break;
                        case ConsoleKey.P:
                            paused = !paused;
                            break;
                        // Randomly modify the chromosome. Or "DisplayOdds", which currently doesn't work in-program?
                        case ConsoleKey.R:
                            if ((cki.Modifiers & ConsoleModifiers.Shift) != 0)
                            {
                                DisplayOdds();
                                changed = true;
                                break;
                            }

                            c = chromosomes[index];

                            switch (r.Next(0, 3))
                            {
                                case 0:
                                    c.Mutate();
                                    break;
                                case 1:
                                    c.Insert();
                                    break;
                                case 2:
                                    if (c.Genes.Count > 3)
                                    {
                                        c.Delete();
                                    }
                                    break;
                            }

                            changed = true;
                            break;
                        default:
                            break;
                    }
                }

                if (!paused)
                {
                    for (int i = 0; i < step; i++)
                    {
                        DoGeneration();
                        t++;

                        if (bestEverChromosomeFitness <= minFitness * dict.Count)
                        {
                            if ((solutionFoundAt == -1) || (t - solutionFoundAt) > 100)
                            {
                                solutionFoundAt = t;
                                solution = true;
                                paused = true;
                                break;
                            }
                        }
                    }
                    changed = true;
                }

                if (tick)
                {
                    t++;
                }

                if (changed)
                {
                    Console.Clear();
                    changed = false;
                    tick = false;

                    DisplayAll(chromosomes, index, index2, t);
                }
            }
        }

        private static void DoShowBestChromosomes()
        {
            Console.Clear();
            int y = 0;

            foreach (Chromosome c in bestEverChromosomes)
            {
                DisplayChromosomeAt(c, 0, y);
                y += 6;
            }

            Console.ReadLine();
        }

        // For now, statically, manually, setup cases for evolution.
        // While the latter cases may seem trivial, what good is an evolutionary algorithm engine that can't evolve an equation for squaring?
        // Any equations more complex than those samples would require that already (ideally) perfect engine.
        // In particular, this is meant to be able to solve (possibly very complex) equations without knowing those equations ahead of time, but that's just one intention.
        private static void DoSetupCases()
        {
            Dictionary<string, double> d;

            // These five are examples of collected data in a game that are meant to be tried against evolved equations.
            // Without having rounded out the engine, it... doesn't do what I want it to.
            // More so, though, I don't yet have support for defining cases, etc.
            //d = new Dictionary<string, double>();
            //d.Add("V", 2); // Bonus Multiplier
            //d.Add("W", 30); // Resource Value
            //d.Add("X", 4); // Happiness Level
            //d.Add("S", 1.25); // Dropoff Type
            //d.Add("R", 4); // Dropoff Level
            //d.Add("Y", 8); // Base Distance
            //d.Add("Z", 4); // Diag Distance
            //d.Add("U", 30); // Efficiency - Desired Value/Target Score
            //dict.Add("Case 58", d);

            //d = new Dictionary<string, double>();
            //d.Add("V", 1.1); // Bonus Multiplier
            //d.Add("W", 30); // Resource Value
            //d.Add("X", 3); // Happiness Level
            //d.Add("S", 1); // Dropoff Type
            //d.Add("R", 1); // Dropoff Level
            //d.Add("Y", 6); // Base Distance
            //d.Add("Z", 5); // Diag Distance
            //d.Add("U", 9); // Efficiency - Desired Value/Target Score
            //dict.Add("Case 1", d);

            //d = new Dictionary<string, double>();
            //d.Add("V", 1.1); // Bonus Multiplier
            //d.Add("W", 25); // Resource Value
            //d.Add("X", 3); // Happiness Level
            //d.Add("S", 1); // Dropoff Type
            //d.Add("R", 1); // Dropoff Level
            //d.Add("Y", 8); // Base Distance
            //d.Add("Z", 8); // Diag Distance
            //d.Add("U", 7); // Efficiency - Desired Value/Target Score
            //dict.Add("Case 2", d);

            //d = new Dictionary<string, double>();
            //d.Add("V", 1.1); // Bonus Multiplier
            //d.Add("W", 25); // Resource Value
            //d.Add("X", 2); // Happiness Level
            //d.Add("S", 1.25); // Dropoff Type
            //d.Add("R", 1); // Dropoff Level
            //d.Add("Y", 6); // Base Distance
            //d.Add("Z", 4); // Diag Distance
            //d.Add("U", 7); // Efficiency - Desired Value/Target Score
            //dict.Add("Case 3", d);

            //d = new Dictionary<string, double>();
            //d.Add("V", 1.1); // Bonus Multiplier
            //d.Add("W", 20); // Resource Value
            //d.Add("X", 3); // Happiness Level
            //d.Add("S", 1.25); // Dropoff Type
            //d.Add("R", 1); // Dropoff Level
            //d.Add("Y", 8); // Base Distance
            //d.Add("Z", 7); // Diag Distance
            //d.Add("U", 5); // Efficiency - Desired Value/Target Score
            //dict.Add("Case 4", d);

            // Evolve an equation such that it evaluates to the square of our single variable... ie, X**2 or X*X.
            for (int x = -10; x < 11; x++)
            {
                if (x != 0)
                {
                    d = new Dictionary<string, double>();
                    d.Add("X", x); // Our variable.
                    d.Add("Y", (double)x * x + x); // Our sought out solution.
                    dict.Add("Case " + x, d);
                }
            }

            // This case, of course, is simply coming up with the product of two variables.
            // Will need to look into parsing... this resulted in a string of 7/5*Y*X being correct - because of integer division.
            // I really don't want to have to force numbers, or variables, to floats/doubles...
            //for (int x = -3; x <= 3; x++)
            //{
            //    for (int y = -3; y <= 3; y++)
            //    {
            //        if (x != 0 && y != 0)
            //        {
            //            d = new Dictionary<string, double>();
            //            d.Add("X", x); // Our variable.
            //            d.Add("Y", y); // ZOMG another variable.
            //            d.Add("Z", x * y); // Our sought after solution.
            //            dict.Add("Case " + x + "" + y, d);
            //        }
            //    }
            //}
        }

        private static void DoShowGenome()
        {
            Console.Clear();

            int cutoff = (int)Math.Sqrt(genome.Max(n => n.Value));

            var sortedGenome = genome.Where(n => n.Value > cutoff).OrderByDescending(n => n.Value).ToList();

            foreach (KeyValuePair<string, int> kvp in sortedGenome)
            {
                Console.WriteLine(kvp.Key + " " + kvp.Value);
            }
            Console.ReadLine();
        }

        // Apparently I don't remember what this does!
        private static void DisplayOdds()
        {
            int t = 0;
            int k = 0;
            int[] odds = new int[numChrom];

            for (int i = 0; i < numChrom; i++)
            {
                bool jump = false;

                while (!jump)
                {
                    k++;
                    int j = r.Next(0, 10001);
                    int ic = r.Next(0, numChrom);

                    //Console.WriteLine("r: " + j + " v " + chromosomes[ic].Fitness);

                    if (chromosomes[ic].Fitness <= j)
                    {
                        odds[ic]++;
                        jump = true;
                    }

                    if (++t > 100)
                    {
                        odds[r.Next(0, numChrom)]++;
                        jump = true;
                        t = 0;
                    }
                }
            }

            //Console.ReadLine();
            //tFitness = 0;
            //Console.Clear();
            //Console.WriteLine("Trials: " + k);
            //foreach (int i in odds)
            //{
            //    Console.WriteLine((double) i / (double) numChrom);
            //}
            //Console.ReadLine();
        }

        private static void DoGeneration()
        {
            Chromosome c;
            Chromosome b;

            List<Chromosome> c2 = new List<Chromosome>();

            bestChromosomes = new List<Chromosome>();

            List<Chromosome> c3 = chromosomes;

            c3 = c3.OrderBy(n => n.Fitness).ToList();

            bestChromosomeString = c3[0].GeneString;
            bestChromosomeFitness = c3[0].Fitness;

            if (bestChromosomeFitness < bestEverChromosomeFitness)
            {
                bestEverChromosomeFitness = bestChromosomeFitness;
                bestEverChromosomeString = c3[0].GeneString;
                bestEverChromosomeStringValidated = c3[0].GeneStringValidated;
                bestEverChromosomeValue = c3[0].Result;
            }

            while (c2.Count < numChrom)
            {
                int j = GetSortOfRandomChromosomeIndex(c3);
                int k = GetSortOfRandomChromosomeIndex(c3);
                int v;

                c = c3[j];
                b = c3[k];

                // Crossover at a given index is currently disabled - without proper handling for lenghts, it rapidly selected for the shortest possible chromosomes.
                //Chromosome a = Chromosome.Crossover(c, b, 3);
                List<Chromosome> children;

                children = Chromosome.Crossover(c, b);

                // Currently, we force chromosomes to be minimally three genes in length. To do so, we randomly insert a gene into shorter chromosomes.
                while (children[0].Genes.Count < 3)
                {
                    children[0].Insert();
                }

                while (children[1].Genes.Count < 3)
                {
                    children[1].Insert();
                }

                // Take the child chromosomes. Record them in(to) the genome.
                // Then apply the possibility of mutations - flat mutation, insertion, deletion, or swaps.
                foreach (Chromosome a in children)
                {
                    if (genome.TryGetValue(a.GeneString, out v))
                    {
                        genome[a.GeneString]++;
                    }
                    else
                    {
                        genome.Add(a.GeneString, 1);
                    }

                    if (r.Next(0, 10000) > mutationChance)
                    {
                        a.Mutate();
                    }

                    if (r.Next(0, 10000) > insertionChance)
                    {
                        a.Insert();
                    }

                    if (r.Next(0, 10000) > deletionChance)
                    {
                        a.Delete();
                    }

                    if (r.Next(0, 10000) > swapChance)
                    {
                        a.Swap();
                    }

                    c2.Add(a);
                    CalculateFitness(a);

                    double maxFit = bestEverChromosomes.Min(n => n.Fitness);

                    if (a.Fitness < maxFit)
                    {
                        if (!bestEverChromosomes.Contains(a))
                        {
                            bestEverChromosomes.Add(a);
                            bestEverChromosomes = bestEverChromosomes.OrderBy(n => n.Fitness).ToList();
                            bestEverChromosomes.RemoveAt(8);
                        }
                    }
                }
            }

            c3 = c3.OrderByDescending(n => n.TimesBred).ToList();
            bredChromosomeString = c3[0].GeneString;
            bredChromosomeFitness = c3[0].Fitness;

            c3 = c2;
            c3 = c3.OrderBy(n => n.Fitness).ToList();

            for (int i = 0; i < 8; i++)
            {
                bestChromosomes.Add(c3[i]);
            }

            chromosomes = c2;
        }

        private static void DisplayBestChromosomes()
        {
            Console.Clear();
            int y = 4;

            foreach (Chromosome c in bestChromosomes)
            {
                DisplayChromosomeAt(c, 0, y);
                y += 6;
            }

            Console.ReadLine();
        }

        // Haha.
        // Currently, chromosomes compete in the sense that we favor fitter equations for generation.
        // So it's sort of random... within the bounds of fit(ter) equations. If we can't find decently fit equatons, we expand our search.
        private static int GetSortOfRandomChromosomeIndex(List<Chromosome> c)
        {
            int t = 0;

            int ramp = 1;

            while (true)
            {
                int i = r.Next(1, ramp + 1);
                int ic = r.Next(0, c.Count);

                if (c[ic].Fitness <= i)
                {
                    c[ic].TimesBred++;
                    return ic;
                }

                if (++t > 10)
                {
                    ramp *= 10;
                    t = 0;
                }

                if (ramp >= 10000000)
                {
                    i = r.Next(0, c.Count);
                    c[i].TimesBred++;
                    return i;
                }
            }
        }

        // Display a bunch of data that even confuses me. :3
        static void DisplayAll(List<Chromosome> c, int idx, int idx2, int t)
        {
            Console.SetCursorPosition(0, 0);
            Console.WriteLine("T: " + t + "; I: " + idx + "; I2: " + idx2 + "; P: " + page + "xLen: " + c.Average(n => n.Genes.Count) + "; xValue: " + c.Where(n => n.IsValid).Average(n => n.Result));
            Console.Write("Best Ever: " + bestEverChromosomeString + " " + bestEverChromosomeFitness + " " + bestEverChromosomeStringValidated + " = " + bestEverChromosomeValue);
            Console.WriteLine("; xFit: " + c.Average(n => n.Fitness) + "; xFitB: " + c.Average(n => n.FitnessBonus));
            Console.WriteLine("Best: " + bestChromosomeString + " " + bestChromosomeFitness);
            Console.WriteLine("Most Bred: " + bredChromosomeString + " " + bredChromosomeFitness);
            DisplayChromosomes(c, idx, idx2);

            if (solution)
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine("Solution found?");
                Console.ForegroundColor = ConsoleColor.Gray;
                solution = false;
            }
        }

        static void DisplayChromosomes(List<Chromosome> c, int idx, int idx2)
        {
            int x = 0;
            int y = 4;

            for (int i = page * 6; i < (page * 6 + 6); i++)
            {
                if (i == idx)
                {
                    Console.ForegroundColor = ConsoleColor.Blue;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Gray;
                }

                if (i == idx2)
                {
                    Console.BackgroundColor = ConsoleColor.DarkGray;
                }
                else
                {
                    Console.BackgroundColor = ConsoleColor.Black;
                }

                if (i >= numChrom) break;

                DisplayChromosomeAt(c[i], x, y);
                y += 6;

                //if (i == 7)
                //{
                //    x += 40;
                //    y = 4;
                //}
            }

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.BackgroundColor = ConsoleColor.Black;
        }

        static void DisplayChromosomeAt(Chromosome c, int x, int y)
        {
            ConsoleColor fc = Console.ForegroundColor;
            ConsoleColor bc = Console.BackgroundColor;

            Console.SetCursorPosition(x, y);
            Console.WriteLine("Raw:       " + c.GeneString);
            Console.SetCursorPosition(x, y + 1);
            Console.WriteLine("Processed: " + c.GeneStringValidated);
            Console.SetCursorPosition(x, y + 2);
            if (!c.IsValid) Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Evaluated: " + c.Result);
            Console.SetCursorPosition(x, y + 3);
            if (!c.IsValid) Console.ForegroundColor = ConsoleColor.Red;
            if (c.Fitness <= minFitness * dict.Count) Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Fitness: " + c.Fitness + " " + c.FitnessBonus);
            Console.ForegroundColor = fc;

            Console.SetCursorPosition(0, y + 4);
            Console.WriteLine("A: " + c.ParentA);
            Console.SetCursorPosition(0, y + 5);
            Console.WriteLine("B: " + c.ParentB);
        }

        static Chromosome CreateRandomChromosome()
        {
            ArrayList gs = new ArrayList();

            int n = r.Next(3, 16);
            int s = 0;

            // Static size.
            //n = 32;

            for (int i = 0; i < n; i++)
            {
                switch (r.Next(0, 3))
                {
                    case 0:
                        s = r.Next(0, (int)GeneOperators.Power + 1);
                        gs.Add(new Gene((GeneOperators)s));
                        break;
                    case 1:
                        s = r.Next(0, (int)GeneVariables.Z + 1);
                        gs.Add(new Gene((GeneVariables)s));
                        break;
                    default:
                        s = r.Next(0, 9);
                        gs.Add(new Gene(s));
                        break;
                }
            }

            return new Chromosome(gs);
        }
    }
}
