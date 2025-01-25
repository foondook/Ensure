namespace Ensure.Generator.Abstractions;

using Ensure.Generator.Models;

public interface ICodeGenerator
{
    string GenerateSteps(Spec spec, string namespaceName, string className);
    string GenerateTests(Spec spec, string namespaceName, string className);
} 