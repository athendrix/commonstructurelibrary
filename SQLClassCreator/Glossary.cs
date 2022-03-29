using CSL.API;

namespace Namespace.Data
{
    public record globject(glossary glossary) : APIRecord;
    public record glossary(string? title, GlossDiv? GlossDiv) : APIRecord;
    public record GlossDiv(string? title, GlossList? GlossList) : APIRecord;
    public record GlossList(GlossEntry? GlossEntry) : APIRecord;
    public record GlossEntry(string? ID, string? SortAs, string? GlossTerm, string? Acronym, string? Abbrev, GlossDef? GlossDef, string? GlossSee) : APIRecord;
    public record GlossDef(string? para, string?[] GlossSeeAlso) : APIRecord;
}