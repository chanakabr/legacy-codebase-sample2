<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl"
>
  <xsl:output method="xml" omit-xml-declaration="yes" indent="yes" encoding="utf-8"/>
  <xsl:param name="chid"></xsl:param>
  <xsl:template match="GetAllChannels">
    <xsl:element name="collections">
      <xsl:attribute name="adult">false</xsl:attribute>
      <xsl:for-each select="Channel">
        <xsl:element name="xml-link">
          <xsl:element name="name">
            <xsl:value-of select="Title"/>
          </xsl:element>
          <xsl:element name="url">
            <xsl:text disable-output-escaping="no"><![CDATA[/gateways/gateway.ashx?type=category&intChid=]]></xsl:text>
            <xsl:value-of select="Tvmch"/>
            <xsl:text disable-output-escaping="no"><![CDATA[&picsize=]]></xsl:text>
            <xsl:value-of select="Picsize"/>
            <xsl:text disable-output-escaping="no"><![CDATA[&chid=]]></xsl:text>
            <xsl:value-of select="$chid"/>
          </xsl:element>
        </xsl:element>
      </xsl:for-each>
    </xsl:element>
  </xsl:template>
  <xsl:template match="GetChannelMedias">
    <xsl:element name="collection">
      <xsl:element name="items">
        <xsl:for-each select="Media">
          <xsl:element name="id">
            <xsl:value-of select="@ID"/>
            <xsl:text>-</xsl:text>
            <xsl:value-of select="@Type"/>
            <xsl:text disable-output-escaping="no">&amp;chid=</xsl:text>
            <xsl:value-of select="$chid"/>
          </xsl:element>
        </xsl:for-each>
      </xsl:element>
    </xsl:element>
  </xsl:template>
  <xsl:template match="GetMediaInfo">
    <xsl:element name="content">
      <xsl:element name="title">
        <xsl:value-of select="Title"/>
      </xsl:element>
      <xsl:element name="synopsis">
        <xsl:value-of select="Description"/>
      </xsl:element>
      <xsl:element name="durationInMinutes">
        <xsl:value-of select="Duration"/>
      </xsl:element>
      <xsl:element name="censorshipRating">
        <xsl:value-of select="Rating"/>
      </xsl:element>
      <xsl:element name="copyrightStudio">
        <xsl:value-of select="Copyright"/>
      </xsl:element>
      <xsl:element name="adult">
        <xsl:value-of select="Adult"/>
      </xsl:element>
      <xsl:element name="yearProd">
        <xsl:value-of select="ProductionYear"/>
      </xsl:element>
      <xsl:element name="titID">
        <xsl:value-of select="MediaID"/>
        <xsl:text>-</xsl:text>
        <xsl:value-of select="MediaTypeID"/>        
        <xsl:text disable-output-escaping="no">&amp;chid=</xsl:text>
        <xsl:value-of select="$chid"/>
      </xsl:element>
      <xsl:element name="HD">
        <xsl:value-of select="HD"/>
      </xsl:element>
      <xsl:element name="nationalityNames">
        <xsl:for-each select="Country/Name">
          <xsl:element name="element">
            <xsl:value-of select="."/>
          </xsl:element>
        </xsl:for-each>
      </xsl:element>
      <xsl:element name="actors">
        <xsl:for-each select="Actors/Name">
          <xsl:element name="xml-link">
            <xsl:element name="title">
              <xsl:value-of select="."/>
            </xsl:element>
            <xsl:element name="url">
            </xsl:element>
          </xsl:element>
        </xsl:for-each>
      </xsl:element>
      <xsl:element name="directors">
        <xsl:for-each select="Directors/Name">
          <xsl:element name="xml-link">
            <xsl:element name="title">
              <xsl:value-of select="."/>
            </xsl:element>
            <xsl:element name="url">
            </xsl:element>
          </xsl:element>
        </xsl:for-each>
      </xsl:element>
      <xsl:element name="DTRProduct">
        <xsl:element name="contentFile">
          <xsl:element name="URL">
            <xsl:value-of select="URL"/>
          </xsl:element>
          <xsl:element name="durationInMinutes">
            <xsl:value-of select="Duration"/>
          </xsl:element>
        </xsl:element>
        <xsl:element name="licenceDuration">
          <xsl:value-of select="LicenseDuration" />
        </xsl:element>
        <xsl:element name="licenceDurationUnit">
          <xsl:text>h</xsl:text>
        </xsl:element>
        <xsl:element name="price">
          <xsl:element name="price">
            <xsl:value-of select="Price"/>
          </xsl:element>
        </xsl:element>
        <xsl:element name="endDate">
          <xsl:value-of select="EndDate"/>
        </xsl:element>
        <xsl:element name="vtiID">
          <xsl:value-of select="FileID"/>
          <xsl:text>-</xsl:text>
          <xsl:value-of select="MediaTypeID"/>
          <xsl:text>-</xsl:text>
          <xsl:value-of select="PPVModule"/>
          <xsl:text>-</xsl:text>
          <xsl:value-of select="Price"/>
          <xsl:text>-</xsl:text>
          <xsl:value-of select="$chid"/>
        </xsl:element>
      </xsl:element>
      <xsl:element name="trailerContentFileList">
        <xsl:element name="element">
          <xsl:element name="URL">
            <xsl:value-of select="TrailerURL"/>
          </xsl:element>
          <xsl:element name="encodingType">
          </xsl:element>
        </xsl:element>
      </xsl:element>
      <xsl:element name="mediumImageRelativePath">
        <xsl:value-of select="PicURL"/>
      </xsl:element>
      <xsl:element name="mediumImageAbsolutePath">
        <xsl:value-of select="PicURL"/>
      </xsl:element>
    </xsl:element>
  </xsl:template>
  <xsl:template match="service">
    <xsl:element name="service">
      <xsl:attribute name="date">
        <xsl:value-of select="@date"/>
      </xsl:attribute>
      <xsl:element name="settings">
        <xsl:for-each select="settings/url">
          <xsl:element name="url">
            <xsl:attribute name="type">
              <xsl:value-of select="@type"/>
            </xsl:attribute>
            <xsl:text disable-output-escaping="yes">&lt;![CDATA[</xsl:text>
            <xsl:value-of select="normalize-space(text())" disable-output-escaping="yes"/>
            <xsl:text disable-output-escaping="yes">]]&gt;</xsl:text>
          </xsl:element>
        </xsl:for-each>
      </xsl:element>
    </xsl:element>
  </xsl:template>
</xsl:stylesheet>
