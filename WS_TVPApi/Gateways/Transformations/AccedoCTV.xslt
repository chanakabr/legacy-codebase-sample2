<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:tv="http://www.rss-tv.org/rss/tv1.0"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl tv"
>
  <xsl:output method="xml" omit-xml-declaration="yes" indent="yes" encoding="utf-8"/>
  <xsl:param name="tvmChannel"></xsl:param>
  <xsl:template match="GetAllChannels">
    <xsl:element name="rss">
      <!-- a small hack to show the xmlns:tv-->
      <xsl:attribute name="tv:tv"></xsl:attribute>
      <xsl:attribute name="version">2.0</xsl:attribute>
      <xsl:element name="channel">
        <xsl:element name="title">
          <xsl:text>Tvinci VOD</xsl:text>
        </xsl:element>
        <xsl:element name="tv:widget">
          <xsl:attribute name="height">200</xsl:attribute>
          <xsl:attribute name="width">300</xsl:attribute>
          <xsl:text>Photo</xsl:text>
        </xsl:element>
        <xsl:element name="baseURL">http://ibc.cdngc.net/Ipvision/pics/</xsl:element>
        <xsl:element name="tv:basehref">http://test.tvinci.com/tvpapi/gateways/gateway.ashx?type=channel</xsl:element>
        <xsl:for-each select="Channel">
          <xsl:element name="item">
            <xsl:element name="title">
              <xsl:value-of select="Title"/>
            </xsl:element>
            <xsl:element name="image">
              <xsl:attribute name="type">Photo</xsl:attribute>
              <xsl:element name="url">http://ibc.cdngc.net/Ipvision/pics/liked.jpg</xsl:element>
            </xsl:element>
            <xsl:element name="link">
              <xsl:attribute name="tv:type">rss</xsl:attribute>
              <xsl:text>/tvpapi/gateways/gateway.ashx?type=category&amp;intChid=</xsl:text>
              <xsl:value-of select="Tvmch"/>
            </xsl:element>
          </xsl:element>
        </xsl:for-each>
      </xsl:element>
    </xsl:element>
  </xsl:template>
  <xsl:template match="GetChannelInfo">
    <xsl:element name="rss">
      <!-- a small hack to show the xmlns:tv-->
      <xsl:attribute name="tv:tv"></xsl:attribute>
      <xsl:attribute name="version">2.0</xsl:attribute>
      <xsl:element name="channel">
        <xsl:element name="title">
          <xsl:value-of select="Title"/>
        </xsl:element>
        <xsl:element name="baseURL">http://ibc.cdngc.net/Ipvision/pics/</xsl:element>
        <xsl:element name="tv:basehref">http://test.tvinci.com/tvpapi/gateways/gateway.ashx?type=channelInfo</xsl:element>
        <tv:style />
        <language />
        <item>
          <title></title>
          <indirecturl />
          <indirect />
          <xsl:element name="link">
            <xsl:attribute name="tv:type">rss</xsl:attribute>
            <xsl:text>/tvpapi/gateways/gateway.ashx?type=content&amp;intChid=</xsl:text>
            <xsl:value-of select="$tvmChannel"/>
          </xsl:element>
        </item>
      </xsl:element>
    </xsl:element>
  </xsl:template>
</xsl:stylesheet>
