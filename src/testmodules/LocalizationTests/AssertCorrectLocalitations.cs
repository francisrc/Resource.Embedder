using FluentAssertions;
using NUnit.Framework;
using System.Diagnostics;
using System.IO;

namespace LocalizationTests
{
	[TestFixture]
	public class AssertCorrectLocalitations
	{
		[Test]
		public void AssertEnglishOnlyWorks()
		{
			var p = Process.Start("EnglishOnly.exe");

			p.WaitForExit(2000).Should().Be(true);
			p.ExitCode.Should().Be(0);
			Directory.Exists("en").Should().BeFalse("because C# embedds the default culture and doesn't generate satellite assemblies.");
		}

		[Test]
		public void AssertEnglishGermanPolishWorks()
		{
			var p = Process.Start("EnglishGermanPolish.exe");

			p.WaitForExit(2000).Should().Be(true);
			p.ExitCode.Should().Be(0);
			Directory.Exists("en").Should().BeFalse("because C# embedds the default culture and doesn't generate satellite assemblies.");
			Directory.Exists("de").Should().BeFalse("because we embedded the culture and deleted the directory.");
			Directory.Exists("pl").Should().BeFalse("because we embedded the culture and deleted the directory.");
		}

		[Test]
		public void AssertNoLocalizationWorks()
		{
			var p = Process.Start("NoLocalization.exe");

			p.WaitForExit(2000).Should().Be(true);
			p.ExitCode.Should().Be(0);
			Directory.Exists("en").Should().BeFalse("because there are no localizations.");
		}

		[Test]
		public void AssertDeEnEsJaPlRupt()
		{
			var p = Process.Start("DeEnEsJaPlRupt.exe");

			p.WaitForExit(2000).Should().Be(true);
			p.ExitCode.Should().Be(0);
			Directory.Exists("en").Should().BeFalse("because C# embedds the default culture and doesn't generate satellite assemblies.");
			Directory.Exists("de").Should().BeFalse("because we embedded the culture and deleted the directory.");
			Directory.Exists("es").Should().BeFalse("because we embedded the culture and deleted the directory.");
			Directory.Exists("ja").Should().BeFalse("because we embedded the culture and deleted the directory.");
			Directory.Exists("pl").Should().BeFalse("because we embedded the culture and deleted the directory.");
			Directory.Exists("ru").Should().BeFalse("because we embedded the culture and deleted the directory.");
			Directory.Exists("up-BR").Should().BeFalse("because we embedded the culture and deleted the directory.");
		}
	}
}
