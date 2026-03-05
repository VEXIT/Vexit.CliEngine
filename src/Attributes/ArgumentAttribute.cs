/************************************************
 *
 * Copyright:		© 2025 VEXIT ®, www.vexit.com, Tomorrow is today...
 * Author:      	Vex Tatarevic
 * Date Created:    2025-09-15 - Initial creation
 * DateUpdated:		2025-10-27	| Vex | Redesign: split positional ArgumentAttribute and OptionAttribute; removed generic version, type inferred from property
 *
 ************************************************/

using System.Collections;
using System.Reflection;

namespace Vexit.CliEngine.Attributes;

/// <summary>
/// Positional argument attribute. Type is automatically inferred from the property type.
/// Binds by property declaration order.
/// Example: [Argument("template", "The project template to create.")]
/// Applied to: public string Template { get; set; }
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class ArgumentAttribute : Attribute
{
    public string Name { get; }
    public string Description { get; }
    public bool IsRequired { get; set; } = false;
    internal bool IsMultiValue { get; private set; }
    public int MinCount { get; set; } = 0;
    public int MaxCount { get; set; } = 0; // 0 means unlimited
    public string? ValidationMessage { get; set; }

    public ArgumentAttribute(string name, string description = "", bool isRequired = false, int minCount = 0, int maxCount = 0, string? validationMessage = null)
    {
        Name = name;
        Description = description;
        IsRequired = isRequired;
        MinCount = minCount;
        MaxCount = maxCount;
        ValidationMessage = validationMessage;
    }

    /// <summary>
    /// Lifecycle hook invoked by the parser during metadata discovery, immediately after
    /// this attribute is read from a property (e.g., in CliParser.GetDefinitions). Not
    /// called from the constructor because the constructor has no access to the target
    /// <see cref="PropertyInfo"/>.
    ///
    /// Initializes internal flags based on the decorated property. Specifically, sets
    /// <c>IsMultiValue</c> to true when the property type implements <see cref="IEnumerable"/>
    /// and is not <see cref="string"/>, enabling variadic positionals/collecting options.
    /// </summary>
    /// <param name="prop">The reflected property that this attribute is applied to.</param>
    internal void OnInitProp(PropertyInfo prop)
    {
        IsMultiValue = typeof(IEnumerable).IsAssignableFrom(prop.PropertyType) && prop.PropertyType != typeof(string);
    }
}