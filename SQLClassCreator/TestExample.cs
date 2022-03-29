using CSL.API;

namespace SomeNamespace.Somesubspace
{
    public record sign_ups(bool? newsletter) : APIRecord;
    public record TestExample(string? name, string? street, string? city, long? zip, string? user_name, string? catch_phrase, string? user_id, string?[] favorite_colors, DateTime? reg_date, sign_ups? sign_ups, long? long_int, long? neg_long_int, Guid? guid, double? @float) : APIRecord;
}