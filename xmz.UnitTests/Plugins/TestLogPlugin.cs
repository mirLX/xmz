using Microsoft.Extensions.Configuration;
using Neo.Plugins;

namespace Neo.UnitTests.Plugins
{
    public class TestLogPlugin : Plugin, ILogPlugin
    {
        public TestLogPlugin() : base() { }

        public string Output { set; get; }

        public override void Configure() { }

        public new void Log(string source, LogLevel level, string message)
        {
            Output = source + "_" + level.ToString() + "_" + message;
        }

        public void LogMessage(string message)
        {
            Log(message);
        }

        public bool TestOnMessage(object message)
        {
            return OnMessage(message);
        }

        public IConfigurationSection TestGetConfiguration()
        {
            return GetConfiguration();
        }

        protected override bool OnMessage(object message) => true;

        public static bool TestResumeNodeStartup()
        {
            return ResumeNodeStartup();
        }

        public static void TestSuspendNodeStartup()
        {
            SuspendNodeStartup();
        }

        public static void TestLoadPlugins(NeoSystem system)
        {
            LoadPlugins(system);
        }
    }
}
