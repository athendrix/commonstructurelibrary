using System;
using System.Linq;
using System.Reflection;

namespace CSL;

public record RecordParameter(string Name, Type Type, int Position, bool Nullable, Attribute[] Attributes)
{
    public RecordParameter(ParameterInfo parameterInfo) : this(parameterInfo.Name, parameterInfo.ParameterType, parameterInfo.Position, IsNullable(parameterInfo), parameterInfo.GetCustomAttributes().ToArray()) { }
    private static bool IsNullable(ParameterInfo parameter)
    {
        if (parameter.ParameterType.IsValueType) { return System.Nullable.GetUnderlyingType(parameter.ParameterType) != null; }
        foreach(CustomAttributeData customAttribute in parameter.CustomAttributes)
        {
            if(customAttribute.AttributeType.FullName == "System.Runtime.CompilerServices.NullableAttribute")
            {
                return (byte?)customAttribute.ConstructorArguments[0].Value == 2;
            }
        }

        for (MemberInfo? type = parameter.Member; type != null; type = type.DeclaringType)
        {
            foreach(CustomAttributeData customAttribute in type.CustomAttributes)
            {
                if(customAttribute.AttributeType.FullName == "System.Runtime.CompilerServices.NullableContextAttribute")
                {
                    return (byte?)customAttribute.ConstructorArguments[0].Value == 2;
                }
            }
        }

        return false;
    }
}