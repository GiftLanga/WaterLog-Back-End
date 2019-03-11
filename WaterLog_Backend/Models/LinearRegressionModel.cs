namespace WaterLog_Backend.Models
{
    public class LinearRegressionModel
    {
        public double yIntercept { get; set; }
        public double slope { get; set; }
        public double rSquared { get; set; }
        public double start { get; set; }
        public double end { get; set; }
        public double numOfElements { get; set; }
    }
}
