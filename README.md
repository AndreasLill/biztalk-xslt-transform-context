# XSLT Transform Context

There is still no out-of-the-box BizTalk solution for mapping context properties directly in XSLT.

This pipeline component enables this by executing a compiled XSLT transformation on the message and mapping the requested context properties in the XSLT document.

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
