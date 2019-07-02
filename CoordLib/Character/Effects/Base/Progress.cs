using BenLib.Framework;
using BenLib.Standard;
using System;
using System.Globalization;

namespace Coord
{
    internal class ProgressValueInterpolationHelper : ValueInterpolationHelper<Progress> { protected override Progress InterpolateCore(Progress start, Progress end, double progress) => new Progress(Num.Interpolate(start.Value, end.Value, progress), progress == 1 ? end.Mode : start.Mode, Num.Interpolate(start.LagFactor, end.LagFactor, progress)); }

    public readonly struct Progress
    {
        static Progress() => ValueInterpolationHelper<Progress>.Default = new ProgressValueInterpolationHelper();
        public Progress(double value) : this() => Value = value;
        public Progress(double value, ProgressMode mode, double lagFactor = 2) : this(value)
        {
            Mode = mode;
            LagFactor = lagFactor;
        }
        //public Progress(double value, ProgressMode mode, IEasingFunction lagEasingFunction, double lagFactor = 2) : this(value, mode, lagFactor) => LagEasingFunction = lagEasingFunction;

        public double Value { get; }
        public ProgressMode Mode { get; }
        public double LagFactor { get; }
        //public IEasingFunction LagEasingFunction { get; }

        public double Get(int index, int length)
        {
            switch (Mode)
            {
                case ProgressMode.LaggedStart:
                    {
                        double prop = (double)index / length;
                        //if (LagEasingFunction != null) LagEasingFunction.Ease(prop);
                        double lf = LagFactor;
                        return (lf * Value - (lf - 1) * prop).Trim(0, 1);
                    }
                case ProgressMode.OneAtATime:
                    {
                        double lower = (double)index / length;
                        double upper = (double)(index + 1) / length;
                        return ((Value - lower) / (upper - lower)).Trim(0, 1);
                    }
                default: return Value;
            }
        }

        public Progress ChangeValue(double value) => new Progress(value, Mode/*, LagEasingFunction*/, LagFactor);

        public static implicit operator Progress(double value) => new Progress(value);

        public static Progress Parse(string s)
        {
            string[] ss = s.Split(';');
            if (ss.Length != 3) throw new FormatException();
            return new Progress(double.Parse(ss[0]), (ProgressMode)int.Parse(ss[1]), double.Parse(ss[2]));
        }

        public override string ToString() => $"{Value.ToString(CultureInfo.InvariantCulture)};{((int)Mode).ToString(CultureInfo.InvariantCulture)};{LagFactor.ToString(CultureInfo.InvariantCulture)}";
    }

    public enum ProgressMode { AllAtOnce, OneAtATime, LaggedStart }

    public interface IProgressive { Progress Progress { get; set; } }
}
