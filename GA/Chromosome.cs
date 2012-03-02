using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSE;

/*
 * I'm not currently implementing variables and constants. Variables are the harder one. We'll get to it. [26 June 2011]
 */
namespace GA
{
    class Chromosome
    {
        // Comments for variables are superfluous at this point.
        // Defining the variables explicitly is a temporary measure - intention is to dyamically allow variables as needed... likely requiring reflection, or dictionaries.
        private ArrayList genes;
        private static Random r = new Random();
        private bool calculated = false;
        private double? valueCalculated = null;
        private bool hasChanged = false;
        private bool isValid = false;
        public double R = 100; // Dropoff Level (assumed)
        public double S = 100; // Dropoff Bonus (guessed randomly)
        public double T = 100; // Efficiency w/o Bonus (Would be 15, but I don't want the actual result in the variable set.)
        public double U = 100; // Efficiency (Would be 30, but I don't want the actual result in the variable set.)
        public double V = 100; // Bonus (Represented as (100 + Bonus%) / 100, so here Bonus% = 100)
        public double W = 100; // Resource Value
        public double X = 3; // Happiness (Guess to be a flat value)
        public double Y = 100; // Distance A
        public double Z = 100; // Distance B

        private double fitness = 1E100;

        // For each chromosome, track its parents (if applicable).
        private string parentA = "NaN";
        private string parentB = "NaN";

        private int timesBred;

        private double fitnessBonus = 1;

        public int TimesBred
        {
            get { return timesBred; }
            set { timesBred = value; }
        }

        public string ParentA
        {
            get { return parentA; }
        }

        public string ParentB
        {
            get { return parentB; }
        }

        // Default constructor.
        public Chromosome()
        {
            genes = new ArrayList();
        }

        // We'll update this later.
        public Chromosome(ArrayList g)
        {
            genes = g;

            CalculateResult();
        }

        public double Fitness
        {
            get { return fitness; }
            set { fitness = value; }
        }

        public double FitnessBonus
        {
            get { return fitnessBonus; }
            set { fitnessBonus = value; }
        }

        // Should see if there's a better way to convert.
        public Chromosome(Gene[] gs)
        {
            genes = new ArrayList();

            foreach (Gene g in gs)
            {
                genes.Add(g);
            }

            CalculateResult();
        }

        // I forget what this one was meant to be. There's really no point in declaring a fixed size Chromosome...
        //Chromosome(int n)
        //{
        //}

        // Er. Gotta think this through. Static constructor could use two arguments, each, but instanced, no need to specify the Chromosome itself.
        //public Chromosome(Chromosome c, ChromosomeOperator co)
        //{
        //}

        //public Chromosome(Chromosome c, Chromosome d)
        //{
        //}

        /*
         * I think these three operators should be instanced. They act upon an existing Chromosome, and simply modify it.
         * Crossover, however, should be static: it takes two existing Chromosomes, and creates a new one, modifying neither.
         */
        public void Mutate()
        {
            int i = r.Next(0, genes.Count);

            genes[i] = Gene.CreateRandomGene();

            hasChanged = true;

            CalculateResult();
        }

        public void Insert()
        {
            int i = r.Next(0, genes.Count);

            genes.Insert(i, Gene.CreateRandomGene());

            hasChanged = true;

            CalculateResult();
        }

        public void Delete()
        {
            int i = r.Next(0, genes.Count);

            genes.RemoveAt(i);

            hasChanged = true;

            CalculateResult();
        }

        public void Swap()
        {
            int i = r.Next(0, genes.Count);
            int j = r.Next(0, genes.Count);

            Gene g = (Gene)genes[i];
            Gene h = (Gene)genes[j];

            genes[i] = h;
            genes[j] = g;

            hasChanged = true;

            CalculateResult();
        }

        //public static Chromosome Crossover(Chromosome c, Chromosome b)
        //{
        //    return Crossover(c, b, 1);
        //}

        /*
         * Apparently, I very much need to review this code. As noted, my first attempt(s) at making this work properly, er, didn't.
         * There may still be room for different approaches to crossover, but if I can verify that this one works as I intend,
         * and that the fossil code doesn't, I can delete it entirely.
         * 
         * Not that it can't be optimized.
         */
        /// <summary>
        /// Takes two parent Chromosomes, and returns a child Chromosome that derives from a crossover of both.
        /// </summary>
        /// <param name="c">One parent Chromosome.</param>
        /// <param name="b">Another parent Chromosome.</param>
        /// <returns>A child Chromosome.</returns>
        public static List<Chromosome> Crossover(Chromosome c, Chromosome b)
        {
            List<Chromosome> children = new List<Chromosome>();

            /*
             * Meh. My first attempt at this sucked. :)
             * Here, we take a random starting point in each Chromosome.
             * Then, we calculate a random length, no longer than the distance between the starting point and the end.
             */
            //int ics = r.Next(0, c.genes.Count);
            //int ibs = r.Next(0, b.genes.Count);
            //int icl = r.Next(1, c.genes.Count - ics);
            //int ibl = r.Next(1, b.genes.Count - ibs);

            int ic = r.Next(1, c.Genes.Count - 1);
            int ib = r.Next(1, b.Genes.Count - 1);

            //icl = Math.Max(icl, minLength);
            //ibl = Math.Max(ibl, minLength);

            //if (ics + icl > c.genes.Count)
            //{
            //    ics -= Math.Abs(ics + icl - c.genes.Count);
            //}

            //if (ibs + ibl > b.genes.Count)
            //{
            //    ibs -= Math.Abs(ibs + ibl - b.genes.Count);
            //}

            //Console.WriteLine("c.Count: " + c.genes.Count + "; b.Count: " + b.genes.Count);
            //Console.WriteLine("c.Indices: " + ics + " " + (c.genes.Count - ics) + "; b.Indices: " + ibs + " " + (b.genes.Count - ibs));
            //Console.WriteLine("c.Length: " + icl + "; b.Length: " + ibl);
            //Console.WriteLine(c.GeneString);
            //Console.WriteLine(b.GeneString);

            // Get a section of Chromosome c.
            ArrayList tcal1, tcal2; // = c.genes.GetRange(ics, icl);

            // Get a section of Chromosome b.
            ArrayList tbal1, tbal2; // = b.genes.GetRange(ibs, ibl);

            //tcal = c.genes.GetRange(0, ics);
            //tbal = b.genes.GetRange(ics, b.genes.Count - ics);

            tcal1 = c.Genes.GetRange(0, ic);
            tcal2 = c.Genes.GetRange(ic, c.Genes.Count - ic);
            tbal1 = b.Genes.GetRange(0, ib);
            tbal2 = b.Genes.GetRange(ib, b.Genes.Count - ib);

            // Create an array list to combine the sections above.
            ArrayList temp = new ArrayList();
            ArrayList temp2 = new ArrayList();

            //temp.AddRange(tcal1);
            //temp.AddRange(tbal2);

            //temp2.AddRange(tbal1);
            //temp2.AddRange(tcal2);

            int ir = r.Next(0, 2);

            //// I could have this entirely randomized - pick part of one, part of the other, at random, and then put them together, at random.
            //// For now, we'll take either a1 + b2, or b1 + a2.

            ir = 0;

            // I could do this in ternary, but this is just clearer.
            switch (ir)
            {
                // Add c first.
                case 0:
                    temp.AddRange(tcal1);
                    temp.AddRange(tbal2);

                    break;
                // Add b first.
                case 1:
                    temp.AddRange(tbal2);
                    temp.AddRange(tcal1);

                    break;
            }

            ir = r.Next(0, 2);

            ir = 0;

            switch (ir)
            {
                // Add c first.
                case 0:
                    temp2.AddRange(tbal1);
                    temp2.AddRange(tcal2);

                    break;
                // Add b first.
                case 1:
                    temp2.AddRange(tcal2);
                    temp2.AddRange(tbal1);

                    break;
            }

            // Create a Chromosome from the resulting combination of ranges.
            Chromosome t = new Chromosome(temp);
            t.parentA = c.GeneString;
            t.parentB = b.GeneString;

            Chromosome t2 = new Chromosome(temp2);
            t2.parentA = b.GeneString;
            t2.parentB = c.GeneString;

            //Console.WriteLine(c.genes.Count);
            //Console.WriteLine(b.genes.Count);

            //Console.WriteLine(t.genes.Count);
            //Console.WriteLine(t2.genes.Count);

            // Testing.
            //Chromosome tc = new Chromosome(tcal);
            //Chromosome tb = new Chromosome(tbal);

            //Console.WriteLine(tc.GeneString);
            //Console.WriteLine(tb.GeneString);

            //Console.WriteLine(t.parentA);
            //Console.WriteLine(t.parentB);
            //Console.WriteLine(t.GeneString);

            children.Add(t);
            children.Add(t2);

            return children;
        }

        // Properties
        public ArrayList Genes
        {
            get { return genes; }
        }

        public bool IsValid
        {
            get
            {
                return isValid;
            }
        }

        public void ForceCalculate()
        {
            CalculateResult();
        }

        private void CalculateResult()
        {
            isValid = true;

            double result = 0;

            try
            {
                object meh = CsEval.Eval(this, this.GeneStringValidated);
                result = Convert.ToDouble(meh);
            }
            catch (DivideByZeroException)
            {
                result = int.MaxValue;
                isValid = false;
            }

            if (Double.IsPositiveInfinity(result))
            {
                isValid = false;
            }
            else if (Double.IsNegativeInfinity(result))
            {
                isValid = false;
            }
            else if (Double.IsNaN(result))
            {
                isValid = false;
            }

            calculated = true;
            valueCalculated = (double)result;
            hasChanged = false;
        }

        public double Result
        {
            get
            {
                if (!calculated || hasChanged)
                {
                    CalculateResult();
                    return (double)valueCalculated;
                }
                else
                {
                    return (double)valueCalculated;
                }
            }
        }

        public string GeneString
        {
            get
            {
                string ch = "";

                foreach (Gene g in genes)
                {
                    ch = ch + g.GeneString;
                }

                return ch;
            }
        }

        // Huh?
        public string GeneStringValidated
        {
            get
            {
                string ch = this.GeneString;

                while (!IsChromosomeValid(ref ch))
                {
                }

                return ch;
            }
        }

        // This chokes on strings that are composed entirely of operators.
        // Further, this allows the construction of "x***y" - that's invalid, and causes the evaluator to choke.
        private bool IsChromosomeValid(ref string s)
        {
            char[] chars = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };

            if (s.IndexOfAny(chars) == -1)
            {
                s = Double.NaN.ToString();
                s = "Double.NaN";
                //s = "0/0";
                return true;
            }
            //            string ts = s;

            s = s.Replace("@", "");

            // This should nicely remove annoying strings of too-long "*".
            // We'll do this better eventually.
            s = s.Replace("******", "**");
            s = s.Replace("*****", "**");
            s = s.Replace("****", "**");
            s = s.Replace("***", "**");
            s = s.Replace("//", "/");
            s = s.Replace("++", "+");
            s = s.Replace("--", "-");

            // Strip out double operators.
            for (int i = 0; i < s.Length - 1; i++)
            {
                //Console.WriteLine("0: " + s[i] + " ; 1: " + s[i+1]);
                //Console.WriteLine("0: " + IsGeneOperator(s[i]) + "; 1: " + IsGeneOperator(s[i + 1]));
                //Console.WriteLine("0: " + s[i].Equals('*') + "; 1: " + s[i + 1].Equals('*') + "; 2: " + !(s[i].Equals('*') && s[i + 1].Equals('*')));

                if (IsGeneOperator(s[i]) && IsGeneOperator(s[i + 1]))
                {
                    if (!(s[i].Equals('*') && s[i + 1].Equals('*')))
                    {
                        if (!
                            (s[i].Equals('*') && s[i + 1].Equals('+'))
                            ||
                            (s[i].Equals('*') && s[i + 1].Equals('-'))
                            ||
                            (s[i].Equals('/') && s[i + 1].Equals('+'))
                            ||
                            (s[i].Equals('/') && s[i + 1].Equals('-'))
                            )
                        {
                            s = s.Remove(i + 1, 1);

                            return false;
                        }
                    }
                }
            }

            // For evaluation purposes, we need to specify explicit multiplication: 1X or X1 are not valid C# expressions.
            for (int i = 0; i < s.Length - 1; i++)
            {
                //Console.ForegroundColor = ConsoleColor.Red;
                //Console.SetCursorPosition(0, 40);
                //Console.WriteLine("                                           ");
                //Console.SetCursorPosition(0, 40);
                //Console.WriteLine(s);

                if (IsGeneVariable(s[i]) && IsGeneNumeral(s[i + 1]))
                {
                    s = s.Insert(i + 1, "*");
                    //Console.SetCursorPosition(0, 35);
                    //Console.WriteLine("X1");
                }
                else if (IsGeneNumeral(s[i]) && IsGeneVariable(s[i + 1]))
                {
                    s = s.Insert(i + 1, "*");
                    //Console.SetCursorPosition(0, 35);
                    //Console.WriteLine("1X");
                }
                else if (IsGeneVariable(s[i]) && IsGeneVariable(s[i + 1]))
                {
                    s = s.Insert(i + 1, "*");
                    //Console.SetCursorPosition(0, 35);
                    //Console.WriteLine("XX");
                }

                //Console.SetCursorPosition(0, 42);
                //Console.WriteLine("                                           ");
                //Console.SetCursorPosition(0, 41);
                //Console.WriteLine("                                           ");
                //Console.SetCursorPosition(0, 41);
                //Console.WriteLine(s);
                //Console.ForegroundColor = ConsoleColor.Gray;
            }

            // I'm uncertain if these are penalized at all, or simply ignored. If penalized, it may be through length penalization.

            // Trailing operators are ignored.
            if (IsGeneOperator(s[s.Length - 1]))
            {
                s = s.Substring(0, s.Length - 1);
                return false;
            }

            // Leading operators are ignored - except that leading +- is ok.
            if (IsGeneOperator(s[0]))
            {
                if (!(s[0].Equals('+') || s[0].Equals('-')))
                {
                    s = s.Substring(1, s.Length - 1);
                    return false;
                }
            }

            return true;
        }

        static private bool IsGeneOperator(char s)
        {
            if ("+-*/".Contains(s))
            {
                return true;
            }

            return false;
        }

        static private bool IsGeneNumeral(char s)
        {
            if ("0123456789".Contains(s))
            {
                return true;
            }

            return false;
        }

        static private bool IsGeneVariable(char s)
        {
            if ("RSTUVWXYZ".Contains(s))
            {
                return true;
            }

            return false;
        }
    }

    // Maybe??
    enum ChromosomeOperator
    {
        Identity,
        Crossover,
        Mutate,
        Insert,
        Delete
    }
}
