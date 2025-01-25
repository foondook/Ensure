namespace Ensure.Generator.Abstractions;

using Ensure.Generator.Models;

public interface ISpecParser
{
    Spec Parse(string content);
} 