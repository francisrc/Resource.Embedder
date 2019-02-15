using FluentAssertions;
using Modules.TestHelper;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace LocalizationTests
{
    [TestFixture]
    public class AssertCorrectLocalizations
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
        public void AssertDeEnEsJaPlRuptWorkInConsole()
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
        public void AssertEnglishGermanPolishWorksInConsoleProject()
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

        [Test]
        public void TestEmbedTextFileInConsoleExe()
        {
            var file = Path.Combine(RepositoryLocator.Locate(RepositoryDirectory.TestFiles), "test.txt");
            var command = string.Format("\"/input:{0}\" \"/output:{1}\" {2}>ConsoleTestWithResource.exe.test.txt",
                Path.Combine(AssemblyDirectory(), "ConsoleTest.exe"),
                Path.Combine(AssemblyDirectory(), "ConsoleTestWithResource.exe"),
                file);

            // embed file into exe
            var r = Process.Start(Path.Combine(AssemblyDirectory(), "ResourceEmbedder.exe"), command);
            r.WaitForExit(5000).Should().BeTrue();
            r.ExitCode.Should().Be(0);

            // new file should now exist
            File.Exists(Path.Combine(AssemblyDirectory(), "ConsoleTestWithResource.exe")).Should().BeTrue();

            var path = Path.GetTempFileName();
            File.Delete(path);

            // run new file with command line that will extract the resource
            var r2 = Process.Start(Path.Combine(AssemblyDirectory(), "ConsoleTestWithResource.exe"), string.Format("ConsoleTestWithResource.exe.test.txt {0}", path));
            r2.WaitForExit(5000).Should().BeTrue();
            r2.ExitCode.Should().Be(0);

            // since we embedded a text file the file that was extracted should be that same file
            File.ReadAllText(path).Should().Be("Hello world!");
        }

        [Test]
        public void TestCopyLocalProjectReference()
        {
            // test for https://github.com/MarcStan/Resource.Embedder/issues/5

            /* specifically:
             * 
             * if a project reference is set to CopyLocal: false (and then e.g. copied manually via xcopy script or similar) the resource embedder may fail
             * it will only fail if the project containing the reference fullfills this requirement: https://github.com/jbevain/cecil/issues/236
             * 1. either you have a const field with a TypeRef to an enum or
             * 2. you have a custom attribute instantiated with a TypeRef to an enum.
             * 
             * The ProjectForcingCecilAssemblyResolve contains a const reference to an enum from ProjectWithEnum which is a project reference that is marked CopyLocal: False
             * This enum now forces cecil to actually look for the ProjectWithEnum assembly. Resource.Embeder v1.1.1 and prior used to wrongly look in only the target directory of ProjectForcingCecilAssemblyResolve
             * Since copy local is false, the ProjectWithEnum reference is not yet found there; crashing Resource.Embedder
             */


            // technically this test never ran with Resource.Embedder v1.1.1 or prior because a clean build always resulted in a crash and no compiled exe
            // we now use this test to assert that the exe is actually compiled which means that reference bug must now be fixed (otherwise we would get a compile error)

            // I don't know any better way to find out the current configuration
            var config =
#if DEBUG
                "Debug";
#else
                "Release";
#endif
            // locate the exe
            var file = Path.Combine(RepositoryLocator.Locate(RepositoryDirectory.SourceCode), @"testmodules\ProjectForcingCecilAssemblyResolve\bin\" + config + "\\ProjectForcingCecilAssemblyResolve.exe");

            File.Exists(file).Should().BeTrue();

            // execute it and capture the output
            var pi = new ProcessStartInfo(file, "de")
            {
                RedirectStandardOutput = true,
                UseShellExecute = false
            };
            var r2 = new Process { StartInfo = pi };
            var lines = new List<string>();
            r2.OutputDataReceived += (sender, args) =>
            {
                if (args.Data != null)
                    lines.Add(args.Data);
            };

            r2.Start();
            r2.BeginOutputReadLine();

            r2.WaitForExit(1000).Should().BeTrue();
            r2.ExitCode.Should().Be(0);

            // verify the output
            lines.Should().HaveCount(2);
            lines[0].Should().Be("Language: German");
            lines[1].Should().Be("Const enum value is: One");
        }

        #endregion Methods
    }
}
