<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl"
>
  <xsl:output method="xml" indent="yes"/>
  <xsl:param name="chid"></xsl:param>
  <xsl:template match="GetAllChannels">
    <xsl:for-each select="Channel">
      <xsl:element name="collections">
        <xsl:attribute name="adult">false</xsl:attribute>
        <xsl:element name="xml-link">
          <xsl:element name="name">
            <xsl:value-of select="Title"/>
          </xsl:element>
          <xsl:element name="url">
            <xsl:text disable-output-escaping="yes"><![CDATA[/gateways/gateway.ashx?type=category&intChid=]]></xsl:text>
            <xsl:value-of select="Tvmch"/>
            <xsl:text disable-output-escaping="yes"><![CDATA[&picsize=]]></xsl:text>
            <xsl:value-of select="Picsize"/>
            <xsl:text disable-output-escaping="yes"><![CDATA[&chid=]]></xsl:text>
            <xsl:value-of select="$chid"/>
          </xsl:element>
        </xsl:element>
      </xsl:element>
    </xsl:for-each>
  </xsl:template>
  <xsl:template match="GetChannelMedias">
    <xsl:element name="collection">
      <xsl:element name="items">
        <xsl:for-each select="Media">
          <xsl:element name="id">
            <xsl:value-of select="@ID"/>
            <xsl:text>-</xsl:text>
            <xsl:value-of select="@Type"/>
            <xsl:text>-</xsl:text>
            <xsl:value-of select="$chid"/>
          </xsl:element>
        </xsl:for-each>
      </xsl:element>
    </xsl:element>
  </xsl:template>
</xsl:stylesheet>
