using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Banking
{
    public class Probability
    {
        public static Probability Singleton = new Probability();

        Random RNG = new Random();

        public int GetUniformInt(int min, int max)
        {
            return RNG.Next(min, max);
        }

        public float GetUniformFloat(float min, float max)
        {
            return (float)RNG.NextDouble() * (max - min) + min;
        }

        public int GetPoisson(float expected)
        {
            // http://en.wikipedia.org/wiki/Poisson_distribution#Generating_Poisson-distributed_random_variables
            double l = Math.Exp(-expected);
            int k = 0;
            double p = 1;
            do
            {
                ++k;
                double u = RNG.NextDouble();
                p *= u;
            }
            while (p > l);

            return k - 1;
        }

        const int GAUSSIAN_CYCLES = 20;
        public float GetGaussian(float mean, float stdev)
        {
            // Final variance is the sum of the variances of individual samples
            // variance of uniform distribution is (b - a)^2/12 -> from 0 to X -> X^2/12
            double variance_adjustment = stdev * Math.Sqrt(12) / Math.Sqrt(GAUSSIAN_CYCLES);

            double sum = 0.0f;
            for (int i = 0; i < GAUSSIAN_CYCLES; ++i)
            {
                sum += RNG.NextDouble() * variance_adjustment;
            }

            double current_mean = GAUSSIAN_CYCLES * variance_adjustment * 0.5;

            // we have a number between 0 and GAUSSIAN_CYCLES, center the distribution
            sum -= current_mean;
            sum += mean;
            return (float) sum;
        }

        public float InvExp(float x, float half_max)
        {
            // goes from -1 to 1
            //            |  __------
            //            | /
            //            |/
            // -----------+----------
            //           /|
            //        __/ |
            //  ------
            float exponent = 1.098612289f / half_max; // half_max is the point where the graph is 0.5
            float value = 2.0f / (float)(1.0 + Math.Exp(-x * exponent)) - 1.0f;
            return value;
        }
        public float InvExpDecreasing(float x, float half_max)
        {
            // goes from -1 to 1
            //            |\ 
            //            | \
            //            |  ---_____
            // -----------+----------
            float value = InvExp(x, half_max);
            value = 1 - value;
            return value;
        }

        public float Sigmoid(float x, float half_max, float three_quarters_max)
        {
            // from 0 to 1
            // |
            // |        __-------
            // |       /
            // |      /
            // |  __--
            // +-------------------
            float exponent = 1.098612289f / (three_quarters_max - half_max); // half_max is the point where the graph is 0.75, in the original, zero centred, graph
            float value = 1.0f / (float)(1.0 + Math.Exp(-(x - half_max) * exponent));
            return value;
        }

        float TransformBySigmoid(float y, float half_max, float three_quarters_max)
        {
            y = (float) Math.Max(y, 1e-5);
            y = (float) Math.Min(y, 1 - 1e-5);

            float exponent = 1.098612289f / (three_quarters_max - half_max); // half_max is the point where the graph is 0.75, in the original, zero centred, graph
            float value = (float) Math.Log(1.0 / y - 1.0) / -exponent + half_max;
            return value;
        }

        public float GetSigmoid(float max, float half_max, float three_quarters_max)
        {
            float r = (float) RNG.NextDouble() * max;
            return TransformBySigmoid(r, half_max, three_quarters_max);
        }
    }
}
