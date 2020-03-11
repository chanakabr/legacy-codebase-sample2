<?xml version="1.0" ?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt" xmlns:content="http://www.cablelabs.com/namespaces/metadata/xsd/content/1" exclude-result-prefixes="msxsl" xmlns:title="http://www.cablelabs.com/namespaces/metadata/xsd/title/1" xmlns:offer="http://www.cablelabs.com/namespaces/metadata/xsd/offer/1" >
<xsl:output method="xml" omit-xml-declaration="yes" indent="yes"/>

  <xsl:template match="/">
    <feed broadcasterName="whitelabel">
      <export>
          <xsl:apply-templates select="//*[local-name()='Title'][@type = 'ProgramTitle']"/>
      </export>
    </feed>
  </xsl:template>

  <xsl:template match="//*[local-name()='Title']">
    <xsl:element name="media">
      <xsl:attribute name="is_active">true</xsl:attribute>
      <xsl:attribute name="co_guid"><xsl:value-of select="@uriId"/></xsl:attribute>
      <xsl:attribute name="action">insert</xsl:attribute>
      <xsl:call-template name="build_basic_data"/>
    </xsl:element>
  </xsl:template>

  <xsl:template name="build_basic_data">
    <xsl:element name="basic">
      <xsl:element name="media_type">
        <xsl:value-of select="ADIUtils:HandleMediaType(title:ShowType)" />
      </xsl:element>
    </xsl:element>
  </xsl:template>
  
</xsl:stylesheet>