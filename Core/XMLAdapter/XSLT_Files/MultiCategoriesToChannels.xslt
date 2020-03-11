<?xml version="1.0" ?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns="http://tempuri.org/XMLSchema.xsd">
  <xsl:output method="xml" encoding="utf-16"  omit-xml-declaration="yes" indent="yes"/>

  <xsl:variable name="ImagePathConstVar">http://127.0.0.1/pics/</xsl:variable>

  <xsl:template match="/">
    <feed xmlns="http://tempuri.org/XMLSchema.xsd">
      <export>
        <xsl:apply-templates select="//*[local-name()='classificationNode']"/>
      </export>
    </feed>
  </xsl:template>

  <xsl:template match="//*[local-name()='classificationNode']">
    <xsl:element name="channel">
      <xsl:variable name="keyName" select="*[local-name()='classificationTermKey']"/>
      <xsl:attribute name="co_guid"><xsl:value-of select="$keyName"/></xsl:attribute>
      <xsl:attribute name="type">auto</xsl:attribute>
      <xsl:attribute name="action">insert</xsl:attribute>
      <xsl:element name="basic">
        <xsl:element name="name">
          <xsl:apply-templates select="//*[local-name()='classificationTerm'][@key = $keyName]" />
        </xsl:element>
        <xsl:element name="unique_name">
          <xsl:apply-templates select="//*[local-name()='classificationTerm'][@key = $keyName]" />
        </xsl:element>
        <xsl:element name="description">
          <xsl:apply-templates select="//*[local-name()='classificationTerm'][@key = $keyName]" />
        </xsl:element>
        <xsl:element name="enable_feed">true</xsl:element>
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
      <xsl:element name="structure">
        <xsl:element name="cut_tags_type">and</xsl:element>
        <xsl:element name="media_type"></xsl:element>
        <xsl:element name="tags_metas">
          <xsl:element name="tags_meta">
            <xsl:attribute name="name">Category</xsl:attribute>
            <xsl:element name="container">
              <xsl:element name="value"><xsl:attribute name="lang">heb</xsl:attribute>
                <xsl:call-template name="category_build" />
              </xsl:element>
            </xsl:element>
          </xsl:element>
        </xsl:element>
      </xsl:element>
      <xsl:element name="order_by">create date</xsl:element>
      <xsl:element name="order_direction">asc</xsl:element>
    </xsl:element>
  </xsl:template>

  <xsl:template match="//*[local-name()='classificationTermKey']">
    <xsl:apply-templates select=".." mode="rec" />
  </xsl:template>

  <xsl:template match="//*[local-name()='classificationNode']" mode="rec">
    <xsl:call-template name="category_build" />
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
  
  <xsl:template name="category_build">
    <xsl:variable name="parentNameKey" select="*[local-name()='parentClassificationKey']"/>
    <xsl:if test="*[local-name()='parentClassificationKey']">
      <xsl:apply-templates select="//*[local-name()='classificationNode']/*[local-name()='classificationTermKey'][. = $parentNameKey]"/>
      <xsl:text>/</xsl:text> 
    </xsl:if>
    <xsl:variable name="keyName" select="*[local-name()='classificationTermKey']"/>
    <xsl:variable name="TermName" select="//*[local-name()='classificationTerm'][@key = $keyName]/*[local-name()='termDescriptions']/*[local-name()='termDescription'][@lang = 'IW']/*[local-name()='name']"/>
    <xsl:value-of select="$TermName"/>
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

  <xsl:template match="//*[local-name()='termName']">
    <xsl:element name="value">
      <xsl:attribute name="lang">heb</xsl:attribute>
      <xsl:value-of select="."/>
    </xsl:element>
  </xsl:template>

  <!--<xsl:template match="classificationNode">
    <xsl:value-of select="position()"/>
    <xsl:element name="feed">
      <xsl:attribute name="co_guid"></xsl:attribute>
      <xsl:attribute name="type">auto</xsl:attribute>
      <xsl:attribute name="action">insert</xsl:attribute>
    </xsl:element>
  </xsl:template>-->

</xsl:stylesheet>
