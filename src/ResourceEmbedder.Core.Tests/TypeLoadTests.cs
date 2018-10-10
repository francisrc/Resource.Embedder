using FluentAssertions;
using NUnit.Framework;
using ResourceEmbedder.Core.GeneratedCode;
using System;

namespace ResourceEmbedder.Core.Tests
{
	[TestFixture]
	public class TypeLoadTests
	{
		#region Methods

		[Test]
		public void TypeLoadingShouldNotCrashOnInvalidInput()
		{
			try
			{
				InjectedResourceLoader.Attach();
				Type.GetType("NotExistingType, ClearlyNotExistingAssembly").Should().BeNull();
				Type.GetType("NotExistingType, https://not-an-assembly.example.com").Should().BeNull();
			}
			finally
			{
				InjectedResourceLoader.Dettach();
			}
		}

		[Test]
		public void TypeLoadingShouldWorkForValidInputs()
		{
			try
			{
				InjectedResourceLoader.Attach();
				Type.GetType(typeof(InjectedResourceLoader).AssemblyQualifiedName).Should().Be(typeof(InjectedResourceLoader));
			}
			finally
			{
				InjectedResourceLoader.Dettach();
			}
		}

		#endregion Methods
	}
}