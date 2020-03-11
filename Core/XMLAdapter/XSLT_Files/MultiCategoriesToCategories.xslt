<?xml version="1.0" ?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns="http://tempuri.org/XMLSchema.xsd">
  <xsl:output method="xml" omit-xml-declaration="yes" indent="yes"/>
  <xsl:strip-space elements="*"/>

  <xsl:param name="pNamespace" select="'http://tempuri.org/XMLSchema.xsd'"/>

  <xsl:variable name="ImagePathConstVar">http://127.0.0.1/pics/</xsl:variable>

  <xsl:template match="/">
    <feed xmlns="http://tempuri.org/XMLSchema.xsd">
      <xsl:element name="export">
        <xsl:apply-templates select="//*[local-name()='classificationNode']"/>
      </xsl:element>
    </feed>
  </xsl:template>

  <xsl:template match="//*[local-name()='classificationNode']">
    <xsl:element name="category">
      <xsl:variable name="keyName" select="*[local-name()='classificationTermKey']"/>
      <xsl:attribute name="co_guid">
        <xsl:value-of select="$keyName"/>
      </xsl:attribute>
      <xsl:attribute name="order_number">
        <xsl:value-of select="./*[local-name()='peerOrderNum']"/>
      </xsl:attribute>
      <xsl:attribute name="action">insert</xsl:attribute>
      <xsl:element name="basic">
        <xsl:element name="name">
          <xsl:apply-templates select="//*[local-name()='classificationTerm'][@key = $keyName]/*[local-name()='termName']" />
        </xsl:element>
        <xsl:element name="unique_name">
          <xsl:apply-templates select="//*[local-name()='classificationTerm'][@key = $keyName]/*[local-name()='termName']" />
        </xsl:element>
        <xsl:element name="description">
          <xsl:apply-templates select="//*[local-name()='classificationTerm'][@key = $keyName]" />
        </xsl:element>
        <xsl:element name="is_virtual"><xsl:call-template name="check_if_virtual" /></xsl:element>
        <xsl:element name="thumb">
          <xsl:for-each select="//*[local-name()='classificationTerm'][@key = $keyName]/*[local-name()='termMediaSet']/*[local-name()='termMedia']">
            <xsl:variable name="posterKey" select="*[local-name()='termMediaKey']" />
            <xsl:variable name="PostfixURI" select="substring-after(//*[local-name()='media'][@key = $posterKey]/*[local-name()='mediaUri'],'http://yes.co.il//')" />
            <xsl:if test="substring-after($PostfixURI, '.') = 'JPG'" >
              <xsl:value-of select="concat($ImagePathConstVar,$PostfixURI)"/>
            </xsl:if>
          </xsl:for-each>
        </xsl:element>
      </xsl:element>
      <xsl:element name="inner_categories">
        <xsl:for-each select="//*[local-name()='parentClassificationKey'][. = $keyName]">
          <xsl:variable name="childKeyName" select="../*[local-name()='classificationTermKey']"/>
          <xsl:element name="inner_category">
            <xsl:attribute name="co_guid"><xsl:value-of select="//*[local-name()='classificationTerm'][@key = $childKeyName]/@key"/></xsl:attribute>
          </xsl:element>
        </xsl:for-each>
      </xsl:element>
      <xsl:element name="channels">
        <xsl:element name="channel">
          <xsl:attribute name="name">
            <xsl:value-of select="//*[local-name()='classificationTerm'][@key = $keyName]/*[local-name()='termDescriptions']/*[local-name()='termDescription'][@lang = 'IW']/*[local-name()='name']"/>
          </xsl:attribute>
          <xsl:attribute name="position">1</xsl:attribute>
        </xsl:element>
      </xsl:element>
    </xsl:element>
  </xsl:template>

  <xsl:template name="check_if_virtual">
    <xsl:variable name="parentNameKey" select="*[local-name()='parentClassificationKey']"/>
    <xsl:if test="*[local-name()='parentClassificationKey']">
      <xsl:for-each select="//*[local-name()='classificationNode']/*[local-name()='classificationTermKey'][. = $parentNameKey]">
        <xsl:for-each select="..">
          <xsl:call-template name="check_if_virtual" />
        </xsl:for-each>
      </xsl:for-each>
    </xsl:if>
    <xsl:variable name="keyName" select="*[local-name()='classificationTermKey']"/>
    <xsl:variable name="TermName" select="//*[local-name()='classificationTerm'][@key = $keyName]/*[local-name()='termName']"/>
    <xsl:if test="$TermName = 'סדרות'">
      <xsl:text>155</xsl:text>
    </xsl:if>
  </xsl:template>

  <xsl:template match="//*[local-name()='termName']">
    <xsl:element name="value">
      <xsl:attribute name="lang">heb</xsl:attribute>
      <xsl:value-of select="."/>
    </xsl:element>
  </xsl:template>

  <xsl:template match="//*[local-name()='classificationTerm']">
    <xsl:for-each select="*[local-name()='termDescriptions']">
      <xsl:for-each select="*[local-name()='termDescription']">
        <xsl:element name="value">
          <xsl:choose>
            <xsl:when test="@lang = 'AR'">
              <xsl:attribute name="lang">arb</xsl:attribute>
            </xsl:when>
            <xsl:when test="@lang = 'EN'">
              <xsl:attribute name="lang">eng</xsl:attribute>
            </xsl:when>
            <xsl:when test="@lang = 'IW'">
              <xsl:attribute name="lang">heb</xsl:attribute>
            </xsl:when>
            <xsl:when test="@lang = 'RU'">
              <xsl:attribute name="lang">rus</xsl:attribute>
            </xsl:when>
          </xsl:choose>
          <xsl:value-of select="*[local-name()='name']"/>
        </xsl:element>
      </xsl:for-each>
    </xsl:for-each>
  </xsl:template>

</xsl:stylesheet>
