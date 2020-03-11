<?xml version="1.0" ?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl" xmlns:ADIUtils="pda:ADIUtils" xmlns:offer="http://www.cablelabs.com/namespaces/metadata/xsd/offer/1" >
  <xsl:output method="xml" omit-xml-declaration="yes" indent="yes"/>

  <xsl:variable name="prefixURLPhysicalFile">
    <xsl:text></xsl:text>
  </xsl:variable>
  <xsl:variable name="ImagePathConstVar">
    <xsl:text></xsl:text>
  </xsl:variable>

  <xsl:variable name="smallcase" select="'abcdefghijklmnopqrstuvwxyz'" />
  <xsl:variable name="uppercase" select="'ABCDEFGHIJKLMNOPQRSTUVWXYZ'" />

  <xsl:template match="/">
    <feed broadcasterName="whitelabel">
      <export>
        <xsl:apply-templates select="//*[local-name() = 'Presentation']"/>
      </export>
    </feed>
  </xsl:template>

  <xsl:template match="//*[local-name() = 'Presentation']">
    <xsl:call-template name="handle_media"/>
  </xsl:template>

  <xsl:template name="handle_media">
    <xsl:element name="media">
      <xsl:variable name="isMediaVerifed" select="./*[local-name() = 'PresentationMetaGroup']/*[local-name() = 'PresentationStatus']"/>
        <xsl:attribute name="is_active">true</xsl:attribute>
      <xsl:attribute name="co_guid">
        <xsl:value-of select="./*[local-name() = 'PresentationMetaGroup']/*[local-name() = 'Title']/*[local-name() = 'TitleId']"/>
      </xsl:attribute>
      <xsl:attribute name="action">insert</xsl:attribute>
      <xsl:call-template name="build_basic_data"/>
      <xsl:call-template name="build_structure_data"/>
    </xsl:element>
  </xsl:template>

  <xsl:template name="build_basic_data">
    <xsl:element name="basic">
      <xsl:element name="name">
          <xsl:element name="value">
          <xsl:attribute name="lang">
            <xsl:call-template name="Translate_Langueage"/>
          </xsl:attribute>
          <xsl:value-of select="./*[local-name() = 'PresentationMetaGroup']/*[local-name() = 'Title']/*[local-name() = 'TitleNameList']/*[local-name() = 'TitleName']"/>
          </xsl:element>
      </xsl:element>
      <xsl:element name="description">
          <xsl:element name="value">
          <xsl:attribute name="lang">
            <xsl:call-template name="Translate_Langueage"/>
          </xsl:attribute>
          <xsl:value-of select="./*[local-name() = 'PresentationMetaGroup']/*[local-name() = 'Title']/*[local-name() = 'SynopsisList']/*[local-name() = 'Synopsis']"/>
          </xsl:element>
      </xsl:element>
      <xsl:element name="media_type">
        <xsl:text>series</xsl:text>
      </xsl:element>
      <xsl:element name="rules">       
        <xsl:element name="watch_per_rule">
          <xsl:text>Parent account allowed</xsl:text>
        </xsl:element>
      </xsl:element>
      <xsl:element name="dates">
        <xsl:element name="catalog_start">
          <xsl:call-template name="SetStartViewDate"/>
        </xsl:element>
        <xsl:element name="catalog_end">
          <xsl:call-template name="SetEndViewDate"/>
        </xsl:element>
        <xsl:element name="start">
          <xsl:call-template name="SetFStartViewDate"/>
        </xsl:element>
        <xsl:element name="final_end">
          <xsl:call-template name="SetFEndViewDate"/>
        </xsl:element>
      </xsl:element>
      
      <xsl:element name="thumb">
        <xsl:attribute name="url">
          <xsl:for-each select="./*[local-name() = 'PromotionContentList']/*[local-name() = 'PromotionContent']/*[local-name() = 'ContentTypeDisplay'][translate(., $uppercase, $smallcase) = 'cover']">
            <xsl:if test="../*[local-name() = 'FormatList']/*[local-name() = 'Format']/*[local-name() = 'AspectRatio'] = '2:3'">
              <xsl:value-of select="../*[local-name() = 'FormatList']/*[local-name() = 'Format']/*[local-name() = 'StartUrl']"/>
            </xsl:if>
            </xsl:for-each>
        </xsl:attribute>
      </xsl:element>

      <xsl:element name="pic_ratios">
        <xsl:element name="ratio">
          <xsl:attribute name="thumb">
            <xsl:for-each select="./*[local-name() = 'PromotionContentList']/*[local-name() = 'PromotionContent']/*[local-name() = 'ContentTypeDisplay'][translate(., $uppercase, $smallcase) = 'teaser']">
              <xsl:if test="../*[local-name() = 'FormatList']/*[local-name() = 'Format']/*[local-name() = 'AspectRatio'] = '16:9'">
                <xsl:value-of select="../*[local-name() = 'FormatList']/*[local-name() = 'Format']/*[local-name() = 'StartUrl']"/>
              </xsl:if>
            </xsl:for-each>
          </xsl:attribute>
          <xsl:attribute name="ratio">
            <xsl:text>16:9</xsl:text>
          </xsl:attribute>
        </xsl:element>
      </xsl:element>      
    </xsl:element>
  </xsl:template>

  <xsl:template name="SetEndViewDate">
    <xsl:choose>
      <xsl:when test="not(./*[local-name() = 'PresentationMetaGroup']/*[local-name() = 'VisibilityPeriod']/*[local-name() = 'VisibilityPeriodEnd'] = '')">
        <xsl:variable name="fullTime" select="./*[local-name() = 'PresentationMetaGroup']/*[local-name() = 'VisibilityPeriod']/*[local-name() = 'VisibilityPeriodEnd']"/>
        <xsl:variable name="day" select="substring-after(substring-after(substring-before($fullTime,'T'),'-'),'-')"/>
        <xsl:variable name="month" select="substring-before(substring-after(substring-before($fullTime,'T'),'-'),'-')"/>
        <xsl:variable name="year" select="substring-before($fullTime,'-')"/>
        <xsl:variable name="time" select="substring-before(substring-after($fullTime,'T'),'Z')"/>
        <xsl:variable name="with_day_slash" select="concat($day,'/')"/>
        <xsl:variable name="with_month_slash" select="concat($month,'/')"/>
        <xsl:variable name="day_month" select="concat($with_day_slash,$with_month_slash)"/>
        <xsl:variable name="day_month_year" select="concat($day_month,$year)"/>
        <xsl:variable name="day_month_year_s" select="concat($day_month_year,' ')"/>
        <xsl:value-of select="concat($day_month_year_s,$time)"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>1/1/2099 00:00:00</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="SetStartViewDate">
    <xsl:if test="./*[local-name() = 'PresentationMetaGroup']/*[local-name() = 'VisibilityPeriod']/*[local-name() = 'VisibilityPeriodStart']">
      <xsl:variable name="fullTime" select="./*[local-name() = 'PresentationMetaGroup']/*[local-name() = 'VisibilityPeriod']/*[local-name() = 'VisibilityPeriodStart']"/>
      <xsl:variable name="day" select="substring-after(substring-after(substring-before($fullTime,'T'),'-'),'-')"/>
      <xsl:variable name="month" select="substring-before(substring-after(substring-before($fullTime,'T'),'-'),'-')"/>
      <xsl:variable name="year" select="substring-before($fullTime,'-')"/>
      <xsl:variable name="time" select="substring-before(substring-after($fullTime,'T'),'Z')"/>
      <xsl:variable name="with_day_slash" select="concat($day,'/')"/>
      <xsl:variable name="with_month_slash" select="concat($month,'/')"/>
      <xsl:variable name="day_month" select="concat($with_day_slash,$with_month_slash)"/>
      <xsl:variable name="day_month_year" select="concat($day_month,$year)"/>
      <xsl:variable name="day_month_year_s" select="concat($day_month_year,' ')"/>
      <xsl:value-of select="concat($day_month_year_s,$time)"/>
    </xsl:if>
  </xsl:template>

  <xsl:template name="SetFStartViewDate">
    <xsl:choose>
      <xsl:when test="./*[local-name() = 'LicenseList']/License[1]/*[local-name() = 'LicensePeriodGroup']/*[local-name() = 'LicensePurchasePeriodStart']">
      <xsl:variable name="fullTime" select="./*[local-name() = 'LicenseList']/License[1]/*[local-name() = 'LicensePeriodGroup']/*[local-name() = 'LicensePurchasePeriodStart']"/>
        <xsl:variable name="day" select="substring-after(substring-after(substring-before($fullTime,'T'),'-'),'-')"/>
        <xsl:variable name="month" select="substring-before(substring-after(substring-before($fullTime,'T'),'-'),'-')"/>
        <xsl:variable name="year" select="substring-before($fullTime,'-')"/>
        <xsl:variable name="time" select="substring-before(substring-after($fullTime,'T'),'Z')"/>
        <xsl:variable name="with_day_slash" select="concat($day,'/')"/>
        <xsl:variable name="with_month_slash" select="concat($month,'/')"/>
        <xsl:variable name="day_month" select="concat($with_day_slash,$with_month_slash)"/>
        <xsl:variable name="day_month_year" select="concat($day_month,$year)"/>
        <xsl:variable name="day_month_year_s" select="concat($day_month_year,' ')"/>
        <xsl:value-of select="concat($day_month_year_s,$time)"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:call-template name="SetStartViewDate"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="SetFEndViewDate">
    <xsl:choose>
      <xsl:when test="./*[local-name() = 'LicenseList']/License[1]/*[local-name() = 'LicensePeriodGroup']/*[local-name() = 'LicenseUsagePeriodEnd']">
        <xsl:variable name="fullTime" select="./*[local-name() = 'LicenseList']/License[1]/*[local-name() = 'LicensePeriodGroup']/*[local-name() = 'LicenseUsagePeriodEnd']"/>
        <xsl:variable name="day" select="substring-after(substring-after(substring-before($fullTime,'T'),'-'),'-')"/>
        <xsl:variable name="month" select="substring-before(substring-after(substring-before($fullTime,'T'),'-'),'-')"/>
        <xsl:variable name="year" select="substring-before($fullTime,'-')"/>
        <xsl:variable name="time" select="substring-before(substring-after($fullTime,'T'),'Z')"/>
        <xsl:variable name="with_day_slash" select="concat($day,'/')"/>
        <xsl:variable name="with_month_slash" select="concat($month,'/')"/>
        <xsl:variable name="day_month" select="concat($with_day_slash,$with_month_slash)"/>
        <xsl:variable name="day_month_year" select="concat($day_month,$year)"/>
        <xsl:variable name="day_month_year_s" select="concat($day_month_year,' ')"/>
        <xsl:value-of select="concat($day_month_year_s,$time)"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:call-template name="SetEndViewDate"/>
      </xsl:otherwise>
      </xsl:choose>
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
      <xsl:element name="metas">
        <xsl:call-template name="build_tags_data"/>
      </xsl:element>
    </xsl:element>
  </xsl:template>

  <xsl:template name="build_strings_data">
    <!--<xsl:element name="meta">
      <xsl:attribute name="name">Summary short</xsl:attribute>
      <xsl:attribute name="ml_handling">unique</xsl:attribute>
        <xsl:element name="value">
          <xsl:attribute name="lang">
            <xsl:call-template name="Translate_Langueage"/>
          </xsl:attribute>
          <xsl:value-of select="./*[local-name() = 'PresentationMetaGroup']/*[local-name() = 'Title']/*[local-name() = 'SynopsisList']/*[local-name() = 'Synopsis']"/>
        </xsl:element>
    </xsl:element>-->

   <xsl:element name="meta">
      <xsl:attribute name="name">Short title</xsl:attribute>
      <xsl:attribute name="ml_handling">unique</xsl:attribute>
        <xsl:element name="value">
          <xsl:attribute name="lang">
            <xsl:call-template name="Translate_Langueage"/>
          </xsl:attribute>
          <xsl:value-of select="./*[local-name() = 'PresentationMetaGroup']/*[local-name() = 'Title']/*[local-name() = 'TitleNameList']/*[local-name() = 'TitleName']"/>
        </xsl:element>
    </xsl:element>
    
  </xsl:template>

  <xsl:template name="build_doubles_data">
    <xsl:element name="meta">
      <xsl:attribute name="name">Release year</xsl:attribute>
      <xsl:value-of select="./*[local-name() = 'PresentationMetaGroup']/*[local-name() = 'Title']/*[local-name() = 'Production']/*[local-name() = 'ReleaseYear']"/>
    </xsl:element>
  </xsl:template>

  <xsl:template name="build_booleans_data">
    <xsl:element name="meta">
      <xsl:attribute name="name"></xsl:attribute>
    </xsl:element>
  </xsl:template>

  <xsl:template name="build_tags_data">
    <xsl:element name="meta">
      <xsl:attribute name="name">Main cast</xsl:attribute>
      <xsl:attribute name="ml_handling">unique</xsl:attribute>
      <xsl:for-each select="//*[local-name() = 'PresentationMetaGroup']/*[local-name() = 'Title']/*[local-name() = 'ActorNameList']/*[local-name() = 'ActorName']">
        <xsl:element name="container">
          <xsl:element name="value">
            <xsl:attribute name="lang">de</xsl:attribute>
            <xsl:value-of select="."/>
          </xsl:element>
        </xsl:element>
      </xsl:for-each>
    </xsl:element>


    <xsl:element name="meta">
      <xsl:attribute name="name">Series name</xsl:attribute>
      <xsl:attribute name="ml_handling">unique</xsl:attribute>      
      <xsl:for-each select="./*[local-name() = 'PresentationMetaGroup']/*[local-name() = 'Title']/*[local-name() = 'TitleNameList']/*[local-name() = 'TitleName']">
      <xsl:element name="container">
        <xsl:element name="value">
          <xsl:attribute name="lang">de</xsl:attribute>
          <xsl:value-of select="."/>
        </xsl:element>
      </xsl:element>
      </xsl:for-each>     
    </xsl:element>      
    
    <xsl:element name="meta">
      <xsl:attribute name="name">Director</xsl:attribute>
      <xsl:attribute name="ml_handling">unique</xsl:attribute>
      <xsl:for-each select="//*[local-name() = 'PresentationMetaGroup']/*[local-name() = 'Title']/*[local-name() = 'DirectorNameList']/*[local-name() = 'DirectorName']">
        <xsl:element name="container">
          <xsl:element name="value">
            <xsl:attribute name="lang">de</xsl:attribute>
            <xsl:value-of select="."/>
          </xsl:element>
        </xsl:element>
      </xsl:for-each>
    </xsl:element>

    <xsl:element name="meta">
      <xsl:attribute name="name">Genre</xsl:attribute>
      <xsl:attribute name="ml_handling">unique</xsl:attribute>
      <xsl:for-each select="./*[local-name() = 'PresentationMetaGroup']/*[local-name() = 'Title']/*[local-name() = 'GenreNameList']/*[local-name() = 'GenreName']">
        <xsl:element name="container">
          <xsl:element name="value">
            <xsl:attribute name="lang">de</xsl:attribute>
            <xsl:value-of select="."/>
          </xsl:element>
        </xsl:element>
      </xsl:for-each>
    </xsl:element>

    <xsl:element name="meta">
      <xsl:attribute name="name">Studio</xsl:attribute>
      <xsl:attribute name="ml_handling">unique</xsl:attribute>
      <xsl:element name="container">
        <xsl:element name="value">
          <xsl:attribute name="lang">de</xsl:attribute>
          <xsl:value-of select="//*[local-name() = 'PresentationMetaGroup']/*[local-name() = 'Title']/*[local-name() = 'CopyrightDisplay']"/>
        </xsl:element>
      </xsl:element>
    </xsl:element>

    <xsl:element name="meta">
      <xsl:attribute name="name">Dimension</xsl:attribute>
      <xsl:attribute name="ml_handling">unique</xsl:attribute>
      <xsl:for-each select="//*[local-name() = 'PresentationMetaGroup']/*[local-name() = 'Title']/*[local-name() = 'DimensionList']/*[local-name() = 'Dimension']">
        <xsl:element name="container">
          <xsl:element name="value">
            <xsl:attribute name="lang">de</xsl:attribute>
            <xsl:value-of select="."/>
          </xsl:element>
        </xsl:element>
      </xsl:for-each>
    </xsl:element>

    <xsl:element name="meta">
      <xsl:attribute name="name">Provider</xsl:attribute>
      <xsl:attribute name="ml_handling">unique</xsl:attribute>
        <xsl:element name="container">
          <xsl:element name="value">
            <xsl:attribute name="lang">de</xsl:attribute>
            <xsl:value-of select="./*[local-name() = 'PresentationMetaGroup']/*[local-name() = 'ProviderList']/*[local-name() = 'Provider']/*[local-name() = 'ProviderId']"/>
          </xsl:element>
        </xsl:element>
    </xsl:element>

    <xsl:element name="meta">
      <xsl:attribute name="name">Country</xsl:attribute>
      <xsl:attribute name="ml_handling">unique</xsl:attribute>
      <xsl:for-each select="./*[local-name() = 'PresentationMetaGroup']/*[local-name() = 'Title']/*[local-name() = 'Production']/*[local-name() = 'ProductionCountryList']/*[local-name() = 'ProductionCountry']">
        <xsl:element name="container">
          <xsl:element name="value">
            <xsl:attribute name="lang">de</xsl:attribute>
            <xsl:value-of select="."/>
          </xsl:element>
        </xsl:element>
      </xsl:for-each>
    </xsl:element>
  </xsl:template>

  <xsl:template name="Translate_Langueage">
    <xsl:variable name="languageCode" select="./*[local-name() = 'PresentationMetaGroup']/*[local-name() = 'Title']/*[local-name() = 'TitleNameList']/*[local-name() = 'TitleName']/@lang"/>
    <xsl:choose>
      <xsl:when test="$languageCode = 'de'"> 
        <xsl:text>de</xsl:text>
      </xsl:when>
    </xsl:choose>
  </xsl:template>
</xsl:stylesheet>