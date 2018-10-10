namespace ProjectWithEnum
{
    /// <summary>
    /// Test project for https://github.com/MarcStan/Resource.Embedder/issues/5
    /// Cecil needs to resolve a TypeRef to write it properly when you have a const field with a TypeRef to an enum.
    /// So this project contains an enum and the ProjectForcingCecilAssemblyResolve will reference it.
    /// </summary>
    public enum MyEnum
    {
        One = 1,
        Two = 2,
        Three = 3
    }
}