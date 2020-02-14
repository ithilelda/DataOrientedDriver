using DataOrientedDriver;
using System.Collections.Generic;

namespace DODUnitTest
{
    public class MockBlackBoard : BlackBoard
    {
        private Dictionary<string, float> floatDict = new Dictionary<string, float>();
        private Dictionary<string, int> intDict = new Dictionary<string, int>();
        private Dictionary<string, object> objDict = new Dictionary<string, object>();
        public override void PostFloat(string key, float value) => floatDict[key] = value;
        public override void PostInt(string key, int value) => intDict[key] = value;
        public override void Post<T>(string key, T value) where T : class => objDict[key] = value;
        public override float GetFloat(string key) => floatDict[key];
        public override int GetInt(string key) => intDict[key];
        public override T Get<T>(string key) where T : class => objDict[key] as T;
    }
}