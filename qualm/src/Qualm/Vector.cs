namespace Qualm
{
    public struct Vector
    {
        public Number X { get; set; }
        public Number Y { get; set; }

        public Vector(Number x, Number y) : this()
        {
            X = x;
            Y = y;
        }
    }
}