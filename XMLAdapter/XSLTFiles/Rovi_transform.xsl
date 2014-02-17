<?xml version="1.0" ?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl" xmlns:ADIUtils="pda:ADIUtils" xmlns:offer="http://www.cablelabs.com/namespaces/metadata/xsd/offer/1" >
  <xsl:output method="xml" omit-xml-declaration="yes" indent="yes"/>

  <xsl:variable name="prefixURLPhysicalFile">
    <xsl:text></xsl:text>
  </xsl:variable>
  <xsl:variable name="ImagePathConstVar">
    <xsl:text></xsl:text>
  </xsl:variable>

  <xsl:template match="/">
    <feed broadcasterName="whitelabel">
      <export>
        <xsl:apply-templates select="//*[local-name() = 'Title']"/>
      </export>
    </feed>
  </xsl:template>

  <xsl:template match="//*[local-name() = 'Title']">
    <xsl:call-template name="handle_media"/>
  </xsl:template>

  <xsl:template name="handle_media">
    <xsl:element name="media">
      <xsl:attribute name="is_active">true</xsl:attribute>
      <xsl:attribute name="co_guid">
        <xsl:value-of select="*[local-name() = 'TitleId']"/>
      </xsl:attribute>
      <xsl:attribute name="action">insert</xsl:attribute>
      <xsl:call-template name="build_basic_data"/>
      <xsl:call-template name="build_structure_data"/>
      <xsl:call-template name="build_files_data"/>
    </xsl:element>
  </xsl:template>

  <xsl:template name="Set_File_Type">
    <xsl:choose>
      <xsl:when test="./*[local-name() = 'MediaType']/*[local-name() = 'ShortName'] = 'M3U8'">
        <xsl:choose>
          <xsl:when test="./*[local-name() = 'DimensionList']/*[local-name() = 'Dimension'] = 'SD'">
            <xsl:choose>
              <xsl:when test="./*[local-name() = 'MediaType']/*[local-name() = 'EncryptionType'] = 'Marlin'">
                <!--M3U8/SD/Marlin-->
                <xsl:attribute name="type">STB Main SD</xsl:attribute>
              </xsl:when>
              <xsl:when test="./*[local-name() = 'MediaType']/*[local-name() = 'EncryptionType'] = 'PLAYREADY'">
                <!--M3U8/SD/PLAYREADY-->
                <xsl:attribute name="type">Mobile Devices Main SD</xsl:attribute>
              </xsl:when>
            </xsl:choose>
          </xsl:when>
          <xsl:when test="./*[local-name() = 'DimensionList']/*[local-name() = 'Dimension'] = 'HD'">
            <xsl:choose>
              <xsl:when test="./*[local-name() = 'MediaType']/*[local-name() = 'EncryptionType'] = 'Marlin'">
                <!--M3U8/HD/Marlin-->
                <xsl:attribute name="type">STB Main HD</xsl:attribute>
              </xsl:when>
              <xsl:when test="./*[local-name() = 'MediaType']/*[local-name() = 'EncryptionType'] = 'PLAYREADY'">
                <!--M3U8/HD/PLAYREADY-->
                <xsl:attribute name="type">Mobile Devices Main HD</xsl:attribute>
              </xsl:when>
            </xsl:choose>
          </xsl:when>
        </xsl:choose>
      </xsl:when>
      <xsl:when test="./*[local-name() = 'MediaType']/*[local-name() = 'ShortName'] = 'ISM'">
        <xsl:choose>
          <xsl:when test="./*[local-name() = 'DimensionList']/*[local-name() = 'Dimension'] = 'SD'">
            <xsl:choose>
              <xsl:when test="./*[local-name() = 'MediaType']/*[local-name() = 'EncryptionType'] = 'Marlin'">
                <!--ISM/SD/Marlin-->
              </xsl:when>
              <xsl:when test="./*[local-name() = 'MediaType']/*[local-name() = 'EncryptionType'] = 'PLAYREADY'">
                <!--ISM/SD/PLAYREADY-->
                <xsl:attribute name="type">PC Main SD</xsl:attribute>
              </xsl:when>
            </xsl:choose>
          </xsl:when>
          <xsl:when test="./*[local-name() = 'DimensionList']/*[local-name() = 'Dimension'] = 'HD'">
            <xsl:choose>
              <xsl:when test="./*[local-name() = 'MediaType']/*[local-name() = 'EncryptionType'] = 'Marlin'">
                <!--ISM/HD/Marlin-->
              </xsl:when>
              <xsl:when test="./*[local-name() = 'MediaType']/*[local-name() = 'EncryptionType'] = 'PLAYREADY'">
                <!--ISM/HD/PLAYREADY-->
                <xsl:attribute name="type">PC Main HD</xsl:attribute>
              </xsl:when>
            </xsl:choose>
          </xsl:when>
        </xsl:choose>
      </xsl:when>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="build_files_data">
    <xsl:element name="files">
      <xsl:for-each select="../../*[local-name() = 'ContentList']/*[local-name() = 'Content']/*[local-name() = 'FormatList']/*[local-name() = 'Format']" >
        <xsl:if test="./*[local-name() = 'MediaType']/*[local-name() = 'ShortName'] = 'M3U8' and ./*[local-name() = 'MediaType']/*[local-name() = 'EncryptionType'] = 'PLAYREADY'">
          <xsl:element name="file">
            <xsl:call-template name="Set_File_Type"/>
            <xsl:attribute name="quality">HIGH</xsl:attribute>
            <xsl:attribute name="pre_rule">null</xsl:attribute>
            <xsl:attribute name="post_rule">null</xsl:attribute>
            <xsl:attribute name="handling_type">CLIP</xsl:attribute>
            <xsl:attribute name="duration">0</xsl:attribute>
            <xsl:attribute name="cdn_name">Default CDN</xsl:attribute>
            <xsl:attribute name="cdn_code"><xsl:value-of select="./*[local-name() = 'StartUrl']"/></xsl:attribute>
            <xsl:attribute name="break_rule"></xsl:attribute>
            <xsl:attribute name="break_points"></xsl:attribute>
            <xsl:attribute name="billing_type"></xsl:attribute>
            <xsl:attribute name="assetwidth"></xsl:attribute>
            <xsl:attribute name="assetheight"></xsl:attribute>
            <xsl:attribute name="assetduration">0</xsl:attribute>
            <xsl:attribute name="ppv_module"></xsl:attribute>
            <xsl:variable name="formatID" select="./*[local-name() = 'FormatId']"/>
            <xsl:variable name="formatIDU" select="concat($formatID,'_')"/>
            <xsl:variable name="contentID" select="../../*[local-name() = 'ContentId']"/>
            <xsl:attribute name="co_guid"><xsl:value-of select="concat($formatIDU,$contentID)"/></xsl:attribute>
            <xsl:attribute name="output_protection_level"><xsl:value-of select="./*[local-name() = 'MediaType']/*[local-name() = 'MinimumMediaOutputProtectionProfile']"/></xsl:attribute>
          </xsl:element>
        </xsl:if>
      </xsl:for-each>
      <xsl:element name="file">
        <xsl:attribute name="type">Trailer</xsl:attribute>
        <xsl:attribute name="quality">HIGH</xsl:attribute>
        <xsl:attribute name="pre_rule">null</xsl:attribute>
        <xsl:attribute name="post_rule">null</xsl:attribute>
        <xsl:attribute name="handling_type">CLIP</xsl:attribute>
        <xsl:attribute name="duration">0</xsl:attribute>
        <xsl:attribute name="cdn_name">Default CDN</xsl:attribute>
        <xsl:attribute name="cdn_code"><xsl:value-of select="../../*[local-name() = 'PromotionContentList']/*[local-name() = 'PromotionContent'][@comment = 'Trailer']/*[local-name() = 'FormatList']/*[local-name() = 'Format']/*[local-name() = 'StartUrl']"/></xsl:attribute>
        <xsl:attribute name="break_rule"></xsl:attribute>
        <xsl:attribute name="break_points"></xsl:attribute>
        <xsl:attribute name="billing_type"></xsl:attribute>
        <xsl:attribute name="assetwidth"></xsl:attribute>
        <xsl:attribute name="assetheight"></xsl:attribute>
        <xsl:attribute name="assetduration">0</xsl:attribute>
        <xsl:attribute name="ppv_module"></xsl:attribute>
        <xsl:variable name="formatID" select="./*[local-name() = 'FormatId']"/>
        <xsl:variable name="formatIDU" select="concat($formatID,'_')"/>
        <xsl:variable name="contentID" select="../../*[local-name() = 'ContentId']"/>
        <xsl:attribute name="co_guid"><xsl:value-of select="concat($formatIDU,$contentID)"/></xsl:attribute>
      </xsl:element>
    </xsl:element>
  </xsl:template>

  <xsl:template name="build_basic_data">
    <xsl:element name="basic">
      <xsl:element name="name">
        <xsl:for-each select="*[local-name() = 'TitleNameList']/*[local-name() = 'TitleName']">
          <xsl:element name="value"><xsl:attribute name="lang"><xsl:call-template name="Translate_Langueage"/></xsl:attribute><xsl:value-of select="."/></xsl:element>
        </xsl:for-each>
      </xsl:element>
      <xsl:element name="description">
        <xsl:for-each select="../../*[local-name() = 'ContentList']/*[local-name() = 'Content']/*[local-name() = 'SynopsisList']/*[local-name() = 'Synopsis']">
          <xsl:element name="value"><xsl:attribute name="lang"><xsl:call-template name="Translate_Langueage"/></xsl:attribute><xsl:value-of select="."/></xsl:element>
        </xsl:for-each>
      </xsl:element>
      <xsl:element name="media_type"><xsl:value-of select="*[local-name() = 'TitleType']"/></xsl:element>
      <xsl:element name="rules">
        <xsl:element name="geo_block_rule">
          <xsl:variable name="Number_Of_Countries" select="count(../../*[local-name() = 'LicenseList']/License[1]/*[local-name() = 'LicenseGrantsList']/*[local-name() = 'TerritoryWhitelist']/*[local-name() = 'Territory'])"/>
          <xsl:choose>
            <xsl:when test="$Number_Of_Countries = 2">
              <xsl:text>Germany and Austria only</xsl:text>  
            </xsl:when>
            <xsl:otherwise>
              <xsl:choose>
                <xsl:when test="../../*[local-name() = 'LicenseList']/License[1]/*[local-name() = 'LicenseGrantsList']/*[local-name() = 'TerritoryWhitelist']/Territory[1] = 'DE'">
                  <xsl:text>Germany and Austria only</xsl:text>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:text>Austria Only</xsl:text>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:element>
        <xsl:element name="watch_per_rule">
          <xsl:text>Parent account allowed</xsl:text>
        </xsl:element>
      </xsl:element>
      <xsl:element name="dates">
        <xsl:element name="visibility_period_start">
          <xsl:call-template name="SetStartViewDate"/>
        </xsl:element>
        <xsl:element name="start">
          <xsl:call-template name="SetStartDate"/>
        </xsl:element>
        <xsl:element name="catalog_end">
          <xsl:call-template name="SetEndDate"/>
        </xsl:element>
        <xsl:element name="final_end">
          <xsl:call-template name="SetEndViewDate"/>
        </xsl:element>
      </xsl:element>
      <xsl:if test="../../*[local-name() = 'PromotionContentList']/*[local-name() = 'PromotionContent'][@comment = 'Cover']/*[local-name() = 'FormatList']/*[local-name() = 'Format']">
        <xsl:variable name="urlPostfix" select="../../*[local-name() = 'PromotionContentList']/*[local-name() = 'PromotionContent'][@comment = 'Cover']/*[local-name() = 'FormatList']/*[local-name() = 'Format']/*[local-name() = 'StartUrl']"/>
        <xsl:element name="thumb">
          <xsl:attribute name="url">
            <xsl:value-of select="concat($ImagePathConstVar,$urlPostfix)"/>
          </xsl:attribute>
        </xsl:element>
        <xsl:element name="pic_ratios">
          <xsl:for-each select="../../*[local-name() = 'PromotionContentList']/*[local-name() = 'PromotionContent'][@comment = 'Cover']/*[local-name() = 'FormatList']/*[local-name() = 'Format']">
            <xsl:variable name="imageRatio" select="ADIUtils:GetRatio(./*[local-name() = 'Width'],./*[local-name() = 'Height'])" />
            <xsl:element name="ratio">
              <xsl:attribute name="thumb">
                <xsl:value-of select="concat($ImagePathConstVar,./*[local-name() = 'StartUrl'])"/>
              </xsl:attribute>
              <xsl:attribute name="ratio">
                <xsl:value-of select="./*[local-name() = 'AspectRatio']"/>
              </xsl:attribute>
            </xsl:element>
          </xsl:for-each>
        </xsl:element>
      </xsl:if>
    </xsl:element>
  </xsl:template>

  <xsl:template name="SetEndViewDate">
    <xsl:choose>
      <xsl:when test="not(../*[local-name() = 'VisibilityPeriod']/*[local-name() = 'VisibilityPeriodEnd'] = '')">
        <xsl:variable name="fullTime" select="../*[local-name() = 'VisibilityPeriod']/*[local-name() = 'VisibilityPeriodEnd']"/>
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
    <xsl:if test="../*[local-name() = 'VisibilityPeriod']/*[local-name() = 'VisibilityPeriodStart']">
      <xsl:variable name="fullTime" select="../*[local-name() = 'VisibilityPeriod']/*[local-name() = 'VisibilityPeriodStart']"/>
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

  <xsl:template name="SetStartDate">
    <xsl:param name="index" select="1" />
    <xsl:param name="MostSooner" select="1" />
    <xsl:param name="total" select="count(../../*[local-name() = 'LicenseList']/*[local-name() = 'License'])" />

    <xsl:if test="not($index = $total)">
      <xsl:choose>
        <xsl:when test="../../*[local-name() = 'LicenseList']/License[$index]/*[local-name() = 'LicensePeriodGroup']/*[local-name() = 'LicensePurchasePeriodStartUnix'] &lt; ../../*[local-name() = 'LicenseList']/License[$MostSooner]/*[local-name() = 'LicensePeriodGroup']/*[local-name() = 'LicensePurchasePeriodStartUnix']">
            <xsl:call-template name="SetStartDate">
              <xsl:with-param name="index" select="$index + 1" />
              <xsl:with-param name="MostLatest" select="$index" />
            </xsl:call-template>
        </xsl:when>
        <xsl:otherwise>
          <xsl:call-template name="SetStartDate">
            <xsl:with-param name="index" select="$index + 1" />
            <xsl:with-param name="MostLatest" select="$MostSooner" />
          </xsl:call-template>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:if>

    <xsl:if test="$index = $total">
      <xsl:variable name="fullTime" select="../../*[local-name() = 'LicenseList']/License[$MostSooner]/*[local-name() = 'LicensePeriodGroup']/*[local-name() = 'LicensePurchasePeriodStart']"/>
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

  <xsl:template name="SetEndDate">
    <xsl:param name="index" select="1" />
    <xsl:param name="MostLatest" select="1" />
    <xsl:param name="total" select="count(../../*[local-name() = 'LicenseList']/*[local-name() = 'License'])" />

    <xsl:if test="not($index = $total)">
      <xsl:choose>
        <xsl:when test="../../*[local-name() = 'LicenseList']/License[$index]/*[local-name() = 'LicensePeriodGroup']/*[local-name() = 'LicensePurchasePeriodEndUnix'] &gt; ../../*[local-name() = 'LicenseList']/License[$MostLatest]/*[local-name() = 'LicensePeriodGroup']/*[local-name() = 'LicensePurchasePeriodEndUnix']">
          <xsl:call-template name="SetEndDate">
            <xsl:with-param name="index" select="$index + 1" />
            <xsl:with-param name="MostLatest" select="$index" />
          </xsl:call-template>
        </xsl:when>
        <xsl:otherwise>
          <xsl:call-template name="SetEndDate">
            <xsl:with-param name="index" select="$index + 1" />
            <xsl:with-param name="MostLatest" select="$MostLatest" />
          </xsl:call-template>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:if>

    <xsl:if test="$index = $total">
      <xsl:variable name="fullTime" select="../../*[local-name() = 'LicenseList']/License[$MostLatest]/*[local-name() = 'LicensePeriodGroup']/*[local-name() = 'LicensePurchasePeriodEnd']"/>
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
    <xsl:element name="meta">
      <xsl:attribute name="name">Short summary</xsl:attribute>
      <xsl:attribute name="ml_handling">unique</xsl:attribute>
      <xsl:for-each select="*[local-name() = 'SynopsisList']/*[local-name() = 'SynopsisShort']">
        <xsl:element name="value">
          <xsl:attribute name="lang"><xsl:call-template name="Translate_Langueage"/><xsl:value-of select="." /></xsl:attribute>
        </xsl:element>
      </xsl:for-each>
    </xsl:element>
    <xsl:element name="meta">
      <xsl:attribute name="name">Short title</xsl:attribute>
      <xsl:attribute name="ml_handling">unique</xsl:attribute>
      <xsl:for-each select="*[local-name() = 'SecondTitleNameList']/*[local-name() = 'SecondTitleName']">
        <xsl:element name="value">
          <xsl:attribute name="lang"><xsl:call-template name="Translate_Langueage"/><xsl:value-of select="." /></xsl:attribute>
        </xsl:element>
      </xsl:for-each>
    </xsl:element>
  </xsl:template>

  <xsl:template name="build_doubles_data">
    <xsl:element name="meta">
      <xsl:attribute name="name">Release year</xsl:attribute>
      <xsl:value-of select="*[local-name() = 'Production']/*[local-name() = 'ReleaseYear']" />
    </xsl:element>
    <xsl:element name="meta">
      <xsl:attribute name="name">Run time (minutes)</xsl:attribute>
      <xsl:value-of select="../../*[local-name() = 'ContentList']/*[local-name() = 'Content']/*[local-name() = 'RunTimeMinutes']" />
    </xsl:element>
  </xsl:template>

  <xsl:template name="build_booleans_data">
    <xsl:element name="meta">
      <xsl:attribute name="name"></xsl:attribute>
      <xsl:choose>
        <xsl:when test="../*[local-name() = 'LocalizedInfo']/*[local-name() = 'DisplayIndicators'] = 'CC'">true</xsl:when>
        <xsl:otherwise>false</xsl:otherwise>
      </xsl:choose>
    </xsl:element>
  </xsl:template>

  <xsl:template name="build_tags_data">
    <xsl:element name="meta">
      <xsl:attribute name="name">Main cast</xsl:attribute>
      <xsl:attribute name="ml_handling">unique</xsl:attribute>
      <xsl:for-each select="*[local-name() = 'ActorNameList']/*[local-name() = 'ActorName']">
        <xsl:element name="container">
          <xsl:element name="value">
            <xsl:attribute name="lang">grm</xsl:attribute>
            <xsl:value-of select="."/>
          </xsl:element>
        </xsl:element>
      </xsl:for-each>
    </xsl:element>

    <xsl:element name="meta">
      <xsl:attribute name="name">Director</xsl:attribute>
      <xsl:attribute name="ml_handling">unique</xsl:attribute>
      <xsl:for-each select="*[local-name() = 'DirectorNameList']/*[local-name() = 'DirectorName']">
        <xsl:element name="container">
          <xsl:element name="value">
            <xsl:attribute name="lang">grm</xsl:attribute>
            <xsl:value-of select="."/>
          </xsl:element>
        </xsl:element>
      </xsl:for-each>
    </xsl:element>

    <xsl:element name="meta">
      <xsl:attribute name="name">Genre</xsl:attribute>
      <xsl:attribute name="ml_handling">unique</xsl:attribute>
      <xsl:for-each select="*[local-name() = 'GenreNameList']/*[local-name() = 'GenreName']">
        <xsl:element name="container">
          <xsl:element name="value">
            <xsl:attribute name="lang">grm</xsl:attribute>
            <xsl:value-of select="."/>
          </xsl:element>
        </xsl:element>
      </xsl:for-each>
    </xsl:element>

    <xsl:element name="meta">
      <xsl:attribute name="name">Category</xsl:attribute>
      <xsl:attribute name="ml_handling">unique</xsl:attribute>
      <xsl:for-each select="*[local-name() = 'CategoryList']/*[local-name() = 'Category']">
        <xsl:element name="container">
          <xsl:element name="value">
            <xsl:attribute name="lang">grm</xsl:attribute>
            <xsl:value-of select="."/>
          </xsl:element>
        </xsl:element>
      </xsl:for-each>
    </xsl:element>

    <xsl:element name="meta">
      <xsl:attribute name="name">Rating</xsl:attribute>
      <xsl:attribute name="ml_handling">unique</xsl:attribute>
        <xsl:element name="container">
          <xsl:element name="value">
            <xsl:attribute name="lang">grm</xsl:attribute>
            <xsl:value-of select="../../*[local-name() = 'ContentList']/*[local-name() = 'Content']/*[local-name() = 'ParentalControlList']/*[local-name() = 'ParentalControl']/*[local-name() = 'ParentalControlId']"/>
          </xsl:element>
        </xsl:element>
    </xsl:element>

    <xsl:element name="meta">
      <xsl:attribute name="name">Related content</xsl:attribute>
      <xsl:attribute name="ml_handling">unique</xsl:attribute>
      <xsl:for-each select="*[local-name() = 'RelatedTitleIdList']/*[local-name() = 'RelatedTitleId']">
        <xsl:element name="container">
          <xsl:element name="value">
            <xsl:attribute name="lang">grm</xsl:attribute>
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
            <xsl:attribute name="lang">grm</xsl:attribute>
            <xsl:value-of select="../*[local-name() = 'ProviderList']/*[local-name() = 'Provider']/*[local-name() = 'ProviderId']"/>
          </xsl:element>
        </xsl:element>
    </xsl:element>

    <xsl:element name="meta">
      <xsl:attribute name="name">Studio</xsl:attribute>
      <xsl:attribute name="ml_handling">unique</xsl:attribute>
        <xsl:element name="container">
          <xsl:element name="value">
            <xsl:attribute name="lang">grm</xsl:attribute>
            <xsl:value-of select="*[local-name() = 'CopyrightDisplay']"/>
          </xsl:element>
        </xsl:element>
    </xsl:element>

    <xsl:element name="meta">
      <xsl:attribute name="name">Country</xsl:attribute>
      <xsl:attribute name="ml_handling">unique</xsl:attribute>
      <xsl:for-each select="*[local-name() = 'Production']/*[local-name() = 'ProductionCountryDisplay']">
        <xsl:element name="container">
          <xsl:element name="value">
            <xsl:attribute name="lang">grm</xsl:attribute>
            <xsl:value-of select="."/>
          </xsl:element>
        </xsl:element>
      </xsl:for-each>
    </xsl:element>

    <xsl:element name="meta">
      <xsl:attribute name="name">Audio language</xsl:attribute>
      <xsl:attribute name="ml_handling">unique</xsl:attribute>
      <xsl:element name="container">
        <xsl:element name="value">
          <xsl:attribute name="lang">grm</xsl:attribute>
          <xsl:value-of select="../../*[local-name() = 'LicenseList']/*[local-name() = 'License']/*[local-name() = 'LicenseGrantsList']/*[local-name() = 'AudioTrackList']/*[local-name() = 'AudioTrack']"/>
        </xsl:element>
      </xsl:element>
    </xsl:element>

    <xsl:element name="meta">
      <xsl:attribute name="name">Subtitle language</xsl:attribute>
      <xsl:attribute name="ml_handling">unique</xsl:attribute>
      <xsl:element name="container">
        <xsl:element name="value">
          <xsl:attribute name="lang">grm</xsl:attribute>
          <xsl:value-of select="../../*[local-name() = 'LicenseList']/*[local-name() = 'License']/*[local-name() = 'LicenseGrantsList']/*[local-name() = 'SubtitleTrackList']/*[local-name() = 'SubtitleTrack']"/>
        </xsl:element>
      </xsl:element>
    </xsl:element>
  </xsl:template>

  <xsl:template name="Translate_Langueage">
    <xsl:variable name="languageCode" select="@lang"/>
    <xsl:choose>
      <xsl:when test="$languageCode = 'de'">
        <xsl:text>grm</xsl:text>
      </xsl:when>
    </xsl:choose>
  </xsl:template>

</xsl:stylesheet>