using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/*
 * I'm not currently implementing variables and constants. Variables are the harder one. We'll get to it. [26 June 2011]
 * 
 * To Do:
 *      Meta:
 *          Full comments.
 *          XML comments.
 * 
 *      Structure:
 *          Range. I need to figure out how to implement this, and ensure it works as intended.
 *              Range is a stand in for the discrete inclusion of parentheses and brackets. As requiring () [] etc adds to chromosome complexity, gene complexity, parsing complexity, etc, I want to circumvent them.
 * 
 *      Components:
 *      Numerals:
 *          ???
 *      Operators:
 *          Add more?
 *      Variables:
 *          Implement.
 *          This requires figuring out how to specify them dynamically.
 *          Parsing will need to be upgraded: either we enforce the 42x = 42 * x regime, or we fail the chromosome on that measure (or rather, invalidate part of the construct).
 *      Constants:
 *          Determine which ones to include.
 *              Non-derived constants are a given: pi, etc.
 *              I need to consider whether implementation of constants such as sqrt(2) should be included.
 *      Functions:
 *          I need to think about this.
 */
namespace GA
{
    class Gene
    {
        private int value;
        private GeneTypes type;
        private static Random r = new Random();

        public int Value
        {
            get { return value; }
            set { this.value = value; }
        }

        public GeneOperators OperatorValue
        {
            get { return (GeneOperators)value; }
            // set { this.value = value; }
        }

        public GeneVariables VariableValue
        {
            get { return (GeneVariables)value; }
        }

        public GeneTypes Type
        {
            get { return type; }
            set { this.type = value; }
        }

        public Gene()
        {
            value = 0;
            type = GeneTypes.Numeral;
        }

        public Gene(int n)
        {
            value = n;
            type = GeneTypes.Numeral;
        }

        public Gene(GeneVariables gv)
        {
            value = (int)gv;
            type = GeneTypes.Variable;
        }

        public Gene(int n, GeneTypes t)
        {
            value = n;
            type = t;
        }

        public Gene(GeneOperators go)
        {
            value = (int)go;
            type = GeneTypes.Operator;
        }

        // I need a better way of specifying the length/size of the enums for calculations.
        // Failing all else, a constant above, equated to EnumName.LastElement + 1?
        public static Gene CreateRandomGene()
        {
            int s = 0;

            switch (r.Next(0, 3))
            {
                case 0:
                    s = r.Next(0, (int)GeneOperators.Power + 1);
                    return new Gene((GeneOperators)s);
                case 1:
                    s = r.Next(0, (int)GeneVariables.Z + 1);
                    return new Gene((GeneVariables)s);
                default:
                    s = r.Next(0, 9);
                    return new Gene(s);
            }
        }

        public string GeneString
        {
            get
            {
                switch (this.type)
                {
                    case GeneTypes.Numeral:
                        return this.value.ToString();
                    case GeneTypes.Operator:
                        switch (this.OperatorValue)
                        {
                            case GeneOperators.Addition:
                                return "+";
                            case GeneOperators.Subtraction:
                                return "-";
                            case GeneOperators.Multiplication:
                                return "*";
                            case GeneOperators.Division:
                                return "/";
                            case GeneOperators.Power:
                                return "**";
                            default:
                                return "@";
                        }
                    case GeneTypes.Variable:
                        switch (this.VariableValue)
                        {
                            case GeneVariables.R:
                                return "R";
                            case GeneVariables.S:
                                return "S";
                            case GeneVariables.T:
                                return "T";
                            case GeneVariables.U:
                                return "U";
                            case GeneVariables.V:
                                return "V";
                            case GeneVariables.W:
                                return "W";
                            case GeneVariables.X:
                                return "X";
                            case GeneVariables.Y:
                                return "Y";
                            case GeneVariables.Z:
                                return "Z";
                        }

                        return "@";
                    default:
                        return "@";
                }
            }
        }
    }

    // Just these two right now. We'll figure out if there are any other "types" useful to abstract.
    enum GeneTypes
    {
        Numeral,
        Operator,
        Variable,
        Constant,
    }

    // Basic set of operators, for now.
    enum GeneOperators
    {
        Intron,
        Addition,
        Subtraction,
        Multiplication,
        Division,
        Power,
        //SquareRoot,
    }

    enum GeneVariables
    {
        Intron,
        R,
        S,
        T,
        U,
        V,
        W,
        X,
        Y,
        Z,
    }

    enum GeneConstants
    {
        PI,
        E,
        C,
        G,
        FortyTwo,
    }
}
