<?xml version="1.0" ?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns="http://tempuri.org/XMLSchema.xsd">
  <xsl:output method="xml" encoding="utf-16"  omit-xml-declaration="yes" indent="yes"/>

  <xsl:variable name="ImagePathConstVar"></xsl:variable>

  <xsl:template match="/">
    <feed xmlns="http://tempuri.org/XMLSchema.xsd">
      <export>
        <xsl:apply-templates select="//*[local-name()='Campaign']"/>
      </export>
    </feed>
  </xsl:template> 

  <xsl:template match="//*[local-name()='Campaign']">
    <xsl:element name="channel">
      <xsl:attribute name="co_guid"><xsl:value-of select="./*[local-name()='CampaignMetaGroup']/*[local-name()='CampaignId']"/></xsl:attribute>
      <xsl:attribute name="type">manual</xsl:attribute>
      <xsl:attribute name="action">insert</xsl:attribute>
      <xsl:element name="basic">
        <xsl:element name="name">
          <xsl:element name="value">
            <xsl:attribute name="lang">de</xsl:attribute>
            <xsl:value-of select="./*[local-name()='CampaignMetaGroup']/*[local-name()='TitleNameList']/*[local-name()='TitleName']"/>
          </xsl:element>
        </xsl:element>
        <xsl:element name="enable_feed">true</xsl:element>     
      </xsl:element>
      <xsl:element name="structure">
        <xsl:element name="cut_tags_type">and</xsl:element>
      </xsl:element>
      <xsl:element name="order_by">create date</xsl:element>
      <xsl:element name="order_direction">asc</xsl:element>
      <xsl:element name="medias">
        <xsl:for-each select="./*[local-name()='PlacementList']/*[local-name()='Placement']">
          <xsl:element name="media">
            <xsl:attribute name="ID"><xsl:value-of select="./*[local-name()='Presentation']/*[local-name()='TitleId']"/></xsl:attribute>
            <xsl:attribute name="order_number"><xsl:value-of select="./*[local-name()='PlacementSort']"/></xsl:attribute>
          </xsl:element>
        </xsl:for-each>
      </xsl:element>
    </xsl:element>
  </xsl:template>
  
</xsl:stylesheet>
