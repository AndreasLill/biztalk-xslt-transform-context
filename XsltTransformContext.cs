using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.BizTalk.Streaming;
using Microsoft.XLANGs.BaseTypes;
using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Xsl;

namespace BizTalk.ContextMapper.PipelineComponents
{
    [ComponentCategory(CategoryTypes.CATID_PipelineComponent)]
    [ComponentCategory(CategoryTypes.CATID_Any)]
    [System.Runtime.InteropServices.Guid("EABBD392-0E7A-4A50-B457-B316A6ED43AD")]
    public partial class XsltTransformContext : IBaseComponent, IPersistPropertyBag, Microsoft.BizTalk.Component.Interop.IComponent
    {
        #region BaseComponent Configuration

        public string Name => "XsltTransformContext";

        public string Version => "1.0.0";

        public string Description => "Transforms XSLT from a map assembly allowing context properties to be read in XSLT with 'ReadContext(ContextName, PropertySchemaNS)'.";

        [DisplayName("Map Name")]
        public string MapName { get; set; }

        [DisplayName("Map Assembly")]
        public string MapAssembly { get; set; }

        public void GetClassID(out Guid classID)
        {
            classID = new Guid("EABBD392-0E7A-4A50-B457-B316A6ED43AD");
        }

        public void InitNew()
        {
        }

        public void Load(IPropertyBag propertyBag, int errorLog)
        {
            object val = ReadPropertyBag(propertyBag, "MapName");
            if (val != null) MapName = val as string;

            val = ReadPropertyBag(propertyBag, "MapAssembly");
            if (val != null) MapAssembly = val as string;
        }

        public void Save(IPropertyBag propertyBag, bool clearDirty, bool saveAllProperties)
        {
            propertyBag.Write("MapName", MapName);
            propertyBag.Write("MapAssembly", MapAssembly);
        }

        private object ReadPropertyBag(IPropertyBag propertyBag, string propName)
        {
            object val = null;
            try
            {
                propertyBag.Read(propName, out val, 0);
            }
            catch (ArgumentException)
            {
                return val;
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message, e);
            }
            return val;
        }

        #endregion

        public IBaseMessage Execute(IPipelineContext pContext, IBaseMessage pInMsg)
        {
            TransformBase transform = GetMapTransform(MapAssembly, MapName);
            string xsltContent = ReplaceXmlContent(transform, pInMsg.Context);

            VirtualStream outStream = Transform(pInMsg.BodyPart.Data, xsltContent);
            pContext.ResourceTracker.AddResource(outStream);
            outStream.Position = 0;
            pInMsg.BodyPart.Data = outStream;

            return pInMsg;
        }

        private TransformBase GetMapTransform(string mapAssembly, string mapName)
        {
            var assembly = Assembly.Load(mapAssembly);
            var type = assembly.GetType(mapName);
            var transform = Activator.CreateInstance(type) as TransformBase;

            return transform;
        }

        private string ReplaceXmlContent(TransformBase transform, IBaseMessageContext messageContext)
        {
            StringBuilder stringBuilder = new StringBuilder();

            using (StringReader readStream = new StringReader(transform.XmlContent))
            using (StringWriter writeStream = new StringWriter(stringBuilder))
            {
                string line;
                while ((line = readStream.ReadLine()) != null)
                {
                    if (line.Contains("msxsl:ReadContext"))
                    {
                        string pattern = @"msxsl:ReadContext\(([^,]+(?:,\s*[^,]+)*)\)";
                        var values = Regex.Matches(line, pattern);

                        foreach (Match match in values)
                        {
                            string contextFull = match.Groups[0].Value;
                            string[] context = match.Groups[1].Value?.Split(',');
                            string contextName = context[0]?.Trim();
                            string contextSchema = context[1]?.Trim().Replace("'", "");
                            string contextValue = messageContext.Read(contextName, contextSchema) as string;

                            line = line.Replace(contextFull, string.Format("'{0}'", contextValue));
                        }
                    }

                    writeStream.WriteLine(line);
                }
            }

            return stringBuilder.ToString();
        }

        private VirtualStream Transform(Stream stream, string xsltContent)
        {
            XmlReader xsltReader = XmlReader.Create(new StringReader(xsltContent));
            XslCompiledTransform xsltTransform = new XslCompiledTransform();
            XsltSettings xsltSettings = new XsltSettings(false, true);

            xsltTransform.Load(xsltReader, xsltSettings, new XmlUrlResolver());
            XmlReader messageReader = XmlReader.Create(stream);
            VirtualStream virtualStream = new VirtualStream();
            xsltTransform.Transform(messageReader, null, virtualStream);

            return virtualStream;
        }
    }
}
