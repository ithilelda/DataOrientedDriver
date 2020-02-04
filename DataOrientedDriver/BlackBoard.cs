namespace DataOrientedDriver
{
    public abstract class BlackBoard
    {
        public abstract T Get<T>(string key) where T : class;
        public abstract int GetInt(string key);
        public abstract float GetFloat(string key);
        public abstract void Post<T>(string key, T value) where T : class;
        public abstract void Post(string key, int value);
        public abstract void Post(string key, float value);
    }
}