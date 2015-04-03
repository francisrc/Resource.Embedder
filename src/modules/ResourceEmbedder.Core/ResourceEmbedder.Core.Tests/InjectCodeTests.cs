using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using System;
using System.IO;

namespace ResourceEmbedder.Core.Tests
{
	[TestFixture]
	public class InjectCodeTests
	{
		#region Methods

		[Test]
		public void InjectCodeIntoConsoleExe()
		{
			const string file = "ConsoleTestWithInjectedCode.exe";
			if (File.Exists(file))
			{
				File.Delete(file);
			}
			File.Copy("ConsoleTest.exe", file);

			IInjectCode injector = new CecilBasedCodeInjector(Substitute.For<ILogger>());
			injector.Inject(file, CecilHelper.InjectEmbeddedResourceLoader).Should().BeTrue();

			throw new NotImplementedException();
		}

		#endregion Methods
	}
}