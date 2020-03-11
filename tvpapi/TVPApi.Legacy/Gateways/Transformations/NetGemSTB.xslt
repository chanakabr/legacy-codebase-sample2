<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl"
>
  <xsl:output method="xml" omit-xml-declaration="yes" indent="yes" cdata-section-elements="synopsis status signedURL" encoding="utf-8"/>
  <xsl:param name="chid"></xsl:param>
  <xsl:param name="devtype"></xsl:param>
  <xsl:template match="GetAllChannels">
    <xsl:element name="collections">
      <xsl:attribute name="adult">false</xsl:attribute>
      <xsl:for-each select="Channel">
        <xsl:element name="xml-link">
          <xsl:element name="name">
            <xsl:value-of select="Title"/>
          </xsl:element>
          <xsl:element name="url">
            <xsl:text disable-output-escaping="no"><![CDATA[/gateways/gateway.ashx?type=channelMedias&intChid=]]></xsl:text>
            <xsl:value-of select="Tvmch"/>
            <xsl:text disable-output-escaping="no"><![CDATA[&picsize=]]></xsl:text>
            <xsl:value-of select="Picsize"/>
            <xsl:text disable-output-escaping="no"><![CDATA[&chid=]]></xsl:text>
            <xsl:value-of select="$chid"/>
            <xsl:text disable-output-escaping="no"><![CDATA[&devtype=]]></xsl:text>
            <xsl:value-of select="$devtype"/>
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
            <xsl:value-of select="MediaID"/>
            <xsl:text>-</xsl:text>
            <xsl:value-of select="MediaTypeID"/>
            <xsl:text disable-output-escaping="no">%26chid=</xsl:text>
            <xsl:value-of select="$chid"/>
          </xsl:element>
        </xsl:for-each>
      </xsl:element>
    </xsl:element>
  </xsl:template>
  <xsl:template match="PurchaseAuthPurchase">
    <xsl:element name="purchase">
      <xsl:element name="state">
        <xsl:value-of select="state"/>
      </xsl:element>
      <xsl:element name="challenge">
        <xsl:value-of select="challenge"/>
      </xsl:element>
    </xsl:element>
  </xsl:template>
  <xsl:template match="PurchasePricePurchase">
    <xsl:element name="purchase">
      <xsl:element name="price">
        <xsl:value-of select="price"/>
      </xsl:element>
      <xsl:element name="status">
        <xsl:value-of select="status"/>
      </xsl:element>
    </xsl:element>
  </xsl:template>
  <xsl:template match="DoPurchasePurchase">
    <xsl:element name="purchase">
      <xsl:element name="vhiId">
        <xsl:value-of select="vhiId"/>
      </xsl:element>
      <xsl:element name="signedURL">
        <xsl:value-of select="signedURL"/>
      </xsl:element>
      <xsl:element name="status">
        <xsl:value-of select="status"/>
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
        <xsl:text disable-output-escaping="no">%26chid=</xsl:text>
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
          <xsl:text disable-output-escaping="no">%26mediatype=</xsl:text>
          <xsl:value-of select="MediaTypeID"/>
          <xsl:text disable-output-escaping="no">%26ppv=</xsl:text>
          <xsl:value-of select="PPVModule"/>
          <xsl:text disable-output-escaping="no">%26price=</xsl:text>
          <xsl:value-of select="Price"/>
          <xsl:text disable-output-escaping="no">%26chid=</xsl:text>
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
  <xsl:template match="GetAccountInfo/account">
    <xsl:element name="account">
      <xsl:element name="information">
        <xsl:element name="distributor">
          <xsl:value-of select="information/distributor"/>
        </xsl:element>
        <xsl:element name="contract">
          <xsl:value-of select="information/contract"/>
        </xsl:element>
        <xsl:element name="date">
          <xsl:value-of select="information/date"/>
        </xsl:element>
        <xsl:element name="pay">
          <xsl:value-of select="information/pay"/>
        </xsl:element>
        <xsl:element name="email">
          <xsl:value-of select="information/email"/>
        </xsl:element>
        <xsl:element name="logo">
          <xsl:value-of select="information/logo"/>
        </xsl:element>
      </xsl:element>
      <xsl:element name="streams">
        <xsl:for-each select="streams/stream">
          <xsl:element name="stream">
            <xsl:attribute name="id">
              <xsl:value-of select="@id"/>
            </xsl:attribute>
            <xsl:element name="title">
              <xsl:value-of select="title"/>
            </xsl:element>
            <xsl:element name="price">
              <xsl:value-of select="price"/>
            </xsl:element>
            <xsl:element name="realprice">
              <xsl:value-of select="realprice"/>
            </xsl:element>
            <xsl:element name="date">
              <xsl:value-of select="date"/>
            </xsl:element>
          </xsl:element>
        </xsl:for-each>
      </xsl:element>
    </xsl:element>
  </xsl:template>
  <xsl:template match="SearchTitles">
    <xsl:element name="collection">
      <xsl:element name="information">
        <xsl:value-of select="collection/information"/>
      </xsl:element>
      <xsl:element name="items">
        <xsl:for-each select="collection/items/id">
          <xsl:element name="id">
            <xsl:value-of select="."/>
            <xsl:text disable-output-escaping="no">%26chid=</xsl:text>
            <xsl:value-of select="$chid"/>
          </xsl:element>
        </xsl:for-each>
      </xsl:element>
    </xsl:element>
  </xsl:template>
  <xsl:template match="MediaMark">
    <xsl:element name="response">
      <xsl:attribute name="type">
        <xsl:value-of select="response/@type"/>
      </xsl:attribute>
      <xsl:attribute name="action">
        <xsl:value-of select="response/@action"/>
      </xsl:attribute>
      <xsl:value-of select="."/>
    </xsl:element>
  </xsl:template>
</xsl:stylesheet>
