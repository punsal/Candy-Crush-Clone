using Core.Random.Abstract;

namespace Core.Random
{
    public class RandomSystem : RandomSystemBase
    {
        public override float FloatValue => UnityEngine.Random.value;
        
        public RandomSystem(int seed) : base(seed)
        {
            UnityEngine.Random.InitState(seed);
        }

        public override int Next(int min, int max)
        {
            return UnityEngine.Random.Range(min, max);
        }
    }
}