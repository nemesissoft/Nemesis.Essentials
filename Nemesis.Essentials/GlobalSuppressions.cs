// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Style", "IDE0057:Use range operator", Justification = "Multiple targets", Scope = "namespaceanddescendants", Target = "~N:Nemesis.Essentials")]
[assembly: SuppressMessage("GeneratedRegex", "SYSLIB1045:Convert to 'GeneratedRegexAttribute'.", Justification = "Multiple targets", Scope = "namespaceanddescendants", Target = "~N:Nemesis.Essentials")]
[assembly: SuppressMessage("Style", "IDE0056:Use index operator", Justification = "Multiple targets", Scope = "namespaceanddescendants", Target = "~N:Nemesis.Essentials")]

//reflection
[assembly: SuppressMessage("Major Code Smell", "S3011:Reflection should not be used to increase accessibility of classes, methods, or fields", Justification = "Reflection", Scope = "namespaceanddescendants", Target = "~N:Nemesis.Essentials.Design")]
[assembly: SuppressMessage("Major Code Smell", "S3011:Reflection should not be used to increase accessibility of classes, methods, or fields", Justification = "Reflection", Scope = "namespaceanddescendants", Target = "~N:Nemesis.Essentials.Runtime")]

//JetBrains.Annotations
[assembly: SuppressMessage("Minor Code Smell", "S2344:Enumeration type names should not have \"Flags\" or \"Enum\" suffixes", Justification = "JetBrains.Annotations", Scope = "namespaceanddescendants", Target = "~N:JetBrains.Annotations")]
[assembly: SuppressMessage("Minor Code Smell", "S101", Justification = "JetBrains.Annotations", Scope = "namespaceanddescendants", Target = "~N:JetBrains.Annotations")]
[assembly: SuppressMessage("Minor Code Smell", "S4070", Justification = "JetBrains.Annotations", Scope = "namespaceanddescendants", Target = "~N:JetBrains.Annotations")]
[assembly: SuppressMessage("Minor Code Smell", "S1133", Justification = "JetBrains.Annotations", Scope = "namespaceanddescendants", Target = "~N:JetBrains.Annotations")]

