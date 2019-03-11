using System;
using System.Collections.Generic;

namespace WaterLog_Backend
{
    public class Forecast
    {
        public Forecast()
        {

        }

        public List<Double> generateUnixEpochFromDatetime(DateTime first, DateTime last, int numberOfElements)
        {
            
            double firstEpoch = new DateTimeOffset(first).ToUnixTimeSeconds();
            double lastEpoch = new DateTimeOffset(last).ToUnixTimeSeconds();
            double epochDifference = Math.Abs( (lastEpoch - firstEpoch))/(numberOfElements - 1);
            List<double> unixEpochDates = new List<double>();

            for(double i = 0; i < numberOfElements; i++)
            {
                unixEpochDates.Add(firstEpoch +i*epochDifference);
            }
            return unixEpochDates;
        }

        public void LinearRegression(double[] xValues, double[] yValues, out double rSquared, out double yIntercept, out double slope)
        {
            if (xValues.Length != yValues.Length)
            {
                throw new Exception("x and y values should have the same lenght");
            }

            double sumOfXValues = 0;
            double sumOfYValues = 0;
            double sumOfXSquared = 0;
            double sumOfYSquared = 0;
            double sumCodeviates = 0;
            var count = xValues.Length;

            for (var i = 0; i < count; i++)
            {
                var x = xValues[i];
                var y = yValues[i];
                sumCodeviates += x * y;
                sumOfXValues += x;
                sumOfYValues += y;
                sumOfXSquared += x * x;
                sumOfYSquared += y * y;
            }

            var ssX = sumOfXSquared - ((sumOfXValues * sumOfXValues) / count);
            var ssY = sumOfYSquared - ((sumOfYValues * sumOfYSquared) / count);

            var rNumerator = (count * sumCodeviates) - (sumOfXValues * sumOfYValues);
            var rDenom = (count * sumOfXSquared - (sumOfXValues * sumOfXValues)) * (count * sumOfYSquared - (sumOfYValues * sumOfYValues));
            var sCo = sumCodeviates - ((sumOfXValues * sumOfYValues) / count);

            var meanX = sumOfXValues / count;
            var meanY = sumOfYValues / count;
            var dblR = rNumerator / Math.Sqrt(rDenom);

            rSquared = dblR * dblR;
            yIntercept = meanY - ((sCo / ssX) * meanX);
            slope = sCo / ssX;
        }
    }
}
