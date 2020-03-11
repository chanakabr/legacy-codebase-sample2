<?xml version="1.0" ?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl" xmlns:ADIUtils="pda:ADIUtils" xmlns:offer="http://www.cablelabs.com/namespaces/metadata/xsd/offer/1" >
  <xsl:output method="xml" omit-xml-declaration="yes" indent="yes"/>

  <!--<xsl:variable name="prefixURLPhysicalFile"><xsl:text>http://blink.cdn3.dmlib.com/</xsl:text></xsl:variable>-->
  <xsl:variable name="prefixURLPhysicalFile"></xsl:variable>
  <xsl:variable name="ImagePathConstVar"><xsl:text>http://cdn2.d3nw.com/</xsl:text></xsl:variable>

  <xsl:variable name="smallcase" select="'abcdefghijklmnopqrstuvwxyz'" />
  <xsl:variable name="uppercase" select="'ABCDEFGHIJKLMNOPQRSTUVWXYZ'" />

  <xsl:template match="/">
    <feed broadcasterName="whitelabel">
      <export>
        <xsl:apply-templates select="//*[local-name()='ADI']"/>
      </export>
    </feed>
  </xsl:template>

  <xsl:template match="//*[local-name()='ADI']">
      <xsl:element name="media">
        <xsl:attribute name="is_active">
          <xsl:for-each select="//*[local-name() = 'Asset']/*[local-name() = 'Metadata']/*[local-name() = 'AMS'][@Asset_Class = 'title']">
            <xsl:choose>
              <xsl:when test="translate(../*[local-name() = 'App_Data'][@Name = 'Asset_Is_Active']/@Value, $uppercase, $smallcase) = 'true'">
                <xsl:text>true</xsl:text>
              </xsl:when>
              <xsl:otherwise>
                <xsl:text>false</xsl:text>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:for-each>
        </xsl:attribute>
        <xsl:attribute name="co_guid">
          <xsl:value-of select="./*[local-name() = 'Metadata']/*[local-name() = 'AMS']/@Asset_ID"/>
        </xsl:attribute>
        <xsl:attribute name="action">insert</xsl:attribute>
        <xsl:call-template name="build_basic_data"/>
        <xsl:call-template name="build_structure_data"/>
        <xsl:call-template name="build_files_data"/>
      </xsl:element>
  </xsl:template>

  <xsl:template name="build_files_data">
    <xsl:element name="files">
      <xsl:for-each select="//*[local-name() = 'Asset']/*[local-name() = 'Asset']/*[local-name() = 'Metadata']/*[local-name() = 'AMS']">
        <xsl:variable name="FType" select="@Asset_ID"/>
        <xsl:if test="not(../../preceding::*/AMS[@Asset_ID = $FType]/@Asset_Class)">
          <xsl:choose>
            <xsl:when test="translate(@Asset_Class, $uppercase, $smallcase) = 'movie'">
              <xsl:element name="file">
                <xsl:attribute name="type"><xsl:text>iOS Clear</xsl:text></xsl:attribute>
                <xsl:attribute name="quality"><xsl:text>HIGH</xsl:text></xsl:attribute>
                <xsl:attribute name="pre_rule"><xsl:value-of select="ADIUtils:GetAdProvider()"/></xsl:attribute>
                  <xsl:attribute name="post_rule"><xsl:value-of select="ADIUtils:GetAdProvider()"/></xsl:attribute>
                  <xsl:attribute name="handling_type"><xsl:text>Clip</xsl:text></xsl:attribute>
                  <xsl:for-each select="//*[local-name() = 'Asset']/*[local-name() = 'Metadata']/*[local-name() = 'AMS'][@Asset_Class = 'title']">
                    <xsl:attribute name="duration">
                      <xsl:value-of select="ADIUtils:GetFileDuration(../*[local-name() = 'App_Data'][@Name = 'Run_Time']/@Value)"/>
                    </xsl:attribute>
                  </xsl:for-each>
                  <xsl:attribute name="cdn_code">
                    <xsl:value-of select="../../*[local-name() = 'Content']/@Value" />
                  </xsl:attribute>
                  <xsl:attribute name="break_rule"><xsl:value-of select="ADIUtils:GetAdProvider()"/></xsl:attribute>   
                <xsl:attribute name="cdn_name"><xsl:text>Direct Link</xsl:text></xsl:attribute>
                <xsl:for-each select="//*[local-name() = 'Asset']/*[local-name() = 'Metadata']/*[local-name() = 'AMS'][@Asset_Class = 'title']">
                  <xsl:attribute name="break_points">
                    <xsl:value-of select="../*[local-name() = 'App_Data'][@Name = 'Ad_Break']/@Value"/>
                  </xsl:attribute>
                </xsl:for-each>
                <xsl:attribute name="billing_type">
                  <xsl:text>Tvinci</xsl:text>
                </xsl:attribute>
                <xsl:attribute name="ppv_module">
                  <xsl:variable name="assetProduct" select="//*[local-name() = 'Metadata']/*[local-name() = 'AMS']/@Product"/>
                  <xsl:for-each select="//*[local-name() = 'Asset']/*[local-name() = 'Metadata']/*[local-name() = 'AMS'][@Asset_Class = 'title']">
                    <xsl:value-of select="concat($assetProduct,../*[local-name() = 'App_Data'][@Name = 'Billing_ID']/@Value)"/>
                  </xsl:for-each>
                </xsl:attribute>
                <xsl:attribute name="co_guid">
                  <xsl:value-of select="@Asset_ID" />
                </xsl:attribute>
              </xsl:element>
            </xsl:when>
        
            <xsl:when test="translate(@Asset_Class, $uppercase, $smallcase) = 'preview'">
              <xsl:element name="file">
                <xsl:attribute name="type"><xsl:text>Trailer</xsl:text></xsl:attribute>
                <xsl:attribute name="quality"><xsl:text>HIGH</xsl:text></xsl:attribute>
                <xsl:attribute name="pre_rule"></xsl:attribute>
                  <xsl:attribute name="post_rule"></xsl:attribute>
                  <xsl:attribute name="handling_type"><xsl:text>Clip</xsl:text></xsl:attribute>
                  <xsl:attribute name="duration">
                    <xsl:value-of select="ADIUtils:GetFileDuration(../*[local-name() = 'App_Data'][@Name = 'Run_Time']/@Value)"/>
                  </xsl:attribute>
                  <xsl:attribute name="cdn_code">
                    <xsl:value-of select="../../*[local-name() = 'Content']/@Value" />
                  </xsl:attribute>
                  <xsl:attribute name="break_rule"></xsl:attribute>   
                <xsl:attribute name="cdn_name"><xsl:text>Direct Link</xsl:text></xsl:attribute>
                <xsl:for-each select="//*[local-name() = 'Asset']/*[local-name() = 'Metadata']/*[local-name() = 'AMS'][@Asset_Class = 'title']">
                  <xsl:attribute name="break_points"></xsl:attribute>
                </xsl:for-each>
                <xsl:attribute name="billing_type">
                  <xsl:text>Tvinci</xsl:text>
                </xsl:attribute>
                <xsl:attribute name="ppv_module">
                  <xsl:variable name="assetProduct" select="//*[local-name() = 'Metadata']/*[local-name() = 'AMS']/@Product"/>
                  <xsl:for-each select="//*[local-name() = 'Asset']/*[local-name() = 'Metadata']/*[local-name() = 'AMS'][@Asset_Class = 'title']">
                    <xsl:value-of select="concat($assetProduct,../*[local-name() = 'App_Data'][@Name = 'Billing_ID']/@Value)"/>
                  </xsl:for-each>
                </xsl:attribute>
                <xsl:attribute name="co_guid">
                  <xsl:value-of select="@Asset_ID" />
                </xsl:attribute>
              </xsl:element>
            </xsl:when>
        
            <xsl:when test="translate(@Asset_Class, $uppercase, $smallcase) != 'preview' and translate(@Asset_Class, $uppercase, $smallcase) != 'movie' and translate(@Asset_Class, $uppercase, $smallcase) != 'box cover' and translate(@Asset_Class, $uppercase, $smallcase) != 'poster'">
              <xsl:element name="file">
                <xsl:attribute name="type"><xsl:value-of select="ADIUtils:ParseFileType(@Asset_ID)"/></xsl:attribute>
                <xsl:attribute name="quality"><xsl:text>HIGH</xsl:text></xsl:attribute>
                <xsl:attribute name="pre_rule"><xsl:value-of select="ADIUtils:GetAdProvider()"/></xsl:attribute>
                <xsl:attribute name="post_rule"><xsl:value-of select="ADIUtils:GetAdProvider()"/></xsl:attribute>
                <xsl:attribute name="handling_type"><xsl:text>Clip</xsl:text></xsl:attribute>
                <xsl:for-each select="//*[local-name() = 'Asset']/*[local-name() = 'Metadata']/*[local-name() = 'AMS'][@Asset_Class = 'title']">
                  <xsl:attribute name="duration">
                    <xsl:value-of select="ADIUtils:GetFileDuration(../*[local-name() = 'App_Data'][@Name = 'Run_Time']/@Value)"/>
                  </xsl:attribute>
                </xsl:for-each>
                <xsl:attribute name="cdn_name"><xsl:text>Akamai</xsl:text></xsl:attribute>
                <xsl:attribute name="cdn_code">
                  <xsl:value-of select="../../*[local-name() = 'Content']/@Value" />
                </xsl:attribute>
                <xsl:attribute name="break_rule"><xsl:value-of select="ADIUtils:GetAdProvider()"/></xsl:attribute>   
                <xsl:for-each select="//*[local-name() = 'Asset']/*[local-name() = 'Metadata']/*[local-name() = 'AMS'][@Asset_Class = 'title']">
                  <xsl:attribute name="break_points">
                    <xsl:value-of select="../*[local-name() = 'App_Data'][@Name = 'Ad_Break']/@Value"/>
                  </xsl:attribute>
                </xsl:for-each>
                <xsl:attribute name="billing_type"><xsl:text>Tvinci</xsl:text></xsl:attribute>
                <xsl:attribute name="ppv_module">
                <xsl:variable name="assetProduct" select="//*[local-name() = 'Metadata']/*[local-name() = 'AMS']/@Product"/>
                  <xsl:for-each select="//*[local-name() = 'Asset']/*[local-name() = 'Metadata']/*[local-name() = 'AMS'][@Asset_Class = 'title']">
                    <xsl:value-of select="concat($assetProduct,../*[local-name() = 'App_Data'][@Name = 'Billing_ID']/@Value)"/>
                  </xsl:for-each>
                </xsl:attribute>
                <xsl:attribute name="co_guid">
                  <xsl:value-of select="@Asset_ID" />
                </xsl:attribute>
              </xsl:element>
            </xsl:when>
          </xsl:choose>
        </xsl:if>
      </xsl:for-each>
    </xsl:element>
  </xsl:template>

  <xsl:template name="build_basic_data">
    <xsl:element name="basic">    
      <xsl:element name="name">
        <xsl:element name="value">
          <xsl:attribute name="lang">eng</xsl:attribute>
          <xsl:call-template name="set_media_name"/>
        </xsl:element>
      </xsl:element>
      <xsl:element name="description">
        <xsl:element name="value">
          <xsl:attribute name="lang">eng</xsl:attribute>
          <xsl:call-template name="set_media_description"/>
        </xsl:element>
      </xsl:element>
      <xsl:element name="media_type">
        <xsl:for-each select="//*[local-name() = 'Asset']/*[local-name() = 'Metadata']/*[local-name() = 'AMS'][@Asset_Class = 'title']">
          <xsl:variable name="mt" select="../*[local-name() = 'App_Data'][@Name = 'Show_Type']/@Value" />
          <xsl:choose>
            <xsl:when test="translate($mt, $uppercase, $smallcase) = 'movie'">
              <xsl:text>Movie</xsl:text>
            </xsl:when>
            <xsl:when test="translate($mt, $uppercase, $smallcase) = 'series'">
              <xsl:text>Episode</xsl:text>
            </xsl:when>
            <xsl:when test="translate($mt, $uppercase, $smallcase) = 'extra'">
              <xsl:text>Extra</xsl:text>
            </xsl:when>
            <xsl:when test="translate($mt, $uppercase, $smallcase) = 'news'">
              <xsl:text>News</xsl:text>
            </xsl:when>
            <xsl:when test="translate($mt, $uppercase, $smallcase) = 'education'">
              <xsl:text>Education</xsl:text>
            </xsl:when>
          </xsl:choose>
        </xsl:for-each>
      </xsl:element>
      <xsl:for-each select="//*[local-name() = 'Asset']/*[local-name() = 'Metadata']/*[local-name() = 'AMS'][@Asset_Class = 'poster']">
        <xsl:if test="../../*[local-name() = 'Content']/@Value">
          <xsl:element name="thumb">
            <xsl:attribute name="url">
                <xsl:value-of select="../../*[local-name() = 'Content']/@Value"/>
            </xsl:attribute>
          </xsl:element>
        </xsl:if>
      </xsl:for-each>
      <xsl:element name="epg_identifier">
        <xsl:text>0</xsl:text>
      </xsl:element>
      <xsl:element name="rules">
        <xsl:element name="geo_block_rule">
          <xsl:for-each select="//*[local-name() = 'Asset']/*[local-name() = 'Metadata']/*[local-name() = 'AMS'][@Asset_Class = 'title']">
            <xsl:if test="translate(../*[local-name() = 'App_Data'][@Name = 'Geo_Block_Rule']/@Value, $uppercase, $smallcase) = 'singapore'">
              <xsl:text>Singapore only</xsl:text>
            </xsl:if>
          </xsl:for-each>
        </xsl:element>
        <xsl:element name="device_rule">
          <xsl:for-each select="//*[local-name() = 'Asset']/*[local-name() = 'Metadata']/*[local-name() = 'AMS'][@Asset_Class = 'title']">
            <xsl:value-of select="../*[local-name() = 'App_Data'][@Name = 'Device_Rule']/@Value"/>
          </xsl:for-each>
        </xsl:element>
        <xsl:element name="watch_per_rule">
          <xsl:text>Parent allowed</xsl:text>
        </xsl:element>
      </xsl:element>
      <xsl:element name="dates">
        <xsl:element name="start">
          <xsl:for-each select="//*[local-name() = 'Asset']/*[local-name() = 'Metadata']/*[local-name() = 'AMS'][@Asset_Class = 'title']">
              <xsl:value-of select="ADIUtils:ParseDateValue(../*[local-name() = 'App_Data'][@Name = 'Licensing_Window_Start']/@Value)"/>
          </xsl:for-each>
        </xsl:element>
        <xsl:element name="catalog_end">
          <xsl:for-each select="//*[local-name() = 'Asset']/*[local-name() = 'Metadata']/*[local-name() = 'AMS'][@Asset_Class = 'title']">
            <xsl:value-of select="ADIUtils:ParseDateValue(../*[local-name() = 'App_Data'][@Name = 'Licensing_Window_End']/@Value)"/>
          </xsl:for-each>
        </xsl:element>
        <xsl:element name="final_end">
          <xsl:for-each select="//*[local-name() = 'Asset']/*[local-name() = 'Metadata']/*[local-name() = 'AMS'][@Asset_Class = 'title']">
            <xsl:value-of select="ADIUtils:ParseFinalDateValue(../*[local-name() = 'App_Data'][@Name = 'Purge_Date']/@Value)"/>
          </xsl:for-each>
        </xsl:element>
      </xsl:element>
      <xsl:element name="pic_ratios">
        <xsl:for-each select="//*[local-name() = 'Asset']/*[local-name() = 'Metadata']/*[local-name() = 'AMS'][@Asset_Class = 'poster']">
          <xsl:if test="../../*[local-name() = 'Content']/@Value">
            <xsl:element name="ratio">
              <xsl:attribute name="thumb">
                  <xsl:value-of select="../../*[local-name() = 'Content']/@Value"/>
              </xsl:attribute>
              <xsl:attribute name="ratio">
                <xsl:text>16:9</xsl:text>
              </xsl:attribute>
            </xsl:element>
          </xsl:if>
        </xsl:for-each>
        <xsl:for-each select="//*[local-name() = 'Asset']/*[local-name() = 'Metadata']/*[local-name() = 'AMS'][@Asset_Class = 'box cover']">
          <xsl:if test="../../*[local-name() = 'Content']/@Value">
            <xsl:element name="ratio">
              <xsl:attribute name="thumb">
                  <xsl:value-of select="../../*[local-name() = 'Content']/@Value"/>
              </xsl:attribute>
              <xsl:attribute name="ratio">
                <xsl:text>2:3</xsl:text>
              </xsl:attribute>
            </xsl:element>
          </xsl:if>
        </xsl:for-each>
      </xsl:element>
    </xsl:element>
  </xsl:template>

  <xsl:template name="set_media_description">
    <xsl:for-each select="//*[local-name() = 'Asset']/*[local-name() = 'Metadata']/*[local-name() = 'AMS'][@Asset_Class = 'title']">
      <xsl:choose>
        <xsl:when test="../*[local-name() = 'App_Data'][@Name = 'Summary_Short']/@Value">
          <xsl:value-of select="../*[local-name() = 'App_Data'][@Name = 'Summary_Short']/@Value"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="../*[local-name() = 'App_Data'][@Name = 'Summary_Long']/@Value"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:for-each>
  </xsl:template>

  <xsl:template name="set_media_name">
    <xsl:for-each select="//*[local-name() = 'Asset']/*[local-name() = 'Metadata']/*[local-name() = 'AMS'][@Asset_Class = 'title']">
      <xsl:variable name="show_type" select="translate(../*[local-name() = 'App_Data'][@Name = 'Show_Type']/@Value, $uppercase, $smallcase)" />
      <xsl:choose>
        <xsl:when test="$show_type = 'series'">
          <xsl:variable name="serID" select="concat(../*[local-name() = 'App_Data'][@Name = 'SeriesId']/@Value,' -  Episode ')"/>
          <xsl:value-of select="concat($serID, ../*[local-name() = 'App_Data'][@Name = 'Episode_ID']/@Value)"/>
        </xsl:when>
        <xsl:when test="$show_type = 'movie'">
          <xsl:value-of select="../*[local-name() = 'App_Data'][@Name = 'Title']/@Value"/>
        </xsl:when>
        <xsl:when test="$show_type = 'news' or $show_type = 'education'">
          <xsl:variable name="serID" select="concat(../*[local-name() = 'App_Data'][@Name = 'SeriesId']/@Value,' - ')"/>
          <xsl:value-of select="concat($serID, ../*[local-name() = 'App_Data'][@Name = 'Episode_Name']/@Value)"/>
        </xsl:when>
        <xsl:when test="$show_type = 'extra'">
          <xsl:value-of select="../*[local-name() = 'App_Data'][@Name = 'Episode_Name']/@Value"/>
        </xsl:when>
      </xsl:choose>
    </xsl:for-each>
  </xsl:template>

  <xsl:template name="build_structure_data">
    <xsl:element name="structure">
      <xsl:element name="strings">
        <xsl:call-template name="build_strings_data"/>
      </xsl:element>
      <xsl:element name="booleans">
        <xsl:call-template name="build_booleans_data"/>
      </xsl:element>
      <xsl:element name="doubles">
        <xsl:call-template name="build_doubles_data"/>
      </xsl:element>
      <xsl:element name="metas">
        <xsl:call-template name="build_tags_data"/>
      </xsl:element>
    </xsl:element>
  </xsl:template>

  <xsl:template name="build_strings_data">
    <xsl:for-each select="//*[local-name() = 'Asset']/*[local-name() = 'Metadata']/*[local-name() = 'AMS'][@Asset_Class = 'title']">
      <xsl:if test="../*[local-name() = 'App_Data'][@Name = 'Licensing_Window_Start']/@Value">
        <xsl:element name="meta">
          <xsl:attribute name="name">Licensing window start</xsl:attribute>
          <xsl:attribute name="ml_handling">duplicate</xsl:attribute>
          <xsl:element name="value">
            <xsl:attribute name="lang">eng</xsl:attribute>
              <xsl:value-of select="ADIUtils:ParseDateValue(../*[local-name() = 'App_Data'][@Name = 'Licensing_Window_Start']/@Value)"/>
          </xsl:element>
        </xsl:element>
      </xsl:if>
    </xsl:for-each>
    <xsl:for-each select="//*[local-name() = 'Asset']/*[local-name() = 'Metadata']/*[local-name() = 'AMS'][@Asset_Class = 'title']">
      <xsl:if test="../*[local-name() = 'App_Data'][@Name = 'Licensing_Window_End']/@Value">
        <xsl:element name="meta">
          <xsl:attribute name="name">Licensing window end</xsl:attribute>
          <xsl:attribute name="ml_handling">duplicate</xsl:attribute>
          <xsl:element name="value">
            <xsl:attribute name="lang">eng</xsl:attribute>
              <xsl:value-of select="ADIUtils:ParseDateValue(../*[local-name() = 'App_Data'][@Name = 'Licensing_Window_End']/@Value)"/>
          </xsl:element>
        </xsl:element>
      </xsl:if>
    </xsl:for-each>
    <xsl:for-each select="//*[local-name() = 'Asset']/*[local-name() = 'Metadata']/*[local-name() = 'AMS'][@Asset_Class = 'title']">
      <xsl:if test="../*[local-name() = 'App_Data'][@Name = 'Summary_Short']/@Value">
        <xsl:element name="meta">
          <xsl:attribute name="name">Short summary</xsl:attribute>
          <xsl:attribute name="ml_handling">duplicate</xsl:attribute>
          <xsl:element name="value">
            <xsl:attribute name="lang">eng</xsl:attribute>
              <xsl:value-of select="../*[local-name() = 'App_Data'][@Name = 'Summary_Short']/@Value"/>
          </xsl:element>
        </xsl:element>
      </xsl:if>
    </xsl:for-each>
    <xsl:for-each select="//*[local-name() = 'Asset']/*[local-name() = 'Metadata']/*[local-name() = 'AMS'][@Asset_Class = 'title']">
      <xsl:if test="../*[local-name() = 'App_Data'][@Name = 'Title_Brief']/@Value">
        <xsl:element name="meta">
          <xsl:attribute name="name">Short title</xsl:attribute>
          <xsl:attribute name="ml_handling">duplicate</xsl:attribute>
          <xsl:element name="value">
            <xsl:attribute name="lang">eng</xsl:attribute>
              <xsl:value-of select="../*[local-name() = 'App_Data'][@Name = 'Title_Brief']/@Value"/>
          </xsl:element>
        </xsl:element>
      </xsl:if>
    </xsl:for-each>
    <xsl:for-each select="//*[local-name() = 'Asset']/*[local-name() = 'Metadata']/*[local-name() = 'AMS'][@Asset_Class = 'title']">
      <xsl:if test="../*[local-name() = 'App_Data'][@Name = 'Billing_ID']/@Value">
      <xsl:element name="meta">
        <xsl:attribute name="name">Billing ID</xsl:attribute>
        <xsl:attribute name="ml_handling">duplicate</xsl:attribute>
        <xsl:element name="value">
          <xsl:attribute name="lang">eng</xsl:attribute>
          <xsl:value-of select="../*[local-name() = 'App_Data'][@Name = 'Billing_ID']/@Value"/>
        </xsl:element>
      </xsl:element>
      </xsl:if>
    </xsl:for-each>
    <xsl:for-each select="//*[local-name() = 'Asset']/*[local-name() = 'Metadata']/*[local-name() = 'AMS'][@Asset_Class = 'title']">
      <xsl:if test="../*[local-name() = 'App_Data'][@Name = 'Billing_ID']/@Value">
        <xsl:element name="meta">
          <xsl:attribute name="name">PPVModule</xsl:attribute>
          <xsl:attribute name="ml_handling">duplicate</xsl:attribute>
          <xsl:element name="value">
            <xsl:attribute name="lang">eng</xsl:attribute>
            <xsl:variable name="assetProduct" select="//*[local-name() = 'Metadata']/*[local-name() = 'AMS']/@Product"/>
              <xsl:value-of select="concat($assetProduct,../*[local-name() = 'App_Data'][@Name = 'Billing_ID']/@Value)"/>
          </xsl:element>
        </xsl:element>
      </xsl:if>
    </xsl:for-each>
    <xsl:for-each select="//*[local-name() = 'Asset']/*[local-name() = 'Metadata']/*[local-name() = 'AMS'][@Asset_Class = 'title']">
      <xsl:if test="../*[local-name() = 'App_Data'][@Name = 'Episode_Name']/@Value">
        <xsl:element name="meta">
          <xsl:attribute name="name">Episode name</xsl:attribute>
          <xsl:attribute name="ml_handling">duplicate</xsl:attribute>
          <xsl:element name="value">
            <xsl:attribute name="lang">eng</xsl:attribute>
              <xsl:value-of select="../*[local-name() = 'App_Data'][@Name = 'Episode_Name']/@Value"/>
          </xsl:element>
        </xsl:element>
      </xsl:if>
    </xsl:for-each>
    <xsl:for-each select="//*[local-name() = 'Asset']/*[local-name() = 'Metadata']/*[local-name() = 'AMS'][@Asset_Class = 'title']">
      <xsl:if test="../*[local-name() = 'App_Data'][@Name = 'SeasonPackageAssetID']/@Value">
        <xsl:element name="meta">
          <xsl:attribute name="name">SeasonPackageAssetID</xsl:attribute>
          <xsl:attribute name="ml_handling">duplicate</xsl:attribute>
          <xsl:element name="value">
            <xsl:attribute name="lang">eng</xsl:attribute>
              <xsl:value-of select="../*[local-name() = 'App_Data'][@Name = 'SeasonPackageAssetID']/@Value"/>
          </xsl:element>
        </xsl:element>
      </xsl:if>
    </xsl:for-each>
    <xsl:for-each select="//*[local-name() = 'Asset']/*[local-name() = 'Metadata']/*[local-name() = 'AMS'][@Asset_Class = 'title']">
      <xsl:if test="../*[local-name() = 'App_Data'][@Name = 'Hash_Tag']/@Value">
        <xsl:element name="meta">
          <xsl:attribute name="name">Hashtag</xsl:attribute>
          <xsl:attribute name="ml_handling">duplicate</xsl:attribute>
          <xsl:element name="value">
            <xsl:attribute name="lang">eng</xsl:attribute>
              <xsl:value-of select="../*[local-name() = 'App_Data'][@Name = 'Hash_Tag']/@Value"/>
          </xsl:element>
        </xsl:element>
      </xsl:if>
    </xsl:for-each>
    <xsl:for-each select="//*[local-name() = 'Asset']/*[local-name() = 'Metadata']/*[local-name() = 'AMS'][@Asset_Class = 'title']">
      <xsl:if test="../*[local-name() = 'App_Data'][@Name = 'Short_Pinyin']/@Value">
        <xsl:element name="meta">
          <xsl:attribute name="name">Short Pinyin title</xsl:attribute>
          <xsl:attribute name="ml_handling">duplicate</xsl:attribute>
          <xsl:element name="value">
            <xsl:attribute name="lang">eng</xsl:attribute>
              <xsl:value-of select="../*[local-name() = 'App_Data'][@Name = 'Short_Pinyin']/@Value"/>
          </xsl:element>
        </xsl:element>
      </xsl:if>
    </xsl:for-each>
    <xsl:for-each select="//*[local-name() = 'Asset']/*[local-name() = 'Metadata']/*[local-name() = 'AMS'][@Asset_Class = 'title']">
      <xsl:if test="../*[local-name() = 'App_Data'][@Name = 'Long_Pinyin']/@Value">
        <xsl:element name="meta">
          <xsl:attribute name="name">Long Pinyin title</xsl:attribute>
          <xsl:attribute name="ml_handling">duplicate</xsl:attribute>
          <xsl:element name="value">
            <xsl:attribute name="lang">eng</xsl:attribute>
              <xsl:value-of select="../*[local-name() = 'App_Data'][@Name = 'Long_Pinyin']/@Value"/>
          </xsl:element>
        </xsl:element>
      </xsl:if>
    </xsl:for-each>
    <xsl:for-each select="//*[local-name() = 'Asset']/*[local-name() = 'Metadata']/*[local-name() = 'AMS'][@Asset_Class = 'title']">
      <xsl:if test="../*[local-name() = 'App_Data'][@Name = 'Url']/@Value">
        <xsl:element name="meta">
          <xsl:attribute name="name">URL</xsl:attribute>
          <xsl:attribute name="ml_handling">duplicate</xsl:attribute>
          <xsl:element name="value">
            <xsl:attribute name="lang">eng</xsl:attribute>
              <xsl:value-of select="../*[local-name() = 'App_Data'][@Name = 'Url']/@Value"/>
          </xsl:element>
        </xsl:element>
      </xsl:if>
    </xsl:for-each>
    <xsl:for-each select="//*[local-name() = 'Asset']/*[local-name() = 'Metadata']/*[local-name() = 'AMS'][@Asset_Class = 'title']">
      <xsl:if test="../*[local-name() = 'App_Data'][@Name = 'TX_Date']/@Value">
        <xsl:element name="meta">
          <xsl:attribute name="name">TX Date</xsl:attribute>
          <xsl:attribute name="ml_handling">duplicate</xsl:attribute>
          <xsl:element name="value">
            <xsl:attribute name="lang">eng</xsl:attribute>
              <xsl:value-of select="../*[local-name() = 'App_Data'][@Name = 'TX_Date']/@Value"/>
          </xsl:element>
        </xsl:element>
      </xsl:if>
    </xsl:for-each>
    <xsl:for-each select="//*[local-name() = 'Asset']/*[local-name() = 'Metadata']/*[local-name() = 'AMS'][@Asset_Class = 'title']">
      <xsl:if test="../*[local-name() = 'App_Data'][@Name = 'IOS_Product']/@Value">
        <xsl:element name="meta">
          <xsl:attribute name="name">IOS Product</xsl:attribute>
          <xsl:attribute name="ml_handling">duplicate</xsl:attribute>
          <xsl:element name="value">
            <xsl:attribute name="lang">eng</xsl:attribute>
              <xsl:value-of select="../*[local-name() = 'App_Data'][@Name = 'IOS_Product']/@Value"/>
          </xsl:element>
        </xsl:element>
      </xsl:if>
    </xsl:for-each>
    <xsl:for-each select="//*[local-name() = 'Asset']/*[local-name() = 'Metadata']/*[local-name() = 'AMS'][@Asset_Class = 'title']">
      <xsl:if test="../*[local-name() = 'App_Data'][@Name = 'DTW_Product']/@Value">
        <xsl:element name="meta">
          <xsl:attribute name="name">DTW Product</xsl:attribute>
          <xsl:attribute name="ml_handling">duplicate</xsl:attribute>
          <xsl:element name="value">
            <xsl:attribute name="lang">eng</xsl:attribute>
              <xsl:value-of select="../*[local-name() = 'App_Data'][@Name = 'DTW_Product']/@Value"/>
          </xsl:element>
        </xsl:element>
      </xsl:if>
    </xsl:for-each>
    <xsl:for-each select="//*[local-name() = 'Asset']/*[local-name() = 'Metadata']/*[local-name() = 'AMS'][@Asset_Class = 'title']">
      <xsl:if test="../*[local-name() = 'App_Data'][@Name = 'DTW_Billing_Code']/@Value">
        <xsl:element name="meta">
          <xsl:attribute name="name">DTW Billing Code</xsl:attribute>
          <xsl:attribute name="ml_handling">duplicate</xsl:attribute>
          <xsl:element name="value">
            <xsl:attribute name="lang">eng</xsl:attribute>
              <xsl:value-of select="../*[local-name() = 'App_Data'][@Name = 'DTW_Billing_Code']/@Value"/>
          </xsl:element>
        </xsl:element>
      </xsl:if>
    </xsl:for-each>
  </xsl:template>

  <xsl:template name="build_booleans_data">
    <xsl:element name="meta">
      <xsl:attribute name="name">Season premiere</xsl:attribute>
      <xsl:for-each select="//*[local-name() = 'Asset']/*[local-name() = 'Metadata']/*[local-name() = 'AMS'][@Asset_Class = 'title']">
        <xsl:choose>
          <xsl:when test="translate(../*[local-name() = 'App_Data'][@Name = 'Season_Premiere']/@Value, $uppercase, $smallcase) = 'y'">
            <xsl:text>true</xsl:text>
          </xsl:when>
          <xsl:otherwise>
            <xsl:text>0</xsl:text>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:for-each>
    </xsl:element>
    <xsl:element name="meta">
      <xsl:attribute name="name">season_finale</xsl:attribute>
      <xsl:for-each select="//*[local-name() = 'Asset']/*[local-name() = 'Metadata']/*[local-name() = 'AMS'][@Asset_Class = 'title']">
        <xsl:choose>
          <xsl:when test="translate(../*[local-name() = 'App_Data'][@Name = 'season_finale']/@Value, $uppercase, $smallcase) = 'y'">
            <xsl:text>true</xsl:text>
          </xsl:when>
          <xsl:otherwise>
            <xsl:text>0</xsl:text>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:for-each>
    </xsl:element>
    <xsl:element name="meta">
      <xsl:attribute name="name">Closed captions available</xsl:attribute>
      <xsl:for-each select="//*[local-name() = 'Asset']/*[local-name() = 'Metadata']/*[local-name() = 'AMS'][@Asset_Class = 'title']">
        <xsl:choose>
          <xsl:when test="translate(../*[local-name() = 'App_Data'][@Name = 'Closed_Captioning']/@Value, $uppercase, $smallcase) = 'y'">
            <xsl:text>true</xsl:text>
          </xsl:when>
          <xsl:otherwise>
            <xsl:text>0</xsl:text>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:for-each>
    </xsl:element>
    <xsl:element name="meta">
      <xsl:attribute name="name">Interactive</xsl:attribute>
      <xsl:for-each select="//*[local-name() = 'Asset']/*[local-name() = 'Metadata']/*[local-name() = 'AMS'][@Asset_Class = 'title']">
        <xsl:choose>
          <xsl:when test="translate(../*[local-name() = 'App_Data'][@Name = 'Interactive']/@Value, $uppercase, $smallcase) = 'y'">
            <xsl:text>true</xsl:text>
          </xsl:when>
          <xsl:otherwise>
            <xsl:text>0</xsl:text>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:for-each>
    </xsl:element>
    <xsl:element name="meta">
      <xsl:attribute name="name">is_active</xsl:attribute>
      <xsl:for-each select="//*[local-name() = 'Asset']/*[local-name() = 'Metadata']/*[local-name() = 'AMS'][@Asset_Class = 'title']">
        <xsl:choose>
          <xsl:when test="translate(../*[local-name() = 'App_Data'][@Name = 'Asset_Is_Active']/@Value, $uppercase, $smallcase) = 'true'">
            <xsl:text>true</xsl:text>
          </xsl:when>
          <xsl:otherwise>
            <xsl:text>0</xsl:text>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:for-each>
    </xsl:element>
  </xsl:template>

  <xsl:template name="build_doubles_data">
    <xsl:for-each select="//*[local-name() = 'Asset']/*[local-name() = 'Metadata']/*[local-name() = 'AMS'][@Asset_Class = 'title']">
      <xsl:if test="../*[local-name() = 'App_Data'][@Name = 'Year']/@Value">
        <xsl:element name="meta">
          <xsl:attribute name="name">Release year</xsl:attribute>
          <xsl:value-of select="../*[local-name() = 'App_Data'][@Name = 'Year']/@Value"/>
        </xsl:element>
      </xsl:if>
    </xsl:for-each>
    <xsl:for-each select="//*[local-name() = 'Asset']/*[local-name() = 'Metadata']/*[local-name() = 'AMS'][@Asset_Class = 'title']">
        <xsl:variable name="eID" select="../*[local-name() = 'App_Data'][@Name = 'Episode_ID']/@Value"/>
        <xsl:variable name="ENumSNum" select="ADIUtils:ParseENumSNum($eID)"/>
        <xsl:element name="meta">
          <xsl:attribute name="name">Episode number</xsl:attribute>
          <xsl:value-of select="substring-before($ENumSNum,'|')"/>
        </xsl:element>
        <xsl:element name="meta">
          <xsl:attribute name="name">Season number</xsl:attribute>
          <xsl:value-of select="substring-after($ENumSNum,'|')"/>
        </xsl:element>
    </xsl:for-each>
  </xsl:template>

  <xsl:template name="build_tags_data">
    <xsl:element name="meta">
      <xsl:attribute name="name">Provider</xsl:attribute>
      <xsl:attribute name="ml_handling">unique</xsl:attribute>
      <xsl:variable name="fullStr" select="//*[local-name() = 'Metadata']/*[local-name() = 'AMS']/@Provider"/>
      <xsl:call-template name="ParseTag">
        <xsl:with-param name="fullString" select="$fullStr"/>
      </xsl:call-template>
    </xsl:element>
    <xsl:element name="meta">
      <xsl:attribute name="name">Provider ID</xsl:attribute>
      <xsl:attribute name="ml_handling">unique</xsl:attribute>
      <xsl:variable name="fullStr" select="//*[local-name() = 'Metadata']/*[local-name() = 'AMS']/@Provider_ID"/>
      <xsl:call-template name="ParseTag">
        <xsl:with-param name="fullString" select="$fullStr"/>
      </xsl:call-template>
    </xsl:element>
    <xsl:for-each select="//*[local-name() = 'Asset']/*[local-name() = 'Metadata']/*[local-name() = 'AMS'][@Asset_Class = 'title']">
      <xsl:if test="../*[local-name() = 'App_Data'][@Name = 'Rating']/@Value">
        <xsl:element name="meta">
          <xsl:attribute name="name">Rating</xsl:attribute>
          <xsl:attribute name="ml_handling">unique</xsl:attribute>
            <xsl:for-each select="../*[local-name() = 'App_Data'][@Name = 'Rating']">
              <xsl:variable name="fullStr" select="@Value"/>
              <xsl:call-template name="ParseTag">
                <xsl:with-param name="fullString" select="$fullStr"/>
              </xsl:call-template>
            </xsl:for-each>
        </xsl:element>
      </xsl:if>
    </xsl:for-each>
    <xsl:for-each select="//*[local-name() = 'Asset']/*[local-name() = 'Metadata']/*[local-name() = 'AMS'][@Asset_Class = 'title']">
      <xsl:if test="../*[local-name() = 'App_Data'][@Name = 'Genre']/@Value">
        <xsl:element name="meta">
          <xsl:attribute name="name">Genre</xsl:attribute>
          <xsl:attribute name="ml_handling">unique</xsl:attribute>
            <xsl:for-each select="../*[local-name() = 'App_Data'][@Name = 'Genre']">
              <xsl:variable name="fullStr" select="@Value"/>
              <xsl:call-template name="ParseTag">
                <xsl:with-param name="fullString" select="$fullStr"/>
              </xsl:call-template>
            </xsl:for-each>
        </xsl:element>
      </xsl:if>
    </xsl:for-each>
    <xsl:for-each select="//*[local-name() = 'Asset']/*[local-name() = 'Metadata']/*[local-name() = 'AMS'][@Asset_Class = 'title']">
      <xsl:if test="../*[local-name() = 'App_Data'][@Name = 'Product']/@Value">
        <xsl:element name="meta">
          <xsl:attribute name="name">Product</xsl:attribute>
          <xsl:attribute name="ml_handling">unique</xsl:attribute>
            <xsl:for-each select="../*[local-name() = 'App_Data'][@Name = 'Product']">
              <xsl:variable name="fullStr" select="@Value"/>
              <xsl:call-template name="ParseTag">
                <xsl:with-param name="fullString" select="$fullStr"/>
              </xsl:call-template>
            </xsl:for-each>
        </xsl:element>
      </xsl:if>
    </xsl:for-each>
    <xsl:for-each select="//*[local-name() = 'Asset']/*[local-name() = 'Metadata']/*[local-name() = 'AMS'][@Asset_Class = 'title']">
      <xsl:if test="../*[local-name() = 'App_Data'][@Name = 'SeriesId']/@Value">
        <xsl:element name="meta">
          <xsl:attribute name="name">Series name</xsl:attribute>
          <xsl:attribute name="ml_handling">unique</xsl:attribute>
            <xsl:for-each select="../*[local-name() = 'App_Data'][@Name = 'SeriesId']">
              <xsl:variable name="fullStr" select="@Value"/>
              <xsl:call-template name="ParseTag">
                <xsl:with-param name="fullString" select="$fullStr"/>
              </xsl:call-template>
            </xsl:for-each>
        </xsl:element>
      </xsl:if>
    </xsl:for-each>
    <xsl:for-each select="//*[local-name() = 'Asset']/*[local-name() = 'Metadata']/*[local-name() = 'AMS'][@Asset_Class = 'title']">
      <xsl:if test="../*[local-name() = 'App_Data'][@Name = 'Actors_Display']/@Value">
        <xsl:element name="meta">
          <xsl:attribute name="name">Main cast</xsl:attribute>
          <xsl:attribute name="ml_handling">unique</xsl:attribute>
            <xsl:for-each select="../*[local-name() = 'App_Data'][@Name = 'Actors_Display']">
              <xsl:variable name="fullStr" select="@Value"/>
              <xsl:call-template name="ParseTag">
                <xsl:with-param name="fullString" select="$fullStr"/>
              </xsl:call-template>
            </xsl:for-each>
        </xsl:element>
      </xsl:if>
    </xsl:for-each>
    <xsl:for-each select="//*[local-name() = 'Asset']/*[local-name() = 'Metadata']/*[local-name() = 'AMS'][@Asset_Class = 'title']">
      <xsl:if test="../*[local-name() = 'App_Data'][@Name = 'Category']/@Value">
        <xsl:element name="meta">
          <xsl:attribute name="name">Category</xsl:attribute>
          <xsl:attribute name="ml_handling">unique</xsl:attribute>
            <xsl:for-each select="../*[local-name() = 'App_Data'][@Name = 'Category']">
              <xsl:variable name="fullStr" select="@Value"/>
              <xsl:call-template name="ParseTag">
                <xsl:with-param name="fullString" select="$fullStr"/>
              </xsl:call-template>
            </xsl:for-each>
        </xsl:element>
      </xsl:if>
    </xsl:for-each>
    <xsl:for-each select="//*[local-name() = 'Asset']/*[local-name() = 'Metadata']/*[local-name() = 'AMS'][@Asset_Class = 'title']">
      <xsl:if test="../*[local-name() = 'App_Data'][@Name = 'Advisories']/@Value">
        <xsl:element name="meta">
          <xsl:attribute name="name">Rating advisories</xsl:attribute>
          <xsl:attribute name="ml_handling">unique</xsl:attribute>
            <xsl:for-each select="../*[local-name() = 'App_Data'][@Name = 'Advisories']">
              <xsl:variable name="fullStr" select="@Value"/>
              <xsl:call-template name="ParseTag">
                <xsl:with-param name="fullString" select="$fullStr"/>
              </xsl:call-template>
            </xsl:for-each>
        </xsl:element>
      </xsl:if>
    </xsl:for-each>
    <xsl:for-each select="//*[local-name() = 'Asset']/*[local-name() = 'Metadata']/*[local-name() = 'AMS'][@Asset_Class = 'title']">
      <xsl:if test="../*[local-name() = 'App_Data'][@Name = 'Director']/@Value">
        <xsl:element name="meta">
          <xsl:attribute name="name">Director</xsl:attribute>
          <xsl:attribute name="ml_handling">unique</xsl:attribute>
            <xsl:for-each select="../*[local-name() = 'App_Data'][@Name = 'Director']">
              <xsl:variable name="fullStr" select="@Value"/>
              <xsl:call-template name="ParseTag">
                <xsl:with-param name="fullString" select="$fullStr"/>
              </xsl:call-template>
          </xsl:for-each>
        </xsl:element>
      </xsl:if>
    </xsl:for-each>
    <xsl:for-each select="//*[local-name() = 'Asset']/*[local-name() = 'Metadata']/*[local-name() = 'AMS'][@Asset_Class = 'title']">
      <xsl:if test="../*[local-name() = 'App_Data'][@Name = 'Territory']/@Value">
        <xsl:element name="meta">
          <xsl:attribute name="name">Territory</xsl:attribute>
          <xsl:attribute name="ml_handling">unique</xsl:attribute>
            <xsl:for-each select="../*[local-name() = 'App_Data'][@Name = 'Territory']">
              <xsl:variable name="fullStr" select="@Value"/>
              <xsl:call-template name="ParseTag">
                <xsl:with-param name="fullString" select="$fullStr"/>
              </xsl:call-template>
          </xsl:for-each>
        </xsl:element>
      </xsl:if>
    </xsl:for-each>
    <xsl:for-each select="//*[local-name() = 'Asset']/*[local-name() = 'Metadata']/*[local-name() = 'AMS'][@Asset_Class = 'title']">
      <xsl:if test="../*[local-name() = 'App_Data'][@Name = 'Ad_Tag_1']/@Value">
        <xsl:element name="meta">
          <xsl:attribute name="name">Ad Tag 1</xsl:attribute>
          <xsl:attribute name="ml_handling">unique</xsl:attribute>
            <xsl:for-each select="../*[local-name() = 'App_Data'][@Name = 'Ad_Tag_1']">
              <xsl:variable name="fullStr" select="@Value"/>
              <xsl:call-template name="ParseTag">
                <xsl:with-param name="fullString" select="$fullStr"/>
              </xsl:call-template>
            </xsl:for-each>
        </xsl:element>
      </xsl:if>
    </xsl:for-each>
    <xsl:for-each select="//*[local-name() = 'Asset']/*[local-name() = 'Metadata']/*[local-name() = 'AMS'][@Asset_Class = 'title']">
      <xsl:if test="../*[local-name() = 'App_Data'][@Name = 'Ad_Tag_2']/@Value">
        <xsl:element name="meta">
          <xsl:attribute name="name">Ad Tag 2</xsl:attribute>
          <xsl:attribute name="ml_handling">unique</xsl:attribute>
            <xsl:for-each select="../*[local-name() = 'App_Data'][@Name = 'Ad_Tag_2']">
              <xsl:variable name="fullStr" select="@Value"/>
              <xsl:call-template name="ParseTag">
                <xsl:with-param name="fullString" select="$fullStr"/>
              </xsl:call-template>
            </xsl:for-each>
        </xsl:element>
      </xsl:if>
    </xsl:for-each>
    <xsl:for-each select="//*[local-name() = 'Asset']/*[local-name() = 'Metadata']/*[local-name() = 'AMS'][@Asset_Class = 'title']">
      <xsl:if test="../*[local-name() = 'App_Data'][@Name = 'Ad_Tag_3']/@Value">
        <xsl:element name="meta">
          <xsl:attribute name="name">Ad Tag 3</xsl:attribute>
          <xsl:attribute name="ml_handling">unique</xsl:attribute>
            <xsl:for-each select="../*[local-name() = 'App_Data'][@Name = 'Ad_Tag_3']">
              <xsl:variable name="fullStr" select="@Value"/>
              <xsl:call-template name="ParseTag">
                <xsl:with-param name="fullString" select="$fullStr"/>
              </xsl:call-template>
            </xsl:for-each>
        </xsl:element>
      </xsl:if>
    </xsl:for-each>
    <xsl:for-each select="//*[local-name() = 'Asset']/*[local-name() = 'Metadata']/*[local-name() = 'AMS'][@Asset_Class = 'title']">
      <xsl:if test="../*[local-name() = 'App_Data'][@Name = 'I_Channel_Category']/@Value">
        <xsl:element name="meta">
          <xsl:attribute name="name">Free</xsl:attribute>
          <xsl:attribute name="ml_handling">unique</xsl:attribute>
            <xsl:for-each select="../*[local-name() = 'App_Data'][@Name = 'I_Channel_Category']">
              <xsl:variable name="fullStr" select="@Value"/>
              <xsl:call-template name="ParseTag">
                <xsl:with-param name="fullString" select="$fullStr"/>
              </xsl:call-template>
            </xsl:for-each>
        </xsl:element>
      </xsl:if>
    </xsl:for-each>
    <xsl:for-each select="//*[local-name() = 'Asset']/*[local-name() = 'Metadata']/*[local-name() = 'AMS'][@Asset_Class = 'movie']">
      <xsl:if test="../*[local-name() = 'App_Data'][@Name = 'Languages']/@Value">
        <xsl:element name="meta">
          <xsl:attribute name="name">Audio language</xsl:attribute>
          <xsl:attribute name="ml_handling">unique</xsl:attribute>
            <xsl:for-each select="../*[local-name() = 'App_Data'][@Name = 'Languages']">
              <xsl:variable name="fullStr" select="@Value"/>
              <xsl:call-template name="ParseTag">
                <xsl:with-param name="fullString" select="$fullStr"/>
              </xsl:call-template>
            </xsl:for-each>
        </xsl:element>
      </xsl:if>
    </xsl:for-each>
    <xsl:for-each select="//*[local-name() = 'Asset']/*[local-name() = 'Metadata']/*[local-name() = 'AMS'][@Asset_Class = 'title']">
      <xsl:if test="../*[local-name() = 'App_Data'][@Name = 'Broadcaster']/@Value">
        <xsl:element name="meta">
          <xsl:attribute name="name">Broadcaster</xsl:attribute>
          <xsl:attribute name="ml_handling">unique</xsl:attribute>
            <xsl:for-each select="../*[local-name() = 'App_Data'][@Name = 'Broadcaster']">
              <xsl:variable name="fullStr" select="@Value"/>
              <xsl:call-template name="ParseTag">
                <xsl:with-param name="fullString" select="$fullStr"/>
              </xsl:call-template>
            </xsl:for-each>
        </xsl:element>
      </xsl:if>
    </xsl:for-each>
    <xsl:for-each select="//*[local-name() = 'Asset']/*[local-name() = 'Metadata']/*[local-name() = 'AMS'][@Asset_Class = 'title']">
      <xsl:if test="../*[local-name() = 'App_Data'][@Name = 'Search_Tag']/@Value">
        <xsl:element name="meta">
          <xsl:attribute name="name">Search Tag</xsl:attribute>
          <xsl:attribute name="ml_handling">unique</xsl:attribute>
            <xsl:for-each select="../*[local-name() = 'App_Data'][@Name = 'Search_Tag']">
              <xsl:variable name="fullStr" select="@Value"/>
              <xsl:call-template name="ParseTag">
                <xsl:with-param name="fullString" select="$fullStr"/>
              </xsl:call-template>
            </xsl:for-each>
        </xsl:element>
      </xsl:if>
    </xsl:for-each>
    <xsl:for-each select="//*[local-name() = 'Asset']/*[local-name() = 'Metadata']/*[local-name() = 'AMS'][@Asset_Class = 'title']">
      <xsl:if test="../*[local-name() = 'App_Data'][@Name = 'Extra_Type']/@Value">
        <xsl:element name="meta">
          <xsl:attribute name="name">Extra Type</xsl:attribute>
          <xsl:attribute name="ml_handling">unique</xsl:attribute>
            <xsl:for-each select="../*[local-name() = 'App_Data'][@Name = 'Extra_Type']">
              <xsl:variable name="fullStr" select="@Value"/>
              <xsl:call-template name="ParseTag">
                <xsl:with-param name="fullString" select="$fullStr"/>
              </xsl:call-template>
            </xsl:for-each>
        </xsl:element>
      </xsl:if>
    </xsl:for-each>
    <xsl:for-each select="//*[local-name() = 'Asset']/*[local-name() = 'Metadata']/*[local-name() = 'AMS'][@Asset_Class = 'title']">
      <xsl:if test="../*[local-name() = 'App_Data'][@Name = 'Extra_Virtual_Media_Link']/@Value">
        <xsl:element name="meta">
          <xsl:attribute name="name">Extra Virtual Media Link</xsl:attribute>
          <xsl:attribute name="ml_handling">unique</xsl:attribute>
            <xsl:for-each select="../*[local-name() = 'App_Data'][@Name = 'Extra_Virtual_Media_Link']">
              <xsl:variable name="fullStr" select="@Value"/>
              <xsl:call-template name="ParseTag">
                <xsl:with-param name="fullString" select="$fullStr"/>
              </xsl:call-template>
            </xsl:for-each>
        </xsl:element>
      </xsl:if>
    </xsl:for-each>
    <xsl:for-each select="//*[local-name() = 'Asset']/*[local-name() = 'Metadata']/*[local-name() = 'AMS'][@Asset_Class = 'title']">
      <xsl:if test="../*[local-name() = 'App_Data'][@Name = 'Extra_Regular_Media_Link']/@Value">
        <xsl:element name="meta">
          <xsl:attribute name="name">Extra Regular Media Link</xsl:attribute>
          <xsl:attribute name="ml_handling">unique</xsl:attribute>
            <xsl:for-each select="../*[local-name() = 'App_Data'][@Name = 'Extra_Regular_Media_Link']">
              <xsl:variable name="fullStr" select="@Value"/>
              <xsl:call-template name="ParseTag">
                <xsl:with-param name="fullString" select="$fullStr"/>
              </xsl:call-template>
            </xsl:for-each>
        </xsl:element>
      </xsl:if>
    </xsl:for-each>
    <xsl:for-each select="//*[local-name() = 'Asset']/*[local-name() = 'Metadata']/*[local-name() = 'AMS'][@Asset_Class = 'title']">
      <xsl:if test="../*[local-name() = 'App_Data'][@Name = 'Subject_Tag']/@Value">
        <xsl:element name="meta">
          <xsl:attribute name="name">Subject Tag</xsl:attribute>
          <xsl:attribute name="ml_handling">unique</xsl:attribute>
            <xsl:for-each select="../*[local-name() = 'App_Data'][@Name = 'Subject_Tag']">
              <xsl:variable name="fullStr" select="@Value"/>
              <xsl:call-template name="ParseTag">
                <xsl:with-param name="fullString" select="$fullStr"/>
              </xsl:call-template>
            </xsl:for-each>
        </xsl:element>
      </xsl:if>
    </xsl:for-each>
    <xsl:for-each select="//*[local-name() = 'Asset']/*[local-name() = 'Metadata']/*[local-name() = 'AMS'][@Asset_Class = 'title']">
      <xsl:if test="../*[local-name() = 'App_Data'][@Name = 'Featured_Organisation']/@Value">
        <xsl:element name="meta">
          <xsl:attribute name="name">Featured Organisation</xsl:attribute>
          <xsl:attribute name="ml_handling">unique</xsl:attribute>
            <xsl:for-each select="../*[local-name() = 'App_Data'][@Name = 'Featured_Organisation']">
              <xsl:variable name="fullStr" select="@Value"/>
              <xsl:call-template name="ParseTag">
                <xsl:with-param name="fullString" select="$fullStr"/>
              </xsl:call-template>
            </xsl:for-each>
        </xsl:element>
      </xsl:if>
    </xsl:for-each>
    <xsl:for-each select="//*[local-name() = 'Asset']/*[local-name() = 'Metadata']/*[local-name() = 'AMS'][@Asset_Class = 'title']">
      <xsl:if test="../*[local-name() = 'App_Data'][@Name = 'Featured_Individual']/@Value">
        <xsl:element name="meta">
          <xsl:attribute name="name">Featured Individual</xsl:attribute>
          <xsl:attribute name="ml_handling">unique</xsl:attribute>
            <xsl:for-each select="../*[local-name() = 'App_Data'][@Name = 'Featured_Individual']">
              <xsl:variable name="fullStr" select="@Value"/>
              <xsl:call-template name="ParseTag">
                <xsl:with-param name="fullString" select="$fullStr"/>
              </xsl:call-template>
            </xsl:for-each>
        </xsl:element>
      </xsl:if>
    </xsl:for-each>
    <xsl:for-each select="//*[local-name() = 'Asset']/*[local-name() = 'Metadata']/*[local-name() = 'AMS'][@Asset_Class = 'title']">
      <xsl:if test="../*[local-name() = 'App_Data'][@Name = 'Featured_Channel']/@Value">
        <xsl:element name="meta">
          <xsl:attribute name="name">Featured Channel</xsl:attribute>
          <xsl:attribute name="ml_handling">unique</xsl:attribute>
            <xsl:for-each select="../*[local-name() = 'App_Data'][@Name = 'Featured_Channel']">
              <xsl:variable name="fullStr" select="@Value"/>
              <xsl:call-template name="ParseTag">
                <xsl:with-param name="fullString" select="$fullStr"/>
              </xsl:call-template>
            </xsl:for-each>
        </xsl:element>
      </xsl:if>
    </xsl:for-each>
    <xsl:for-each select="//*[local-name() = 'Asset']/*[local-name() = 'Metadata']/*[local-name() = 'AMS'][@Asset_Class = 'title']">
      <xsl:if test="../*[local-name() = 'App_Data'][@Name = 'School_Level']/@Value">
        <xsl:element name="meta">
          <xsl:attribute name="name">School Level</xsl:attribute>
          <xsl:attribute name="ml_handling">unique</xsl:attribute>
            <xsl:for-each select="../*[local-name() = 'App_Data'][@Name = 'School_Level']">
              <xsl:variable name="fullStr" select="@Value"/>
              <xsl:call-template name="ParseTag">
                <xsl:with-param name="fullString" select="$fullStr"/>
              </xsl:call-template>
            </xsl:for-each>
        </xsl:element>
      </xsl:if>
    </xsl:for-each>
    <xsl:for-each select="//*[local-name() = 'Asset']/*[local-name() = 'Metadata']/*[local-name() = 'AMS'][@Asset_Class = 'title']">
      <xsl:if test="../*[local-name() = 'App_Data'][@Name = 'School_Subject']/@Value">
        <xsl:element name="meta">
          <xsl:attribute name="name">School Subject</xsl:attribute>
          <xsl:attribute name="ml_handling">unique</xsl:attribute>
            <xsl:for-each select="../*[local-name() = 'App_Data'][@Name = 'School_Subject']">
              <xsl:variable name="fullStr" select="@Value"/>
              <xsl:call-template name="ParseTag">
                <xsl:with-param name="fullString" select="$fullStr"/>
              </xsl:call-template>
            </xsl:for-each>
        </xsl:element>
      </xsl:if>
    </xsl:for-each>
    <xsl:for-each select="//*[local-name() = 'Asset']/*[local-name() = 'Metadata']/*[local-name() = 'AMS'][@Asset_Class = 'title']">
      <xsl:if test="../*[local-name() = 'App_Data'][@Name = 'School_Chapter']/@Value">
        <xsl:element name="meta">
          <xsl:attribute name="name">Chapter</xsl:attribute>
          <xsl:attribute name="ml_handling">unique</xsl:attribute>
            <xsl:for-each select="../*[local-name() = 'App_Data'][@Name = 'School_Chapter']">
              <xsl:variable name="fullStr" select="@Value"/>
              <xsl:call-template name="ParseTag">
                <xsl:with-param name="fullString" select="$fullStr"/>
              </xsl:call-template>
            </xsl:for-each>
        </xsl:element>
      </xsl:if>
    </xsl:for-each>
  </xsl:template>

  <xsl:template name="ParseTag">
    <xsl:param name="fullString" />

    <xsl:choose>
      <xsl:when test="substring-after($fullString,';') != '' ">
        <xsl:call-template name="ParseTag">
          <xsl:with-param name="fullString" select="substring-after($fullString,';')" />
        </xsl:call-template>
        <xsl:call-template name="set_container_val">
          <xsl:with-param name="value" select="substring-before($fullString,';')"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <xsl:call-template name="set_container_val">
          <xsl:with-param name="value" select="$fullString"/>
        </xsl:call-template>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="set_container_val">
    <xsl:param name="value" />
    
    <xsl:element name="container">
      <xsl:element name="value">
        <xsl:attribute name="lang">eng</xsl:attribute>
        <xsl:value-of select="$value"/>
      </xsl:element>
    </xsl:element>
  </xsl:template>

</xsl:stylesheet>