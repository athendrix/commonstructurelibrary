// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Style", "IDE0057:Use range operator", Justification = "I prefer Substring over the range operator.", Scope = "namespaceanddescendants", Target = "~N:CSL")]
[assembly: SuppressMessage("Style", "IDE0063:Use simple 'using' statement", Justification = "I prefer the using block over the statement because it's more explicit.", Scope = "namespaceanddescendants", Target = "~N:CSL")]
[assembly: SuppressMessage("Style", "IDE0090:Use 'new(...)'", Justification = "It's less explicit, and doesn't increase readability.", Scope = "namespaceanddescendants", Target = "~N:CSL")]
[assembly: SuppressMessage("Style", "IDE0034:Simplify 'default' expression", Justification = "It's less explicit and doesn't increase readability.", Scope = "namespaceanddescendants", Target = "~N:CSL")]
