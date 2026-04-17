using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Xml.Xsl;

namespace OIU.Libs.Email;


/// <summary>
/// 
/// </summary>
public static partial class XslUtility
{
    /// <summary>
    /// Base node used to generate html from xslt
    /// </summary>
    public const string Root = "root";

    /// <summary>
    /// 
    /// </summary>
    private static readonly XmlWriterSettings _settings = new() { ConformanceLevel = ConformanceLevel.Fragment };

    /// <summary>
    /// Merge the xslt into 1 element
    /// </summary>
    /// <param name="layoutXslt"></param>
    /// <param name="pagesXslt"></param>
    /// <returns></returns>
    public static string MergeXSLT(string layoutXslt, params ReadOnlySpan<string> pagesXslt)
    {
        var layoutElement = XElement.Load(new StringReader(layoutXslt));
        var childsElements = new IEnumerable<XElement>[pagesXslt.Length];
        for (var i = 0; i < pagesXslt.Length; i++)
            childsElements[i] = XElement.Load(pagesXslt[i]).Elements();

        layoutElement.AddFirst(childsElements);
        return layoutElement.ToString();
    }
    /// <summary>
    /// Generate a html from xsl content
    /// </summary>
    /// <param name="xslt">XSLT content</param>
    /// <param name="model">Object used to replace the variables inside the XSLT</param>
    /// <param name="cleanup">Clean the resulting data like remove enter and extra white spaces to shorter the resulting file</param>
    /// <returns></returns>
    public static string GenerateStringHtml(string xslt, object model, bool cleanup = false)
    {
        var xmlModel = ObjectToXml(model);

        using var writer = new StringWriter();
        using XmlWriter xmlWriter = XmlWriter.Create(writer, _settings);
        using var xmlReader = XmlReader.Create(new StringReader(xslt));
        using var xmlModelReader = XmlReader.Create(new StringReader(xmlModel));

        var xsltTransform = new XslCompiledTransform();
        xsltTransform.Load(xmlReader);
        xsltTransform.Transform(xmlModelReader, xmlWriter);

        var content = writer.ToString();
        if (!cleanup)
            return content;

        content = ReplaceEnters().Replace(content, string.Empty);
        content = FixSpacesWithDot().Replace(content, match => $". {match.Groups[^1].Value}");
        content = content.Replace("#lb#", "\n");

        return content;
    }
    /// <summary>
    /// Load the xslt from embedded resources and merge the layout with the pages
    /// </summary>
    /// <param name="assembly">Assembly where the xslt is embedded</param>
    /// <param name="layout">XSLT use as layout. Layout allow to create a common portion of the xslt</param>
    /// <param name="pages">XSLT pages used to build the final xslt</param>
    /// <returns></returns>
    public static string FromResource(Assembly assembly, string layout, params ReadOnlySpan<string> pages)
    {
        using var reader = FromResourceAsReader(assembly, layout, pages);
        return reader.ReadToEnd();
    }
    /// <summary>
    /// Load the xslt from embedded resources and merge the layout with the pages
    /// </summary>
    /// <param name="assembly"></param>
    /// <param name="layout"></param>
    /// <param name="pages"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static TextReader FromResourceAsReader(Assembly assembly, string layout, params ReadOnlySpan<string> pages)
    {
        ArgumentNullException.ThrowIfNull(layout);

        if (!pages.IsEmpty)
            return MergeXSLT(assembly, layout, pages);
        return new StreamReader(assembly.GetManifestResourceStream(layout)!);
    }

    #region Private Methods
    /// <summary>
    /// 
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    private static string ObjectToXml(object model)
    {
        using var writer = new StringWriter();
        var nsXmlSerializer = new XmlSerializerNamespaces();

        var xmlSerializer = new XmlSerializer(model.GetType(), new XmlRootAttribute(Root));
        xmlSerializer.Serialize(writer, model);

        return writer.ToString();
    }
    private static XElement LoadXslt(Assembly assembly, string resource)
    {
        var stream = assembly.GetManifestResourceStream(resource)!;
        var xmlReader = XmlReader.Create(stream);
        return XElement.Load(xmlReader);
    }
    private static StringReader MergeXSLT(Assembly assembly, string layout, params ReadOnlySpan<string> pages)
    {
        var layoutElement = LoadXslt(assembly, layout);
        var childsElements = new IEnumerable<XElement>[pages.Length];
        for (var i = 0; i < pages.Length; i++)
            childsElements[i] = LoadXslt(assembly, pages[i]).Elements();

        layoutElement.AddFirst(childsElements);
        return new StringReader(layoutElement.ToString());
    }


    [GeneratedRegex(@"\r\n\s+", RegexOptions.Compiled | RegexOptions.ECMAScript)]
    private static partial Regex ReplaceEnters();
    [GeneratedRegex(@"\.([A-Z])", RegexOptions.Compiled | RegexOptions.ECMAScript)]
    private static partial Regex FixSpacesWithDot();
    #endregion
}
