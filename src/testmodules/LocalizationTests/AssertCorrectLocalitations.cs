using FluentAssertions;
using NUnit.Framework;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace LocalizationTests
{
    [TestFixture]
    public class AssertCorrectLocalitations
    {
        #region Methods

        private static string AssemblyDirectory()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var codebase = new Uri(assembly.CodeBase);
            var path = codebase.LocalPath;
            return new FileInfo(path).DirectoryName;
        }

        [Test]
        public void AssertDeEnEsJaPlRupt()
        {
            var p = Process.Start(Path.Combine(AssemblyDirectory(), "DeEnEsJaPlRupt.exe"));

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

        [Test]
        public void AssertEnglishGermanPolishWorks()
        {
            var p = Process.Start(Path.Combine(AssemblyDirectory(), "EnglishGermanPolish.exe"));

            p.WaitForExit(2000).Should().Be(true);
            p.ExitCode.Should().Be(0);
            Directory.Exists("en").Should().BeFalse("because C# embedds the default culture and doesn't generate satellite assemblies.");
            Directory.Exists("de").Should().BeFalse("because we embedded the culture and deleted the directory.");
            Directory.Exists("pl").Should().BeFalse("because we embedded the culture and deleted the directory.");
        }

        [Test]
        public void AssertEnglishOnlyWorks()
        {
            var p = Process.Start(Path.Combine(AssemblyDirectory(), "EnglishOnly.exe"));

            p.WaitForExit(2000).Should().Be(true);
            p.ExitCode.Should().Be(0);
            Directory.Exists("en").Should().BeFalse("because C# embedds the default culture and doesn't generate satellite assemblies.");
        }

        [Test]
        public void AssertNoLocalizationWorks()
        {
            var p = Process.Start(Path.Combine(AssemblyDirectory(), "NoLocalization.exe"));

            p.WaitForExit(2000).Should().Be(true);
            p.ExitCode.Should().Be(0);
            Directory.Exists("en").Should().BeFalse("because there are no localizations.");
        }

        #endregion Methods
    }
}