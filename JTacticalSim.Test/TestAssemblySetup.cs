using NUnit.Framework;

namespace JTacticalSim.Test
{
	[SetUpFixture]
	public class TestAssemblySetup
	{
		[OneTimeSetUp]
		public void Init()
		{
			// ConfigurationManager.AppSettings binds to the entry assembly's config file.
			// When running under a test runner whose host process isn't testhost.dll
			// (e.g., Rider's ReSharper runner), the config won't be found automatically.
			// Detect this and load from the test assembly's config file explicitly.
			if (ConfigurationManager.AppSettings["datafilepathDefault"] != null)
				return;

			var configPath = Path.Combine(AppContext.BaseDirectory, "JTacticalSim.Test.dll.config");
			if (!File.Exists(configPath))
				return;

			var configMap = new ExeConfigurationFileMap { ExeConfigFilename = configPath };
			var config = ConfigurationManager.OpenMappedExeConfiguration(configMap, ConfigurationUserLevel.None);

			foreach (var key in config.AppSettings.Settings.AllKeys)
			{
				ConfigurationManager.AppSettings[key] = config.AppSettings.Settings[key].Value;
			}
		}
	}
}
