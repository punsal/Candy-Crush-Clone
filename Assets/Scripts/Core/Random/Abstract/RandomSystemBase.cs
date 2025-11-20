namespace Core.Random.Abstract
{
    public abstract class RandomSystemBase
    {
        protected readonly int Seed;

        protected RandomSystemBase(int seed)
        {
            Seed = seed;
        }

        public abstract float FloatValue { get; }

        public abstract int Next(int min, int max);
    }
}