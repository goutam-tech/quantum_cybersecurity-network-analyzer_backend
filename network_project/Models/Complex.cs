namespace network_project.Models
{
    public struct Complex
    {
        public double Real;
        public double Imaginary;

        public Complex(double real, double imaginary)
        {
            Real = real;
            Imaginary = imaginary;
        }

        public double Magnitude => Math.Sqrt(Real * Real + Imaginary * Imaginary);
        public double MagnitudeSquared => Real * Real + Imaginary * Imaginary;

        public static Complex operator +(Complex a, Complex b) {
            return new Complex(a.Real + b.Real, a.Imaginary + b.Imaginary);
        }

        public static Complex operator -(Complex a, Complex b) =>
            new Complex(a.Real - b.Real, a.Imaginary - b.Imaginary);

        public static Complex operator *(Complex a, Complex b) =>
            new Complex(a.Real * b.Real - a.Imaginary * b.Imaginary,
                        a.Real * b.Imaginary + a.Imaginary * b.Real);

        public static Complex operator *(double scalar, Complex c) =>
            new Complex(scalar * c.Real, scalar * c.Imaginary);

        public static Complex operator *(Complex c, double scalar) =>
            new Complex(scalar * c.Real, scalar * c.Imaginary);

        public static Complex Euler(double theta) =>
            new Complex(Math.Cos(theta), Math.Sin(theta));

        public override string ToString() =>
            $"({Real:F4} + {Imaginary:F4}i)";
    }
}
