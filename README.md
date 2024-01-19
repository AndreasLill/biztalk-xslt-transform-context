# XSLT Transform Context

There is still no out-of-the-box BizTalk solution for mapping context properties directly in XSLT.

This pipeline component enables this by executing a compiled XSLT transformation on the message and mapping the requested context properties in the XSLT document.

## How To Use

Before the pipeline component executes the XSLT transformation it reads each line for any occurrences of "msxsl:ReadContext" in the XSLT document.

The first argument of ReadContext is the name of the context property.

The second argument of ReadContext is the namspace of the property schema.

Example:
```XML
<xsl:value-of select="msxsl:ReadContext('InterchangeID', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')" />
```

## Properties

### MapAssembly
The assembly of the biztalk map.

Example:
```
BizTalk.Integration.Maps, Version=1.0.0.0, Culture=neutral, PublicKeyToken=07fa30ac1ed17a61
```

### MapName
The full name of the biztalk map.

Example:
```
BizTalk.Integration.Maps.FromSchema_To_ToSchema
```
