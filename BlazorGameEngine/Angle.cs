using System.Diagnostics;

namespace BlazorGameEngine
{
    [DebuggerDisplay("{ToRadians()} rad | {ToDegrees()}°")]
    public readonly struct Angle : IEquatable<Angle>, IComparable<Angle>
    {
        public static readonly Angle Zero = new Angle(0);

        private readonly float radians;
        private Angle(float radians) => this.radians = radians;

        public static Angle FromDegrees(float degrees) => new Angle(degrees * MathF.PI / 180);
        public static Angle FromRadians(float radians) => new Angle(radians);

        public float ToDegrees() => radians * 180 / MathF.PI;
        public float ToRadians() => radians;

        public static Angle operator +(Angle a, Angle b) => new Angle(a.radians + b.radians);
        public static Angle operator -(Angle a, Angle b) => new Angle(a.radians - b.radians);
        public static Angle operator *(Angle a, float b) => new Angle(a.radians * b);
        public static Angle operator *(float a, Angle b) => new Angle(a * b.radians);
        public static Angle operator /(Angle a, float b) => new Angle(a.radians / b);
        public static float operator /(Angle a, Angle b) => a.radians / b.radians;
        public static Angle operator %(Angle a, Angle b) => new Angle(a.radians % b.radians);

        public static bool operator ==(Angle a, Angle b) => a.radians == b.radians;
        public static bool operator !=(Angle a, Angle b) => a.radians != b.radians;
        public static bool operator <(Angle a, Angle b) => a.radians < b.radians;
        public static bool operator >(Angle a, Angle b) => a.radians > b.radians;
        public static bool operator <=(Angle a, Angle b) => a.radians <= b.radians;
        public static bool operator >=(Angle a, Angle b) => a.radians >= b.radians;

        public bool Equals(Angle other) => radians == other.radians;
        public override bool Equals(object? obj) => obj is Angle other && Equals(other);
        public override int GetHashCode() => radians.GetHashCode();
        public int CompareTo(Angle other) => radians.CompareTo(other.radians);

        public void ToDegrees(out int degrees, out int minutes, out float seconds)
        {
            var degreesDouble = ToDegrees();
            degrees = (int)degreesDouble;
            var minutesDouble = (degreesDouble - degrees) * 60;
            minutes = (int)minutesDouble;
            seconds = (minutesDouble - minutes) * 60;
        }

        public float Sin() => MathF.Sin(radians);
        public float Cos() => MathF.Cos(radians);
        public float Tan() => MathF.Tan(radians);

        public static Angle Atan2(float y, float x) => new Angle(MathF.Atan2(y, x));
    }
}
