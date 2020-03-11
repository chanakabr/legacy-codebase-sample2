<?xml version="1.0" ?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt" xmlns:content="http://www.cablelabs.com/namespaces/metadata/xsd/content/1" exclude-result-prefixes="msxsl" xmlns:ADIUtils="pda:ADIUtils" xmlns:title="http://www.cablelabs.com/namespaces/metadata/xsd/title/1" xmlns:offer="http://www.cablelabs.com/namespaces/metadata/xsd/offer/1" >
  <xsl:output method="xml" omit-xml-declaration="yes" indent="yes"/>

  <!--<xsl:variable name="ImagePathConstVar">http://ibc.cdngc.net/Yes/PicsOrig/</xsl:variable>-->
  <!--<xsl:variable name="ImagePathConstVar">http://127.0.0.1/pics/</xsl:variable>-->
  <xsl:variable name="ImagePathConstVar"></xsl:variable>

  <xsl:template match="/">
    <feed broadcasterName="whitelabel">
      <export>
        <xsl:apply-templates select="//*[local-name()='Title'][@type = 'ProgramTitle']"/>
      </export>
    </feed>
  </xsl:template>

  <xsl:template match="//*[local-name()='Title'][@type = 'ProgramTitle']">
    <xsl:element name="media">
      <xsl:attribute name="is_active">true</xsl:attribute>
      <xsl:attribute name="co_guid">
        <xsl:value-of select="@uriId"/>
      </xsl:attribute>
      <xsl:attribute name="action">insert</xsl:attribute>
      <xsl:call-template name="build_basic_data"/>
      <xsl:call-template name="build_structure_data"/>
    </xsl:element>
  </xsl:template>

  <xsl:template name="build_series_structure_data">
    <xsl:element name="structure">
      <xsl:element name="strings">
        <xsl:call-template name="build_strings_data"/>
      </xsl:element>
      <xsl:element name="doubles">
        <xsl:call-template name="build_doubles_data"/>
      </xsl:element>
      <xsl:element name="booleans">
        <xsl:call-template name="build_booleans_data"/>
      </xsl:element>
      <xsl:call-template name="build_series_tags_data"/>
    </xsl:element>
  </xsl:template>

  <xsl:template name="build_series_tags_data">
    <xsl:element name="metas">
      <xsl:call-template name="build_actors_data"/>
      <xsl:call-template name="build_genre_data"/>
      <xsl:call-template name="build_subgenre_data"/>
      <xsl:if test=" //*[local-name()='Category']/*[local-name()='CategoryPath']">
        <xsl:call-template name="build_category_tag"/>
      </xsl:if>
      <xsl:if test="*[local-name()='Rating']">
        <xsl:call-template name="build_rating_tag"/>
      </xsl:if>
      <xsl:if test="//*[local-name()='Title'][@type = 'ShowTitle']">
        <xsl:call-template name="build_series_name_tag"/>
      </xsl:if>
    </xsl:element>
  </xsl:template>

  <xsl:template name="build_series_basic_data">
    <xsl:element name="basic">
      <xsl:element name="name">
        <xsl:apply-templates select="title:LocalizableTitle/title:TitleLong" />
      </xsl:element>
      <xsl:element name="description">
        <xsl:apply-templates select="title:LocalizableTitle/title:SummaryShort" />
      </xsl:element>
      <xsl:element name="media_type">
        <xsl:text>Series</xsl:text>
      </xsl:element>
      <xsl:call-template name="add_image_thumb"/>
    </xsl:element>
  </xsl:template>

  <xsl:template name="build_structure_data">
    <xsl:element name="structure">
      <xsl:element name="strings">
        <xsl:call-template name="build_strings_data"/>
      </xsl:element>
      <xsl:element name="doubles">
        <xsl:call-template name="build_doubles_data"/>
      </xsl:element>
      <xsl:element name="booleans">
        <xsl:call-template name="build_booleans_data"/>
      </xsl:element>
      <xsl:call-template name="build_tags_data"/>
    </xsl:element>
  </xsl:template>

  <xsl:template name="build_strings_data">
    <xsl:element name="meta">
      <xsl:attribute name="name">Sort title</xsl:attribute>
      <xsl:attribute name="ml_handling">unique</xsl:attribute>
      <xsl:apply-templates select="title:LocalizableTitle/title:TitleSortName" />
    </xsl:element>
    <xsl:if test="*[local-name()='Duration']">
      <xsl:element name="meta">
        <xsl:attribute name="name">DisplayRunTime</xsl:attribute>
        <xsl:attribute name="ml_handling">unique</xsl:attribute>
        <xsl:element name="value">
          <xsl:attribute name="lang">heb</xsl:attribute>
          <xsl:value-of select="*[local-name()='DisplayRunTime']"/>
        </xsl:element>
      </xsl:element>
    </xsl:if>
    <xsl:if test="//*[local-name()='bundleID']">
    <xsl:element name="meta">
      <xsl:attribute name="name">bundle ID</xsl:attribute>
      <xsl:attribute name="ml_handling">unique</xsl:attribute>
      <xsl:element name="value">
        <xsl:attribute name="lang">heb</xsl:attribute>
        <xsl:variable name="titleUriId" select="@uriId"/>
        <xsl:value-of select="//*[local-name()='bundleID'][@TitleID = $titleUriId]/@uuID"/>
      </xsl:element>
    </xsl:element>
    </xsl:if>
    <xsl:element name="meta">
      <xsl:attribute name="name">Subtitle</xsl:attribute>
      <xsl:attribute name="ml_handling">unique</xsl:attribute>
      <xsl:element name="value">
        <xsl:attribute name="lang">heb</xsl:attribute>
        <xsl:variable name="titleRef" select="@uriId"/>
        <xsl:for-each select="//*[local-name()='ContentGroup']/*[local-name()='TitleRef'][@uriId = $titleRef]">
          <xsl:variable name="subRef" select="../*[local-name()='Ext']/*[local-name()='SubtitleRef']/@uriId"/>
          <xsl:value-of select="//*[local-name()='Subtitle'][@uriId = $subRef]/*[local-name()='SourceUrl']"/>
        </xsl:for-each>
      </xsl:element>
    </xsl:element>
  </xsl:template>

  <xsl:template name="build_tags_data">
    <xsl:element name="metas">
      <xsl:call-template name="build_actors_data"/>
      <xsl:call-template name="build_genre_data"/>
      <xsl:call-template name="build_subgenre_data"/>
      <xsl:if test=" //*[local-name()='Category']/*[local-name()='CategoryPath'] and //*[local-name()='OfferType'] != 'Series' ">
        <!--<xsl:call-template name="build_category_tag"/>-->
      </xsl:if>
      <xsl:if test="*[local-name()='Rating']">
        <xsl:call-template name="build_rating_tag"/>
      </xsl:if>
      <xsl:if test="//*[local-name()='Title'][@type = 'ShowTitle']">
        <xsl:call-template name="build_series_name_tag"/>
      </xsl:if>
      <xsl:call-template name="build_product_type"/>
      <xsl:call-template name="build_product_key"/>
    </xsl:element>
  </xsl:template>

  <xsl:template name="build_product_type">
    <xsl:element name="meta">
      <xsl:attribute name="name">Product type</xsl:attribute>
      <xsl:attribute name="ml_handling">unique</xsl:attribute>
      <xsl:call-template name="set_product_type"/>
    </xsl:element>
  </xsl:template>

  <xsl:template name="build_product_key">
    <xsl:element name="meta">
      <xsl:attribute name="name">Product key</xsl:attribute>
      <xsl:attribute name="ml_handling">unique</xsl:attribute>
      <xsl:call-template name="set_product_key"/>
    </xsl:element>
  </xsl:template>

  <xsl:variable name="svodType">99999</xsl:variable>

  <xsl:template name="set_product_type">
    <xsl:element name="container">
      <xsl:element name="value">
        <xsl:attribute name="lang">heb</xsl:attribute>
        <xsl:variable name="productType" select="//*[local-name()='Offer']/*[local-name()='Ext']/*[local-name()='CaTemplateInstance']/*[local-name()='Parameters']/*[local-name()='CaParameter'][@name = 'VOD Rate Code']/@value" />
        <xsl:choose>
          <xsl:when test="$productType = $svodType">
            <xsl:text>SVOD</xsl:text>
          </xsl:when>
          <xsl:otherwise>
            <xsl:text>TVOD</xsl:text>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:element>
    </xsl:element>
  </xsl:template>

  <xsl:template name="set_product_key">
    <xsl:element name="container">
      <xsl:element name="value">
        <xsl:attribute name="lang">heb</xsl:attribute>
        <xsl:value-of select="//*[local-name()='Offer']/*[local-name()='Ext']/*[local-name()='CaTemplateInstance']/*[local-name()='Parameters']/*[local-name()='CaParameter'][@name = 'SVOD']/@value"/>
      </xsl:element>
    </xsl:element>
  </xsl:template>

  <xsl:template name="build_series_name_tag">
    <xsl:element name="meta">
      <xsl:attribute name="name">Series Name</xsl:attribute>
      <xsl:attribute name="ml_handling">unique</xsl:attribute>
      <xsl:call-template name="set_series_name"/>
    </xsl:element>
  </xsl:template>

  <!--Force heb languge workaround-->
  <xsl:template name="set_series_name">
    <xsl:element name="container">
      <xsl:for-each select="//*[local-name()='Title'][@type = 'ShowTitle']/*[local-name()='LocalizableTitle']">
        <xsl:element name="value">
          <xsl:attribute name="lang">
            heb<!--<xsl:value-of select="ADIUtils:HandleLanguage(@xml:lang)"/>-->
          </xsl:attribute>
          <xsl:value-of select="*[local-name()='TitleSortName']"/>
        </xsl:element>
      </xsl:for-each>
    </xsl:element>
  </xsl:template>

  <xsl:template name="build_rating_tag">
    <xsl:element name="meta">
      <xsl:attribute name="name">Rating</xsl:attribute>
      <xsl:attribute name="ml_handling">unique</xsl:attribute>
      <xsl:call-template name="set_rating"/>
    </xsl:element>
  </xsl:template>

  <xsl:template name="set_rating">
    <xsl:element name="container">
      <xsl:for-each select="*[local-name()='Rating']">
        <xsl:variable name="ratingValue" select="ADIUtils:HandleRating(.)"/>
        <xsl:if test="$ratingValue">
          <xsl:element name="value">
            <xsl:attribute name="lang">heb</xsl:attribute>
            <xsl:value-of select="$ratingValue"/>
          </xsl:element>
        </xsl:if>
      </xsl:for-each>
    </xsl:element>
  </xsl:template>

  <xsl:template name="build_category_tag">
    <xsl:element name="meta">
      <xsl:attribute name="name">Category</xsl:attribute>
      <xsl:attribute name="ml_handling">unique</xsl:attribute>
      <xsl:call-template name="set_category"/>
    </xsl:element>
  </xsl:template>

  <xsl:template name="set_category">
    <xsl:element name="container">
      <xsl:element name="value">
        <xsl:attribute name="lang">heb</xsl:attribute>
        <xsl:value-of select="//*[local-name()='Category']/*[local-name()='CategoryPath']"/>
      </xsl:element>
    </xsl:element>
  </xsl:template>

  <xsl:template name="build_subgenre_data">
    <xsl:element name="meta">
      <xsl:attribute name="name">Sub Genre</xsl:attribute>
      <xsl:attribute name="ml_handling">unique</xsl:attribute>
      <xsl:call-template name="set_sub_genre"/>
    </xsl:element>
  </xsl:template>

  <xsl:template name="build_genre_data">
    <xsl:element name="meta">
      <xsl:attribute name="name">Genre</xsl:attribute>
      <xsl:attribute name="ml_handling">unique</xsl:attribute>
      <xsl:call-template name="set_genre"/>
    </xsl:element>
  </xsl:template>

  <xsl:template name="set_genre">
    <xsl:for-each select="title:Genre">
      <xsl:if test="ADIUtils:HandleGenre(.,'eng')">
        <xsl:element name="container">
          <xsl:element name="value">
            <xsl:attribute name="lang">eng</xsl:attribute>
            <xsl:value-of select="ADIUtils:HandleGenre(.,'eng')"/>
          </xsl:element>
          <xsl:element name="value">
            <xsl:attribute name="lang">rus</xsl:attribute>
            <xsl:value-of select="ADIUtils:HandleGenre(.,'rus')"/>
          </xsl:element>
          <xsl:element name="value">
            <xsl:attribute name="lang">heb</xsl:attribute>
            <xsl:value-of select="ADIUtils:HandleGenre(.,'heb')"/>
          </xsl:element>
        </xsl:element>
      </xsl:if>
    </xsl:for-each>
  </xsl:template>

  <xsl:template name="set_sub_genre">
    <xsl:for-each select="title:Genre">
      <xsl:if test="ADIUtils:HandleSubGenre(.,'eng')">
        <xsl:element name="container">
          <xsl:element name="value">
            <xsl:attribute name="lang">eng</xsl:attribute>
            <xsl:value-of select="ADIUtils:HandleSubGenre(.,'eng')"/>
          </xsl:element>
          <xsl:element name="value">
            <xsl:attribute name="lang">rus</xsl:attribute>
            <xsl:value-of select="ADIUtils:HandleSubGenre(.,'rus')"/>
          </xsl:element>
          <xsl:element name="value">
            <xsl:attribute name="lang">heb</xsl:attribute>
            <xsl:value-of select="ADIUtils:HandleSubGenre(.,'heb')"/>
          </xsl:element>
        </xsl:element>
      </xsl:if>
    </xsl:for-each>

  </xsl:template>

  <xsl:template name="build_actors_data">
    <xsl:if test="not(count(title:LocalizableTitle[title:Actor]) = 0)" >
      <xsl:element name="meta">
        <xsl:attribute name="name">Cast</xsl:attribute>
        <xsl:attribute name="ml_handling">unique</xsl:attribute>
        <xsl:for-each select="title:LocalizableTitle[title:Actor][1]/title:Actor">
          <xsl:variable name="level1Count" select="position()"/>
          <xsl:element name="container">
            <xsl:for-each select="../../title:LocalizableTitle[title:Actor]">
              <xsl:variable name="level2Count" select="position()"/>
              <xsl:apply-templates select="../title:LocalizableTitle[title:Actor][$level2Count]/title:Actor[$level1Count]" />
            </xsl:for-each>
          </xsl:element>
        </xsl:for-each>
      </xsl:element>
    </xsl:if>
  </xsl:template>

  <xsl:template match="title:Actor">
    <xsl:element name="value">
      <xsl:attribute name="lang">
        <xsl:value-of select="ADIUtils:HandleLanguage(../@xml:lang)"/>
      </xsl:attribute>
      <xsl:value-of select="@fullName"/>
    </xsl:element>
  </xsl:template>

  <xsl:template name="build_doubles_data">
    <xsl:if test="*[local-name()='Year']">
      <xsl:call-template name="build_year" />
    </xsl:if>
    <xsl:if test="*[local-name()='Ext']/*[local-name()='EpisodeNo']">
      <xsl:call-template name="build_episode_no" />
    </xsl:if>
    <xsl:if test="//*[local-name()='Title'][@type = 'SeriesTitle']/*[local-name()='Ext']/*[local-name()='SeasonNo']">
      <xsl:call-template name="build_season_no" />
    </xsl:if>
    <xsl:call-template name="build_price" />
  </xsl:template>

  <xsl:template name="build_price">
    <xsl:element name="meta">
      <xsl:attribute name="name">Price</xsl:attribute>
      <xsl:variable name="full_price" select="//*[local-name()='Offer']/*[local-name()='Ext']/*[local-name()='CaTemplateInstance']/*[local-name()='Parameters']/*[local-name()='CaParameter'][@name = 'Price']/@value"/>
      <xsl:value-of select="ADIUtils:HandleDiv($full_price, 100)" />
    </xsl:element>
  </xsl:template>

  <xsl:template name="build_offer_no">
    <xsl:element name="meta">
      <xsl:attribute name="name">Offer Number</xsl:attribute>
      <xsl:value-of select="ADIUtils:GetOfferNumber()"/>
    </xsl:element>
  </xsl:template>

  <xsl:template name="build_season_no">
    <xsl:element name="meta">
      <xsl:attribute name="name">Season Number</xsl:attribute>
      <xsl:value-of select="//*[local-name()='Title'][@type = 'SeriesTitle']/*[local-name()='Ext']/*[local-name()='SeasonNo']"/>
    </xsl:element>
  </xsl:template>

  <xsl:template name="build_episode_no">
    <xsl:element name="meta">
      <xsl:attribute name="name">Episode Number</xsl:attribute>
      <xsl:value-of select="*[local-name()='Ext']/*[local-name()='EpisodeNo']"/>
    </xsl:element>
  </xsl:template>

  <xsl:template name="build_year">
    <xsl:element name="meta">
      <xsl:attribute name="name">Release year</xsl:attribute>
      <xsl:value-of select="*[local-name()='Year']"/>
    </xsl:element>
  </xsl:template>

  <xsl:template name="build_booleans_data">
  </xsl:template>

  <xsl:template name="build_basic_data">
    <xsl:element name="basic">
      <xsl:element name="name">
        <xsl:apply-templates select="title:LocalizableTitle/title:TitleLong" />
      </xsl:element>
      <xsl:element name="description">
        <xsl:apply-templates select="title:LocalizableTitle/title:SummaryShort" />
      </xsl:element>
      <xsl:element name="media_type">
        <xsl:value-of select="ADIUtils:HandleMediaType(title:ShowType)" />
      </xsl:element>
      <xsl:call-template name="add_image_thumb"/>
    </xsl:element>
  </xsl:template>

  <xsl:template name="add_image_thumb">
    <xsl:if test="//*[local-name()='ContentGroup'][@type = ShowContentGroup]">
      <xsl:variable name="posterRefVar" select="//*[local-name()='ContentGroup'][@type = ShowContentGroup]/*[local-name()='PosterRef']/@uriId"/>
      <xsl:variable name="posterSourceUrl" select="..//*[local-name()='Poster'][@uriId = $posterRefVar]/*[local-name()='SourceUrl']"/>
      <xsl:element name="thumb">
        <xsl:attribute name="url">
          <xsl:value-of select="concat($ImagePathConstVar,$posterSourceUrl)"/>
        </xsl:attribute>
      </xsl:element>
    </xsl:if>
  </xsl:template>

  <xsl:template name="build_data">
    <xsl:element name="value">
      <xsl:attribute name="lang">
        <xsl:value-of select="ADIUtils:HandleLanguage(../@xml:lang)"/>
      </xsl:attribute>
      <xsl:apply-templates />
    </xsl:element>
  </xsl:template>

  <xsl:template match="title:LocalizableTitle/title:TitleLong">
    <xsl:call-template name="build_data"/>
  </xsl:template>

  <xsl:template match="title:LocalizableTitle/title:SummaryShort">
    <xsl:call-template name="build_data"/>
  </xsl:template>

  <xsl:template match="title:LocalizableTitle/title:TitleSortName">
    <xsl:call-template name="build_data"/>
  </xsl:template>

</xsl:stylesheet>